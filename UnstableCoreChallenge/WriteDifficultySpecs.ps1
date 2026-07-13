$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$balancePath = Join-Path $scriptDir "balance.json"
$balance = Get-Content -Path $balancePath -Raw | ConvertFrom-Json
$specsDir = Join-Path $scriptDir $balance.SpecsDirectory

function Get-PropertyValue {
    param(
        [Parameter(Mandatory = $true)]
        [object] $Object,

        [Parameter(Mandatory = $true)]
        [string] $Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]

    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function ConvertTo-RoundedInt {
    param(
        [double] $Value,

        [int] $Minimum = 0
    )

    return [Math]::Max($Minimum, [int] [Math]::Round($Value))
}

function ConvertTo-RoundedAmount {
    param(
        [double] $Value
    )

    $rounded = [int] ([Math]::Round($Value / 5) * 5)

    return [Math]::Max(1, $rounded)
}

function ConvertTo-ScaledRange {
    param(
        [object] $Range,

        [double] $Multiplier,

        [switch] $Amount
    )

    if ($null -eq $Range) {
        return $null
    }

    $min = [double] $Range.Min * $Multiplier
    $max = [double] $Range.Max * $Multiplier

    if ($Amount) {
        $min = ConvertTo-RoundedAmount -Value $min
        $max = ConvertTo-RoundedAmount -Value $max
    }
    else {
        $min = ConvertTo-RoundedInt -Value $min
        $max = ConvertTo-RoundedInt -Value $max
    }

    if ($max -lt $min) {
        $max = $min
    }

    [ordered]@{
        Min = $min
        Max = $max
    }
}

function Get-TierRule {
    param(
        [int] $Tier
    )

    foreach ($rule in $balance.TierRules) {
        if ($rule.Tier -eq $Tier) {
            return $rule
        }
    }

    throw "No tier rule found for tier $Tier."
}

function Get-TierAllowed {
    param(
        [int] $Tier,

        [int] $MinCycle,

        [object] $Difficulty
    )

    $rule = Get-TierRule -Tier $Tier
    $requiredCycle = [int] $rule.MinCycle + [int] $Difficulty.TierGraceCycles

    return $MinCycle -ge $requiredCycle
}

function Get-ScaledPayment {
    param(
        [object] $Payment,

        [int] $Tier,

        [object] $Difficulty
    )

    $tierRule = Get-TierRule -Tier $Tier
    $tierMultiplier = 1

    if ($Difficulty.Id -ne "Normal") {
        $tierMultiplier = [double] $tierRule.FavorMultiplier
    }

    $amountMultiplier = [double] $Difficulty.AmountMultiplier * $tierMultiplier

    $minAmount = ConvertTo-RoundedAmount -Value ([double] $Payment.MinAmount * $amountMultiplier)
    $maxAmount = ConvertTo-RoundedAmount -Value ([double] $Payment.MaxAmount * $amountMultiplier)

    if ($maxAmount -lt $minAmount) {
        $maxAmount = $minAmount
    }

    [ordered]@{
        MinCount = [int] $Payment.MinCount
        MaxCount = [int] $Payment.MaxCount
        MinAmount = $minAmount
        MaxAmount = $maxAmount
    }
}

function Get-ScaledPaymentEntries {
    param(
        [object] $PaymentEntries,

        [int] $PaymentCount,

        [object] $Difficulty
    )

    if ($null -eq $PaymentEntries -or $PaymentCount -eq 0) {
        return $null
    }

    $min = [int] $PaymentEntries.Min + [int] $Difficulty.PaymentEntryOffset
    $max = [int] $PaymentEntries.Max + [int] $Difficulty.PaymentEntryOffset

    $min = [Math]::Max(1, [Math]::Min($min, $PaymentCount))
    $max = [Math]::Max($min, [Math]::Min($max, $PaymentCount))

    [ordered]@{
        Min = $min
        Max = $max
    }
}

function Get-ScaledScienceChance {
    param(
        [object] $Stage,

        [object] $Difficulty
    )

    $scienceChance = Get-PropertyValue -Object $Stage -Name "ScienceChance"

    if ($null -eq $scienceChance) {
        return $null
    }

    return [Math]::Min([double] 1, [Math]::Round([double] $scienceChance * [double] $Difficulty.ScienceChanceMultiplier, 2))
}

function Get-ScaledStage {
    param(
        [object] $Stage,

        [object] $Difficulty
    )

    $minCycleOffset = Get-PropertyValue -Object $Difficulty -Name "MinCycleOffset"

    if ($null -eq $minCycleOffset) {
        $minCycleOffset = 0
    }

    $minCycle = [Math]::Max(0, [int] $Stage.MinCycle + [int] $minCycleOffset)
    $maxBombs = [int] $Stage.MaxBombs

    if ($minCycle -ge [int] $Difficulty.BombOffsetMinCycle) {
        $maxBombs += [int] $Difficulty.BombOffset
    }

    $maxBombs = [Math]::Max(0, $maxBombs)
    $payments = @()
    $basePayments = @(Get-PropertyValue -Object $Stage -Name "ChallengeStagePayment")

    if ($null -eq $basePayments[0]) {
        $basePayments = @()
    }

    for ($tier = 0; $tier -lt $basePayments.Count; $tier++) {
        if (-not (Get-TierAllowed -Tier $tier -MinCycle $minCycle -Difficulty $Difficulty)) {
            break
        }

        $payments += Get-ScaledPayment -Payment $basePayments[$tier] -Tier $tier -Difficulty $Difficulty
    }

    $result = [ordered]@{
        MinCycle = $minCycle
        MaxBombs = $maxBombs
    }

    $paymentEntries = Get-ScaledPaymentEntries `
        -PaymentEntries (Get-PropertyValue -Object $Stage -Name "PaymentEntries") `
        -PaymentCount $payments.Count `
        -Difficulty $Difficulty

    if ($null -ne $paymentEntries) {
        $result.PaymentEntries = $paymentEntries
    }

    $scienceChance = Get-ScaledScienceChance -Stage $Stage -Difficulty $Difficulty

    if ($null -ne $scienceChance) {
        $result.ScienceChance = $scienceChance
        $result.SciencePayment = ConvertTo-ScaledRange `
            -Range (Get-PropertyValue -Object $Stage -Name "SciencePayment") `
            -Multiplier ([double] $Difficulty.SciencePaymentMultiplier) `
            -Amount
    }

    if ($payments.Count -gt 0) {
        $result.Payments = $payments
    }

    $daysMin = Get-PropertyValue -Object $Stage -Name "DaysMin"
    $daysMax = Get-PropertyValue -Object $Stage -Name "DaysMax"

    if ($null -ne $daysMin -and $null -ne $daysMax) {
        $result.DaysMin = ConvertTo-RoundedInt -Value ([double] $daysMin * [double] $Difficulty.DaysMultiplier) -Minimum 1
        $result.DaysMax = ConvertTo-RoundedInt -Value ([double] $daysMax * [double] $Difficulty.DaysMultiplier) -Minimum $result.DaysMin
    }

    return $result
}

