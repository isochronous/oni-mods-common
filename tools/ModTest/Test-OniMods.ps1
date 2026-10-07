<#
.SYNOPSIS
  Launch Oxygen Not Included with only the mods under development enabled, then restore the normal loadout.

.DESCRIPTION
  Backs up mods.json, rewrites it so only the given local mods are enabled (for both the base game
  and Spaced Out), launches the game through Steam, waits for it to exit, and puts the backup back.
  Local mods that have no mods.json entry yet (freshly built ones) get one. Nothing else is touched:
  mod folders, configs and saves stay as they are.

.PARAMETER Mods
  Local mod folder names to enable. Defaults to the data-dump mod plus the mods under development.
.PARAMETER DryRun
  Show the resulting loadout and write it to the scratch folder instead of touching the game.
#>
[CmdletBinding()]
param(
    [string[]]$Mods = @(
        'OniDataDump',
        'Bitshifter',
        'MotionSensorRange',
        'MultichannelCritterSensor',
        'NaturalBackwalls',
        'NoMagicFridges',
        'SmartWeightPlate',
        'StoragePodRedux',
        'SupplyClosetUnlocked',
        'SweepZones'
    ),
    [switch]$DryRun
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --- self-contained helpers -------------------------------------------------------------------
$OniAppId    = 457140
$ProfileLink = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Klei\OxygenNotIncluded'

function Test-OniRunning {
    return [bool](Get-Process -Name 'OxygenNotIncluded', 'Restarter' -ErrorAction SilentlyContinue)
}

function Assert-OniNotRunning {
    if (Test-OniRunning) { throw 'Oxygen Not Included (or its Restarter) is running. Close it first.' }
}

function Get-DotNetStringHash {
    # Mono/.NET Framework legacy string hash; the game stores it as a local mod's label.version (hash of the folder name).
    param([string]$s)
    [uint64]$mask = 4294967295
    [uint64]$h1 = 5381; [uint64]$h2 = 5381
    $cs = [int[]][char[]]$s + 0 + 0
    $i = 0
    while ($cs[$i] -ne 0) {
        $h1 = ((($h1 -shl 5) + $h1) -bxor [uint64]$cs[$i]) -band $mask
        if ($cs[$i + 1] -eq 0) { break }
        $h2 = ((($h2 -shl 5) + $h2) -bxor [uint64]$cs[$i + 1]) -band $mask
        $i += 2
    }
    [uint64]$r = ($h1 + (($h2 * 1566083941) -band $mask)) -band $mask
    return [BitConverter]::ToInt32([BitConverter]::GetBytes([uint32]$r), 0)
}
# -----------------------------------------------------------------------------------------------

$modsDir  = Join-Path $ProfileLink 'mods'
$modsJson = Join-Path $modsDir 'mods.json'
$localDir = Join-Path $modsDir 'local'
$dlcs     = @('EXPANSION1_ID', '')
$utf8     = New-Object System.Text.UTF8Encoding $false

if (-not $DryRun) { Assert-OniNotRunning }
if (-not (Test-Path -LiteralPath $modsJson)) { throw "No mods.json at $modsJson" }

$data = [IO.File]::ReadAllText($modsJson, $utf8) | ConvertFrom-Json
$wanted = @{}
foreach ($m in $Mods) { $wanted[$m] = $false }

# Disable everything, enable the wanted local mods.
foreach ($entry in $data.mods) {
    $id = $entry.label.id
    if ($entry.label.distribution_platform -eq 0 -and $wanted.ContainsKey($id)) {
        $entry.enabledForDlc = @($dlcs)
        $wanted[$id] = $true
    } else {
        $entry.enabledForDlc = @()
    }
}

# Add entries for wanted local folders the game has not registered yet.
$list = New-Object System.Collections.ArrayList
foreach ($entry in $data.mods) { [void]$list.Add($entry) }
foreach ($id in @($wanted.Keys)) {
    if ($wanted[$id]) { continue }
    $folder = Join-Path $localDir $id
    if (-not (Test-Path -LiteralPath $folder)) {
        Write-Warning "No local mod folder '$id' under $localDir; skipped"
        continue
    }
    $title = $id; $staticID = $id
    $yaml = Join-Path $folder 'mod.yaml'
    if (Test-Path -LiteralPath $yaml) {
        $text = Get-Content -LiteralPath $yaml -Raw
        $m = [regex]::Match($text, '(?m)^title:\s*"?([^"\r\n]+)"?');    if ($m.Success) { $title = $m.Groups[1].Value.Trim() }
        $m = [regex]::Match($text, '(?m)^staticID:\s*"?([^"\r\n]+)"?'); if ($m.Success) { $staticID = $m.Groups[1].Value.Trim() }
    }
    [void]$list.Add([pscustomobject]([ordered]@{
        label = [ordered]@{ distribution_platform = 0; id = $id; title = $title; version = Get-DotNetStringHash $id }
        status = 1; enabled = $false; enabledForDlc = @($dlcs); crash_count = 0; reinstall_path = $null; staticID = $staticID
    }))
    $wanted[$id] = $true
    Write-Host "Registered new local mod '$id' ($title)"
}
$data.mods = $list.ToArray()
$json = $data | ConvertTo-Json -Depth 6

$enabled = @($data.mods | Where-Object { $_.enabledForDlc.Count -gt 0 } | ForEach-Object { $_.label.title })
Write-Host "Test loadout ($($enabled.Count) mods): $($enabled -join ', ')"

if ($DryRun) {
    $out = Join-Path $env:TEMP 'oni-modtest.mods.json'
    [IO.File]::WriteAllText($out, $json, $utf8)
    Write-Host "Dry run: loadout written to $out, game not launched."
    return
}

$backup = "$modsJson.modtest-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item -LiteralPath $modsJson -Destination $backup
Write-Host "Backed up mods.json to $backup"
try {
    [IO.File]::WriteAllText($modsJson, $json, $utf8)
    Write-Host 'Launching Oxygen Not Included through Steam...'
    Start-Process "steam://rungameid/$OniAppId"
    $deadline = (Get-Date).AddSeconds(180)
    while (-not (Test-OniRunning) -and (Get-Date) -lt $deadline) { Start-Sleep 2 }
    if (-not (Test-OniRunning)) { throw 'The game did not start within 3 minutes.' }
    Write-Host 'Game running; waiting for it to exit...'
    $quietSince = $null
    while ($true) {
        if (Test-OniRunning) { $quietSince = $null }
        elseif ($null -eq $quietSince) { $quietSince = Get-Date }
        elseif (((Get-Date) - $quietSince).TotalSeconds -ge 20) { break }
        Start-Sleep 3
    }
}
finally {
    if (Test-OniRunning) {
        Write-Warning "The game is still running; mods.json NOT restored. Restore by hand when it closes:`n  Copy-Item '$backup' '$modsJson'"
    } else {
        Copy-Item -LiteralPath $backup -Destination $modsJson -Force
        Write-Host "Restored the normal loadout from $backup"
    }
}
