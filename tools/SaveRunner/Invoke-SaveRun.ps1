<#
.SYNOPSIS
  Load a save in the real game with nobody at the keyboard, run it for a while with saving disabled,
  collect probe readings, and quit.

.DESCRIPTION
  Copies the save next to the results (the original is never the file the game has open), rewrites
  mods.json so the current loadout also has the SaveRunner dev mod (plus -With, minus -Without),
  launches the game through Steam with the --saverun arguments, waits for it to quit on its own, then
  restores mods.json. The mod blocks every save to disk for the whole session, so the colony on disk
  is untouched; kplayerprefs.yaml is not changed, so the game runs in whatever mode it is already in.

.PARAMETER Save
  The .sav file to load.
.PARAMETER Seconds
  Game seconds to run after the colony has loaded (default 60).
.PARAMETER Speed
  Game speed 1 to 3 while running (default 1).
.PARAMETER Probe
  What the mod records once per game second: 'fridges' (every item in every fridge-controller
  building) or nothing.
.PARAMETER With
  Extra mods to enable for the run (local folder names or Workshop item ids).
.PARAMETER Without
  Mods to disable for the run (local folder names or Workshop item ids), e.g. -Without NoMagicFridges
  to see how a save behaves without the mod under test.
.PARAMETER TimeoutSec
  Real seconds the whole session may take before the mod, and failing that this script, kills it.

.EXAMPLE
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath Sandbox 001.sav' -Seconds 90 -Probe fridges -Label nmf
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath Sandbox 001.sav' -Seconds 90 -Probe fridges -Without NoMagicFridges -Label vanilla
#>
param(
    [Parameter(Mandatory)][string]$Save,
    [int]$Seconds = 60,
    [ValidateRange(1, 3)][int]$Speed = 1,
    [ValidateSet('', 'fridges')][string]$Probe = '',
    [string[]]$With = @(),
    [string[]]$Without = @(),
    [int]$TimeoutSec = 900,
    [string]$Label = 'run',
    [string]$OutDir = (Join-Path $PSScriptRoot 'runs'),
    [string]$SteamExe = 'C:\Program Files (x86)\Steam\steam.exe',
    [int]$AppId = 457140
)
$ErrorActionPreference = 'Stop'
# Started with pwsh -File, a comma-separated argument arrives as one string: split the list parameters.
$With = @($With | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$Without = @($Without | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })

$klei = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Klei\OxygenNotIncluded'
$modsJson = Join-Path $klei 'mods\mods.json'
$prefs = Join-Path $klei 'kplayerprefs.yaml'
$playerLog = Join-Path $env:USERPROFILE 'AppData\LocalLow\Klei\Oxygen Not Included\Player.log'
$utf8 = New-Object System.Text.UTF8Encoding($false)

if (-not (Test-Path -LiteralPath $Save)) { throw "Save not found: $Save" }
if (Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue) { throw 'The game is already running; close it first.' }
if (-not (Test-Path $SteamExe)) { throw "Steam not found at $SteamExe" }
if (-not (Test-Path (Join-Path $klei 'mods\local\SaveRunner\SaveRunner.dll'))) { throw 'SaveRunner is not deployed; run: dotnet build tools/SaveRunner -c Release' }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $OutDir "$Label-$stamp"
New-Item -ItemType Directory -Force $runDir | Out-Null
# A copy under a name without spaces: the path travels through Steam's command line.
$saveCopy = Join-Path $runDir 'colony.sav'
Copy-Item -LiteralPath $Save -Destination $saveCopy
$csv = Join-Path $runDir 'probe.csv'
$statusFile = "$csv.status"
$spacedOut = ([IO.File]::ReadAllText($prefs, $utf8) -match '(?m)^\s*EXPANSION1_ID\.ENABLED:\s*1')
$dlc = if ($spacedOut) { 'EXPANSION1_ID' } else { '' }

