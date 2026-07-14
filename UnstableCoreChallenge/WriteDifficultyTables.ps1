$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$balancePath = Join-Path $scriptDir "balance.json"
$balance = Get-Content -Path $balancePath -Raw | ConvertFrom-Json
$specsDir = Join-Path $scriptDir "Blueprints\UnstableCoreChallengeDifficultySpecs"
$outputPath = Join-Path $scriptDir "Tables.html"

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

function Format-Range {
    param(
        [object] $Min,
        [object] $Max
    )

    if ($null -eq $Min -and $null -eq $Max) {
        return "-"
    }

    if ($Min -eq $Max) {
        return "$Min"
    }

    return "$Min-$Max"
}

function Format-RangeObject {
    param(
        [object] $Object
    )

    if ($null -eq $Object) {
        return "-"
    }

    $min = Get-PropertyValue -Object $Object -Name "Min"
    $max = Get-PropertyValue -Object $Object -Name "Max"

    return Format-Range -Min $min -Max $max
}

function Get-AverageRange {
    param(
        [object] $Min,
        [object] $Max
    )

    if ($null -eq $Min -and $null -eq $Max) {
        return 0
    }

    if ($null -eq $Min) {
        return [double] $Max
    }

    if ($null -eq $Max) {
        return [double] $Min
    }

    return ([double] $Min + [double] $Max) / 2
}

function Get-AverageRangeObject {
    param(
        [object] $Object
    )

    if ($null -eq $Object) {
        return 0
    }

    return Get-AverageRange `
        -Min (Get-PropertyValue -Object $Object -Name "Min") `
        -Max (Get-PropertyValue -Object $Object -Name "Max")
}

function Format-Percent {
    param(
        [object] $Value
    )

    if ($null -eq $Value) {
        return "-"
    }

    return "{0:P0}" -f [double] $Value
}

function Format-SciencePayment {
    param(
        [object] $Stage
    )

    $payment = Get-PropertyValue -Object $Stage -Name "SciencePayment"

    if ($null -eq $payment) {
        return "-"
    }

    return Format-RangeObject -Object $payment
}

function Format-Days {
    param(
        [object] $Stage
    )

    return Format-Range `
        -Min (Get-PropertyValue -Object $Stage -Name "DaysMin") `
        -Max (Get-PropertyValue -Object $Stage -Name "DaysMax")
}

function Format-Rewards {
    param(
        [object] $Stage
    )

    $rewards = @(Get-PropertyValue -Object $Stage -Name "Rewards")

    if ($null -eq $rewards[0]) {
        return "-"
    }

    return ($rewards | ForEach-Object {
        $amount = Format-RangeObject -Object (Get-PropertyValue -Object $_ -Name "Amount")
        "$($_.GoodId): $amount"
    }) -join ", "
}

function Format-TierPayment {
    param(
        [object] $Payment
    )

    if ($null -eq $Payment) {
        return "-"
    }

    $count = Format-Range `
        -Min (Get-PropertyValue -Object $Payment -Name "MinCount") `
        -Max (Get-PropertyValue -Object $Payment -Name "MaxCount")
    $amount = Format-Range `
        -Min (Get-PropertyValue -Object $Payment -Name "MinAmount") `
        -Max (Get-PropertyValue -Object $Payment -Name "MaxAmount")

    return "$count x $amount"
}

function Get-TierWeight {
    param(
        [int] $Tier
    )

    foreach ($rule in $balance.TierRules) {
        if ($rule.Tier -eq $Tier) {
            return [double] $rule.ScoreWeight
        }
    }

    return 1
}

function Get-TierScore {
    param(
        [object] $Payment,

        [int] $Tier
    )

    if ($null -eq $Payment) {
        return 0
    }

    $count = Get-AverageRange `
        -Min (Get-PropertyValue -Object $Payment -Name "MinCount") `
        -Max (Get-PropertyValue -Object $Payment -Name "MaxCount")
    $amount = Get-AverageRange `
        -Min (Get-PropertyValue -Object $Payment -Name "MinAmount") `
        -Max (Get-PropertyValue -Object $Payment -Name "MaxAmount")

    return $count * $amount * (Get-TierWeight -Tier $Tier)
}

