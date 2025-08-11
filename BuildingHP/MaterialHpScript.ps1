# ===== CONFIG =====
# Folder containing *.BuildingMaterialDurability.json
$SpecPath      = "./Blueprints/BuildingMaterialDurability"

# Folder containing building JSON files
$BuildingsPath = "C:\Users\lukev\OneDrive\Documents\Timberborn\TimberDump\Objects"

# Sort order ($true = highest HP first)
$SortDescending = $true
# ==================

function Remove-JsonComments {
    param([Parameter(Mandatory)][string]$Text)
    # Remove /* block comments */
    $noBlock = $Text -replace '(?s)/\*.*?\*/',''
    # Remove // comments (full-line or trailing, but not :// in URLs)
    $lines = $noBlock -split "`n" | ForEach-Object {
        $line = $_
        if ($line -match '^\s*//') { '' } else { $line -replace '(?<!:)\s//.*$','' }
    }
    ($lines -join "`n")
}

# 1) Load durability specs: GoodId -> Durability
$durability = @{}
Get-ChildItem -Path $SpecPath -Filter *.json -Recurse | ForEach-Object {
    $raw = Get-Content $_.FullName -Raw
    $json = $null
    try {
        $json = $raw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        try {
            $json = (Remove-JsonComments -Text $raw) | ConvertFrom-Json -ErrorAction Stop
        } catch {
            Write-Warning "Spec parse failed, skipped: $($_.FullName)"
            return
        }
    }

    if ($null -ne $json -and $json.PSObject.Properties.Name -contains 'BuildingMaterialDurabilitySpec') {
        $spec = $json.BuildingMaterialDurabilitySpec
        $good = [string]$spec.GoodId
        $hp   = [double]$spec.Durability
        if ($good) { $durability[$good] = $hp }
    }
}

# 2) Scan buildings and compute HP = weighted average over known materials
$results = Get-ChildItem -Path $BuildingsPath -Filter *.json -Recurse |
    Sort-Object FullName |
    ForEach-Object {
        $file = $_
        $raw = Get-Content $file.FullName -Raw
        $json = $null
        try {
            $json = $raw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            try {
                $json = (Remove-JsonComments -Text $raw) | ConvertFrom-Json -ErrorAction Stop
            } catch {
                return
            }
        }
        if (-not $json) { return }
        if (-not ($json.PSObject.Properties.Name -contains 'BuildingSpec')) { return }

        $bs = $json.BuildingSpec
        if (-not $bs) { return }
        if (-not ($bs.PSObject.Properties.Name -contains '_buildingCost')) { return }

        $items = @($bs._buildingCost)
        if (-not $items -or $items.Count -eq 0) { return }

        $sumAmt = 0.0
        $sumAmtTimesDur = 0.0

        foreach ($it in $items) {
            $gid = [string]$it._goodId
            $amt = [double]$it._amount
            if ([string]::IsNullOrWhiteSpace($gid)) { continue }
            if (-not $durability.ContainsKey($gid)) { continue }

            $d = [double]$durability[$gid]
            $sumAmt        += $amt
            $sumAmtTimesDur += ($amt * $d)
        }

        if ($sumAmt -le 0) { return }  # no known materials -> ignore

        $hp = $sumAmtTimesDur / $sumAmt  # weighted average
        $hpRounded = [int][math]::Round($hp, 0)

        [pscustomobject]@{
            FileName = $file.Name
            HP       = $hpRounded
        }
    }

# 3) Sort and print FileName<TAB>HP
if ($SortDescending) {
    $sorted = $results | Sort-Object -Property HP, FileName -Descending
} else {
    $sorted = $results | Sort-Object -Property HP, FileName
}

$sorted | ForEach-Object {
    "{0}`t{1}" -f $_.FileName, $_.HP
}