function Set-Mods {
    $json = [IO.File]::ReadAllText($modsJson, $utf8) | ConvertFrom-Json
    $enable = @('SaveRunner') + $With
    $seen = @{}
    foreach ($m in $json.mods) {
        $id = $m.label.id
        if ($Without -contains $id) { $m.enabled = $false; $m.enabledForDlc = @(); continue }
        if ($enable -contains $id) {
            $seen[$id] = $true
            $m.enabled = $true
            $m.enabledForDlc = @($dlc)
            $m.crash_count = 0
            # A rebuilt local mod is left ReinstallPending (3) by the game; the boot-time reinstall
            # then blocks the main menu. Installed (1) makes the game just load it.
            if ($m.label.distribution_platform -eq 0) { $m.status = 1; $m.reinstall_path = $null }
        }
    }
    foreach ($id in $enable) {
        if ($seen[$id]) { continue }
        if (-not (Test-Path (Join-Path $klei "mods\local\$id"))) { throw "Local mod folder not found: $id" }
        $json.mods += [pscustomobject]@{
            label = [pscustomobject]@{ distribution_platform = 0; id = $id; title = $id; version = 0 }
            status = 1; enabled = $true; enabledForDlc = @($dlc); crash_count = 0; reinstall_path = $null; staticID = "Isochronous.$id"
        }
    }
    [IO.File]::WriteAllText($modsJson, ($json | ConvertTo-Json -Depth 10), $utf8)
}

# A leftover backup means an earlier run died before its finally block: it holds the real loadout,
# so put it back first instead of overwriting it.
$backup = "$modsJson.saverun-backup"
if (Test-Path $backup) {
    Write-Warning "Leftover backup $backup from an unfinished run; restoring it before starting."
    Copy-Item $backup $modsJson -Force
} else {
    Copy-Item $modsJson $backup
}
$launcherStatus = 'not-started'
try {
    Set-Mods
    $gameArgs = @('-applaunch', "$AppId", "--saverun=$saveCopy", "--saverun-seconds=$Seconds", "--saverun-speed=$Speed", "--saverun-out=$csv", "--saverun-timeout=$TimeoutSec")
    if ($Probe) { $gameArgs += "--saverun-probe=$Probe" }
    Write-Host "Loadout: current + SaveRunner$(if ($With) { ' + ' + ($With -join ', ') })$(if ($Without) { ' - ' + ($Without -join ', ') }); mode $(if ($spacedOut) { 'Spaced Out' } else { 'base game' })"
    Write-Host "Launching: $($gameArgs -join ' ')"
    $t0 = Get-Date
    Start-Process -FilePath $SteamExe -ArgumentList $gameArgs | Out-Null
    $proc = $null
    while (-not $proc -and ((Get-Date) - $t0).TotalSeconds -lt 120) { Start-Sleep 2; $proc = Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue }
    if (-not $proc) { throw 'The game did not start within 120 s.' }
    $launcherStatus = 'exited'
    while (-not $proc.HasExited) {
        Start-Sleep 5
        if ((Test-Path $statusFile) -and ((Get-Date) - (Get-Item $statusFile).LastWriteTime).TotalSeconds -gt 60) {
            Stop-Process -Id $proc.Id -Force; $launcherStatus = 'killed-after-status'; break
        }
        if (((Get-Date) - $t0).TotalSeconds -gt ($TimeoutSec + 180)) { Stop-Process -Id $proc.Id -Force; $launcherStatus = 'killed'; break }
        $proc.Refresh()
    }
    Start-Sleep 3
    if (Test-Path $playerLog) { Copy-Item $playerLog (Join-Path $runDir 'Player.log') -Force }
}
finally {
    Copy-Item $backup $modsJson -Force
    Remove-Item $backup -Force
    Write-Host 'Restored mods.json.'
}
$status = if (Test-Path $statusFile) { [IO.File]::ReadAllText($statusFile, $utf8).Trim() } else { '(no status file: the run never finished)' }
$rows = if (Test-Path $csv) { @([IO.File]::ReadAllLines($csv, $utf8)).Count - 1 } else { 0 }
Write-Host ("Game {0} after {1} s; status {2}; {3} probe row(s) in {4}" -f $launcherStatus, [int]((Get-Date) - $t0).TotalSeconds, $status, $rows, $runDir)