function Get-StageScore {
    param(
        [object] $Stage,

        [object[]] $Payments
    )

    $maxBombs = Get-PropertyValue -Object $Stage -Name "MaxBombs"
    $bombPressure = [double] $maxBombs * [double] $balance.Score.BombWeight
    $paymentEntries = Get-AverageRangeObject -Object (Get-PropertyValue -Object $Stage -Name "PaymentEntries")
    $tierTotal = 0

    for ($tier = 0; $tier -lt 6; $tier++) {
        $tierTotal += Get-TierScore -Payment $Payments[$tier] -Tier $tier
    }

    $paymentPressure = $paymentEntries * $tierTotal
    $scienceChance = Get-PropertyValue -Object $Stage -Name "ScienceChance"
    $sciencePayment = Get-AverageRangeObject -Object (Get-PropertyValue -Object $Stage -Name "SciencePayment")
    $sciencePressure = [double] $scienceChance * $sciencePayment * [double] $balance.Score.ScienceWeight
    $averageDays = Get-AverageRange `
        -Min (Get-PropertyValue -Object $Stage -Name "DaysMin") `
        -Max (Get-PropertyValue -Object $Stage -Name "DaysMax")
    $timePressure = 0

    if ($averageDays -gt 0) {
        $timePressure = [double] $balance.Score.TimeWeight / $averageDays
    }

    return [Math]::Round($bombPressure + $paymentPressure + $sciencePressure + $timePressure)
}

function Format-ScoreIncrease {
    param(
        [double] $Score,

        [object] $PreviousScore
    )

    if ($null -eq $PreviousScore) {
        return "-"
    }

    $increase = [Math]::Round($Score - [double] $PreviousScore)

    if ($increase -gt 0) {
        return "+$increase"
    }

    return "$increase"
}

function Encode-Html {
    param(
        [object] $Value
    )

    return [System.Net.WebUtility]::HtmlEncode("$Value")
}

function Add-Cell {
    param(
        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.List[string]] $Lines,

        [Parameter(Mandatory = $true)]
        [string] $Value,

        [int] $RowSpan = 1,

        [string] $Class = ""
    )

    $rowSpanAttribute = ""
    $classAttribute = ""

    if ($RowSpan -gt 1) {
        $rowSpanAttribute = " rowspan=""$RowSpan"""
    }

    if ($Class -ne "") {
        $classAttribute = " class=""$Class"""
    }

    $Lines.Add("                            <td$classAttribute$rowSpanAttribute>$(Encode-Html $Value)</td>")
}

$difficulties = Get-ChildItem -Path $specsDir -Filter "*.blueprint.json" |
    ForEach-Object {
        $json = Get-Content -Path $_.FullName -Raw | ConvertFrom-Json
        $spec = $json.UnstableCoreChallengeDifficultySpec

        [pscustomobject]@{
            Id = $spec.Id
            Order = $spec.Order
            NameLoc = $spec.NameLoc
            Stages = @($spec.Stages)
        }
    } |
    Sort-Object -Property Order, Id

