$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
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
$lines.Add("            .source { margin: 0 0 24px; color: color-mix(in srgb, CanvasText 70%, Canvas); }")
$lines.Add("            .table-wrap { overflow-x: auto; border: 1px solid color-mix(in srgb, CanvasText 18%, Canvas); border-radius: 6px; }")
$lines.Add("            table { width: 100%; border-collapse: collapse; min-width: 980px; }")
$lines.Add("            th, td { padding: 8px 10px; border-bottom: 1px solid color-mix(in srgb, CanvasText 14%, Canvas); vertical-align: top; }")
$lines.Add("            th { position: sticky; top: 0; background: Canvas; text-align: left; }")
$lines.Add("            tbody tr:nth-child(even) td { background: color-mix(in srgb, CanvasText 4%, Canvas); }")
$lines.Add("            .number { text-align: right; white-space: nowrap; }")
$lines.Add("            .stage { font-weight: 600; }")
$lines.Add("            .tier { white-space: nowrap; }")
$lines.Add("        </style>")
$lines.Add("    </head>")
$lines.Add("    <body>")
$lines.Add("        <h1>Unstable Core Challenge Difficulty Tables</h1>")
$lines.Add("        <p class=""source"">Generated from <code>*.blueprint.json</code> files in <code>$(Encode-Html $specsDir)</code>.</p>")

foreach ($difficulty in $difficulties) {
    $lines.Add("        <h2>$(Encode-Html $difficulty.Id)</h2>")
    $lines.Add("        <div class=""table-wrap"">")
    $lines.Add("            <table>")
    $lines.Add("                <thead>")
    $lines.Add("                    <tr>")
    $lines.Add("                        <th>Stage</th>")
    $lines.Add("                        <th>Min cycle</th>")
    $lines.Add("                        <th>Max bombs</th>")
    $lines.Add("                        <th>Payment entries</th>")
    $lines.Add("                        <th>Science chance</th>")
    $lines.Add("                        <th>Science payment</th>")
    $lines.Add("                        <th>Days</th>")
    $lines.Add("                        <th>Tier</th>")
    $lines.Add("                        <th>Count</th>")
    $lines.Add("                        <th>Amount</th>")
    $lines.Add("                    </tr>")
    $lines.Add("                </thead>")
    $lines.Add("                <tbody>")

    for ($i = 0; $i -lt $difficulty.Stages.Count; $i++) {
        $stage = $difficulty.Stages[$i]
        $payments = @(Get-PropertyValue -Object $stage -Name "ChallengeStagePayment")

        if ($null -eq $payments[0]) {
            $payments = @($null)
        }

        $rowSpan = $payments.Count

        for ($paymentIndex = 0; $paymentIndex -lt $payments.Count; $paymentIndex++) {
            $payment = $payments[$paymentIndex]

            $lines.Add("                    <tr>")

            if ($paymentIndex -eq 0) {
                Add-Cell -Lines $lines -Value "$($i + 1)" -RowSpan $rowSpan -Class "number stage"
                Add-Cell -Lines $lines -Value "$($stage.MinCycle)" -RowSpan $rowSpan -Class "number"
                Add-Cell -Lines $lines -Value "$($stage.MaxBombs)" -RowSpan $rowSpan -Class "number"
                Add-Cell -Lines $lines -Value (Format-RangeObject -Object (Get-PropertyValue -Object $stage -Name "PaymentEntries")) -RowSpan $rowSpan -Class "number"
                Add-Cell -Lines $lines -Value (Format-Percent -Value (Get-PropertyValue -Object $stage -Name "ScienceChance")) -RowSpan $rowSpan -Class "number"
                Add-Cell -Lines $lines -Value (Format-SciencePayment -Stage $stage) -RowSpan $rowSpan -Class "number"
                Add-Cell -Lines $lines -Value (Format-Days -Stage $stage) -RowSpan $rowSpan -Class "number"
            }

            if ($null -eq $payment) {
                Add-Cell -Lines $lines -Value "-" -Class "tier"
                Add-Cell -Lines $lines -Value "-" -Class "number"
                Add-Cell -Lines $lines -Value "-" -Class "number"
            }
            else {
                Add-Cell -Lines $lines -Value "Tier $paymentIndex" -Class "tier"
                Add-Cell -Lines $lines -Value (Format-Range `
                    -Min (Get-PropertyValue -Object $payment -Name "MinCount") `
                    -Max (Get-PropertyValue -Object $payment -Name "MaxCount")) -Class "number"
                Add-Cell -Lines $lines -Value (Format-Range `
                    -Min (Get-PropertyValue -Object $payment -Name "MinAmount") `
                    -Max (Get-PropertyValue -Object $payment -Name "MaxAmount")) -Class "number"
            }

            $lines.Add("                    </tr>")
        }
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
