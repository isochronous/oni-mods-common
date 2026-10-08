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
.PARAMETER Omit
  Local mod folder names to leave disabled even though they are in the list, e.g. -Omit VentFreezeFix to
  check that a bug still happens without the fix.
.PARAMETER Only
  Enable just this one local mod and nothing else, e.g. -Only VentFreezeFix to test a mod in isolation.
  Replaces the list; cannot be combined with -Mods or -Omit.
.PARAMETER NoLocal
  Enable no local mods at all, only the -Steam items, e.g. -NoLocal -Steam 3816086407 to see how a
  Workshop mod behaves on its own. Cannot be combined with -Mods, -Omit or -Only.
.PARAMETER Steam
  Workshop item ids to enable as well, e.g. -Steam 3816086407. The item must be subscribed. One the
  game has not registered yet (subscribed since its last launch) is downloaded through the Steam
  client with tools/WorkshopUpload, unpacked into mods/Steam/<id> and given a mods.json entry, the
  way the game itself installs it at launch.
.PARAMETER KeepDumps
  Keep the oni-data-dump.*.json files the data-dump mod writes during the session. By default they are
  deleted when the game exits, along with the mods.json backup once it has been restored.
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
        'SweepZones',
        'VentFreezeFix'
    ),
    [string[]]$Omit = @(),
    [string]$Only,
    [string[]]$Steam = @(),
    [switch]$NoLocal,
    [switch]$KeepDumps,
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

if ($Only) {
    if ($PSBoundParameters.ContainsKey('Mods') -or $Omit.Count -gt 0) { throw "-Only cannot be combined with -Mods or -Omit" }
    $Mods = @($Only)
}
if ($NoLocal) {
    if ($PSBoundParameters.ContainsKey('Mods') -or $Omit.Count -gt 0 -or $Only) { throw "-NoLocal cannot be combined with -Mods, -Omit or -Only" }
    $Mods = @()
}

$data = [IO.File]::ReadAllText($modsJson, $utf8) | ConvertFrom-Json
$wanted = @{}
foreach ($m in $Mods) { if ($Omit -notcontains $m) { $wanted[$m] = $false } }
foreach ($m in $Omit) { if ($Mods -notcontains $m) { Write-Warning "-Omit '$m' is not in the mod list; nothing to omit" } }

# Disable everything, enable the wanted local mods and Workshop items.
$steamFound = @{}
foreach ($entry in $data.mods) {
    $id = $entry.label.id
    if ($entry.label.distribution_platform -eq 0 -and $wanted.ContainsKey($id)) {
        $entry.enabledForDlc = @($dlcs)
        $wanted[$id] = $true
    } elseif ($entry.label.distribution_platform -eq 1 -and $Steam -contains $id) {
        $entry.enabledForDlc = @($dlcs)
        $steamFound[$id] = $entry.label.title
    } else {
        $entry.enabledForDlc = @()
    }
}