function Get-CollapsedStages {
    param(
        [object[]] $Stages
    )

    $byCycle = [ordered]@{}

    foreach ($stage in $Stages) {
        $byCycle["$($stage.MinCycle)"] = $stage
    }

    foreach ($key in $byCycle.Keys) {
        $byCycle[$key]
    }
}

function Copy-Stage {
    param(
        [object] $Stage
    )

    $copy = [ordered]@{}

    foreach ($key in $Stage.Keys) {
        $copy[$key] = $Stage[$key]
    }

    return $copy
}

function Add-EndlessStages {
    param(
        [object[]] $Stages,

        [object] $Difficulty
    )

    $endlessBombs = Get-PropertyValue -Object $Difficulty -Name "EndlessBombs"

    if ($null -eq $endlessBombs -or $Stages.Count -eq 0) {
        return $Stages
    }

    $result = [System.Collections.Generic.List[object]]::new()

    foreach ($stage in $Stages) {
        $result.Add($stage)
    }

    $template = $Stages[-1]
    $nextCycle = [int] $template.MinCycle + [int] $endlessBombs.EveryCycles
    $nextBombs = [int] $template.MaxBombs + 1

    while ($nextBombs -le [int] $endlessBombs.MaxBombs) {
        $stage = Copy-Stage -Stage $template
        $stage.MinCycle = $nextCycle
        $stage.MaxBombs = $nextBombs
        $result.Add($stage)

        $nextCycle += [int] $endlessBombs.EveryCycles
        $nextBombs++
    }

    return $result.ToArray()
}

foreach ($difficulty in $balance.Difficulties) {
    $stages = @(foreach ($stage in $balance.NormalStages) {
        Get-ScaledStage -Stage $stage -Difficulty $difficulty
    })
    $stages = @(Get-CollapsedStages -Stages $stages)
    $stages = @(Add-EndlessStages -Stages $stages -Difficulty $difficulty)

    $blueprint = [ordered]@{
        UnstableCoreChallengeDifficultySpec = [ordered]@{
            Id = $difficulty.Id
            Order = [int] $difficulty.Order
            NameLoc = $difficulty.NameLoc
            Stages = @($stages)
        }
    }

    if ($difficulty.Id -eq "Normal") {
        $blueprint.UnstableCoreChallengeDifficultySpec.IsDefault = $true
    }

    $outputPath = Join-Path $specsDir "$($difficulty.Id).blueprint.json"
    $json = $blueprint | ConvertTo-Json -Depth 20
    Set-Content -Path $outputPath -Value $json -Encoding UTF8
    Write-Output "Wrote $outputPath"
}