$tierWeights = ($balance.TierRules |
    Sort-Object -Property Tier |
    ForEach-Object { $_.ScoreWeight }) -join ", "

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("<!doctype html>")
$lines.Add("<html lang=""en"">")
$lines.Add("    <head>")
$lines.Add("        <meta charset=""utf-8"">")
$lines.Add("        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">")
$lines.Add("        <title>Unstable Core Challenge Difficulty Tables</title>")
$lines.Add("        <style>")
$lines.Add("            :root { color-scheme: light dark; font-family: Segoe UI, Arial, sans-serif; }")
$lines.Add("            body { margin: 24px; background: Canvas; color: CanvasText; }")
$lines.Add("            h1 { margin: 0 0 8px; font-size: 28px; }")
$lines.Add("            h2 { margin: 32px 0 12px; font-size: 22px; }")
$lines.Add("            .source, .formula { margin: 0 0 12px; color: color-mix(in srgb, CanvasText 70%, Canvas); }")
$lines.Add("            .formula { max-width: 1120px; line-height: 1.45; }")
$lines.Add("            .table-wrap { overflow-x: auto; border: 1px solid color-mix(in srgb, CanvasText 18%, Canvas); border-radius: 6px; }")
$lines.Add("            table { width: 100%; border-collapse: collapse; min-width: 1380px; }")
$lines.Add("            th, td { padding: 8px 10px; border-bottom: 1px solid color-mix(in srgb, CanvasText 14%, Canvas); vertical-align: top; }")
$lines.Add("            th { position: sticky; top: 0; background: Canvas; text-align: left; }")
$lines.Add("            tbody tr:nth-child(even) td { background: color-mix(in srgb, CanvasText 4%, Canvas); }")
$lines.Add("            .number { text-align: right; white-space: nowrap; }")
$lines.Add("            .cycle { font-weight: 600; }")
$lines.Add("            .score { font-weight: 700; }")
$lines.Add("            .tier { white-space: nowrap; font-family: Consolas, monospace; }")
$lines.Add("            .reward { white-space: nowrap; }")
$lines.Add("        </style>")
$lines.Add("    </head>")
$lines.Add("    <body>")
$lines.Add("        <h1>Unstable Core Challenge Difficulty Tables</h1>")
$lines.Add("        <p class=""source"">Generated from <code>*.blueprint.json</code> files in <code>$(Encode-Html $specsDir)</code>.</p>")
$lines.Add("        <p class=""formula"">Score = bombs x $($balance.Score.BombWeight) + average payment entries x weighted tier payments + science chance x average science payment x $($balance.Score.ScienceWeight) + $($balance.Score.TimeWeight) / average days. Tier weights: $tierWeights.</p>")

foreach ($difficulty in $difficulties) {
    $lines.Add("        <h2>$(Encode-Html $difficulty.Id)</h2>")
    $lines.Add("        <div class=""table-wrap"">")
    $lines.Add("            <table>")
    $lines.Add("                <thead>")
    $lines.Add("                    <tr>")
    $lines.Add("                        <th>Min cycle</th>")
    $lines.Add("                        <th>Score</th>")
    $lines.Add("                        <th>Increase</th>")
    $lines.Add("                        <th>Tier 0</th>")
    $lines.Add("                        <th>Tier 1</th>")
    $lines.Add("                        <th>Tier 2</th>")
    $lines.Add("                        <th>Tier 3</th>")
    $lines.Add("                        <th>Tier 4</th>")
    $lines.Add("                        <th>Tier 5</th>")
    $lines.Add("                        <th>Payment entries</th>")
    $lines.Add("                        <th>Max bombs</th>")
    $lines.Add("                        <th>Science chance</th>")
    $lines.Add("                        <th>Science payment</th>")
    $lines.Add("                        <th>Days</th>")
    $lines.Add("                        <th>Rewards</th>")
    $lines.Add("                    </tr>")
    $lines.Add("                </thead>")
    $lines.Add("                <tbody>")
    $previousScore = $null

    for ($i = 0; $i -lt $difficulty.Stages.Count; $i++) {
        $stage = $difficulty.Stages[$i]
        $payments = @(Get-PropertyValue -Object $stage -Name "Payments")

        if ($null -eq $payments[0]) {
            $payments = @()
        }

        $score = Get-StageScore -Stage $stage -Payments $payments
        $increase = Format-ScoreIncrease -Score $score -PreviousScore $previousScore
        $previousScore = $score

        $lines.Add("                    <tr>")
        Add-Cell -Lines $lines -Value "$($stage.MinCycle)" -Class "number cycle"
        Add-Cell -Lines $lines -Value "$score" -Class "number score"
        Add-Cell -Lines $lines -Value $increase -Class "number"

        for ($tier = 0; $tier -lt 6; $tier++) {
            Add-Cell -Lines $lines -Value (Format-TierPayment -Payment $payments[$tier]) -Class "tier"
        }

        Add-Cell -Lines $lines -Value (Format-RangeObject -Object (Get-PropertyValue -Object $stage -Name "PaymentEntries")) -Class "number"
        Add-Cell -Lines $lines -Value "$($stage.MaxBombs)" -Class "number"
        Add-Cell -Lines $lines -Value (Format-Percent -Value (Get-PropertyValue -Object $stage -Name "ScienceChance")) -Class "number"
        Add-Cell -Lines $lines -Value (Format-SciencePayment -Stage $stage) -Class "number"
        Add-Cell -Lines $lines -Value (Format-Days -Stage $stage) -Class "number"
        Add-Cell -Lines $lines -Value (Format-Rewards -Stage $stage) -Class "reward"
        $lines.Add("                    </tr>")
    }

    $lines.Add("                </tbody>")
    $lines.Add("            </table>")
    $lines.Add("        </div>")
}

$lines.Add("    </body>")
$lines.Add("</html>")

$html = $lines -join [Environment]::NewLine
Set-Content -Path $outputPath -Value $html -Encoding UTF8

Write-Output "Wrote $outputPath"