# Add entries for wanted Workshop items the game has not registered yet: download through the Steam
# client (what the game does at launch), unpack the item's zip into mods/Steam/<id>, register it.
$steamDir = Join-Path $modsDir 'Steam'
$uploader = Join-Path $PSScriptRoot '..\WorkshopUpload\WorkshopUpload.csproj'
foreach ($id in $Steam) {
    if ($steamFound.ContainsKey($id)) { continue }
    Write-Host "Workshop item $id is not registered yet; checking it with the Steam client"
    # Steam reports subscription and install state per app, so the download runs as the game, not as the uploader app.
    $env:SteamAppId = '457140'; $env:SteamGameId = '457140'
    try { $out = & dotnet run -c Release --project $uploader -- download $id 2>&1 | ForEach-Object { "$_" } }
    finally { Remove-Item Env:SteamAppId, Env:SteamGameId -ErrorAction SilentlyContinue }
    $subscribed = ($out | Select-String -Pattern '^subscribed: True' -Quiet)
    $zip = ($out | Select-String -Pattern '^install path: (.+?) \(\d+ bytes\)$' | ForEach-Object { $_.Matches[0].Groups[1].Value } | Select-Object -First 1)
    $updated = ($out | Select-String -Pattern '^updated: (\d+)$' | ForEach-Object { [long]$_.Matches[0].Groups[1].Value } | Select-Object -First 1)
    if (-not $subscribed) { Write-Warning "Workshop item $id is not subscribed; the game would remove it again, so it is skipped"; continue }
    if (-not $zip -or -not (Test-Path -LiteralPath $zip)) { Write-Warning "Workshop item $id did not download:`n$($out -join "`n")"; continue }
    $target = Join-Path $steamDir $id
    $marker = Join-Path $target 'mod.yaml'
    if (-not (Test-Path -LiteralPath $marker) -or (Get-Item -LiteralPath $zip).LastWriteTimeUtc -gt (Get-Item -LiteralPath $marker).LastWriteTimeUtc) {
        if (Test-Path -LiteralPath $target) { Remove-Item -Recurse -Force $target }
        Expand-Archive -LiteralPath $zip -DestinationPath $target -Force
        Write-Host "Unpacked Workshop item $id into $target"
    }
    $title = $id; $staticID = $id
    $yaml = Join-Path $target 'mod.yaml'
    if (Test-Path -LiteralPath $yaml) {
        $text = Get-Content -LiteralPath $yaml -Raw
        $m = [regex]::Match($text, '(?m)^title:\s*["'']?([^"''\r\n]+)["'']?');    if ($m.Success) { $title = $m.Groups[1].Value.Trim() }
        $m = [regex]::Match($text, '(?m)^staticID:\s*["'']?([^"''\r\n]+)["'']?'); if ($m.Success) { $staticID = $m.Groups[1].Value.Trim() }
    }
    $data.mods += [pscustomobject]([ordered]@{
        label = [ordered]@{ distribution_platform = 1; id = $id; title = $title; version = $updated }
        status = 1; enabled = $false; enabledForDlc = @($dlcs); crash_count = 0; reinstall_path = $null; staticID = $staticID
    })
    $steamFound[$id] = $title
    Write-Host "Registered Workshop item '$title' ($id) for this run"
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
        $m = [regex]::Match($text, '(?m)^title:\s*["'']?([^"''\r\n]+)["'']?');    if ($m.Success) { $title = $m.Groups[1].Value.Trim() }
        $m = [regex]::Match($text, '(?m)^staticID:\s*["'']?([^"''\r\n]+)["'']?'); if ($m.Success) { $staticID = $m.Groups[1].Value.Trim() }
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
if ($Omit.Count -gt 0) { Write-Host "Omitted: $($Omit -join ', ')" }
if ($steamFound.Count -gt 0) { Write-Host "Workshop: $(($steamFound.GetEnumerator() | ForEach-Object { "$($_.Value) ($($_.Key))" }) -join ', ')" }

if ($DryRun) {
    $out = Join-Path $env:TEMP 'oni-modtest.mods.json'
    [IO.File]::WriteAllText($out, $json, $utf8)
    Write-Host "Dry run: loadout written to $out, game not launched."
    return
}

$backup = "$modsJson.modtest-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
Copy-Item -LiteralPath $modsJson -Destination $backup
Write-Host "Backed up mods.json to $backup"
$started = Get-Date
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
        Remove-Item -LiteralPath $backup -Force
        Write-Host 'Restored the normal loadout; backup removed.'
        if (-not $KeepDumps) {
            # The data-dump mod rewrites its json files on every launch; the ones from this session are single-use.
            $dumps = @(Get-ChildItem -LiteralPath $ProfileLink -Filter 'oni-data-dump.*.json' -ErrorAction SilentlyContinue | Where-Object { $_.LastWriteTime -gt $started })
            foreach ($d in $dumps) { Remove-Item -LiteralPath $d.FullName -Force }
            if ($dumps.Count -gt 0) { Write-Host "Removed $($dumps.Count) data-dump json file(s) written this session." }
        }
    }
}
