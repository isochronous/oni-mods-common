<#
.SYNOPSIS
  Load a save in the real game with nobody at the keyboard, run it for a while with saving disabled,
  collect probe readings, and quit.

.DESCRIPTION
  Copies the save next to the results (the original is never the file the game has open), rewrites
  mods.json so the current loadout also has the SaveRunner dev mod (plus -With, minus -Without),
  launches the game with the --saverun arguments (through Steam, or on oni-moono: -Runtime), waits for
  it to quit on its own, then restores mods.json. The mod blocks every save to disk for the whole
  session, so the colony on disk is untouched; kplayerprefs.yaml is not changed, so the game runs in whatever mode it is already in.

.PARAMETER Save
  The .sav file to load.
.PARAMETER Seconds
  Game seconds to run after the colony has loaded (default 60; 300 with -Probe memory).
.PARAMETER Speed
  Game speed 1 to 3 while running (default 1; 3 with -Probe memory).
.PARAMETER Probe
  What the mod records once per game second: 'fridges' (every item in every fridge-controller
  building), 'memory' (process, managed and Unity memory from boot on, what each mod adds as it
  loads, and an end-of-run snapshot of what the memory is made of; SaveRunner is moved to the top of
  mods.json so it loads before every other mod) or nothing.
.PARAMETER With
  Extra mods to enable for the run (local folder names or Workshop item ids).
.PARAMETER Without
  Mods to disable for the run (local folder names or Workshop item ids), e.g. -Without NoMagicFridges
  to see how a save behaves without the mod under test.
.PARAMETER Only
  Disable every other mod: the run has only SaveRunner and the -With mods. Much faster to boot than
  a full loadout; use it when the save does not need the player's mods (a test fixture).
.PARAMETER TimeoutSec
  Real seconds the whole session may take before the mod, and failing that this script, kills it
  (default 900; 1800 with -Probe memory).
.PARAMETER Runtime
  Stock (default: the game through Steam) or Moono (oni-moono's player started directly, the --saverun
  arguments each behind --game-arg, its own --log in the run folder as moono.log; no --game, Moono takes
  the install from Steam).
.PARAMETER MoonoExe
  oni-moono's player for -Runtime Moono. Default: ONI_MOONO_EXE, else F:\modding\oni-moono\player\OniMoono.exe.
.PARAMETER DryRun
  Print the loadout and the command line, and stop: no game check, no folder, mods.json untouched.

.EXAMPLE
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath Sandbox 001.sav' -Seconds 90 -Probe fridges -Label nmf
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath Sandbox 001.sav' -Seconds 90 -Probe fridges -Without NoMagicFridges -Label vanilla
  ./Invoke-SaveRun.ps1 'C:\...\fixtures\testbed-so.sav' -Seconds 10 -TimeoutSec 180 -Only -Label smoke
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath v009.sav' -Probe memory -Without 2854869130,3281716506 -Label mem-full
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath v009.sav' -Probe memory -Only -Label mem-none
  ./Invoke-SaveRun.ps1 'C:\...\The Aftermath v009.sav' -Seconds 30 -Runtime Moono -Without 2854869130,3281716506 -Label moono
#>
param(
    [Parameter(Mandatory)][string]$Save,
    [int]$Seconds = 60,
    [ValidateRange(1, 3)][int]$Speed = 1,
    [ValidateSet('', 'fridges', 'memory')][string]$Probe = '',
    [string[]]$With = @(),
    [string[]]$Without = @(),
    [switch]$Only,
    [int]$TimeoutSec = 900,
    [string]$Label = 'run',
    [string]$OutDir = (Join-Path $PSScriptRoot 'runs'),
    [string]$SteamExe = 'C:\Program Files (x86)\Steam\steam.exe',
    [int]$AppId = 457140,
    [ValidateSet('Stock', 'Moono')][string]$Runtime = 'Stock',
    [string]$MoonoExe = '',
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'
# Started with pwsh -File, a comma-separated argument arrives as one string: split the list parameters.
$With = @($With | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$Without = @($Without | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
# A memory run: half a cycle at speed 3 shows growth after the load without holding the PC for long,
# and a full loadout takes minutes to boot before the run starts.
if ($Probe -eq 'memory') {
    if (-not $PSBoundParameters.ContainsKey('Seconds')) { $Seconds = 300 }
    if (-not $PSBoundParameters.ContainsKey('Speed')) { $Speed = 3 }
    if (-not $PSBoundParameters.ContainsKey('TimeoutSec')) { $TimeoutSec = 1800 }
}

$klei = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Klei\OxygenNotIncluded'
$modsJson = Join-Path $klei 'mods\mods.json'
$prefs = Join-Path $klei 'kplayerprefs.yaml'
$playerLog = Join-Path $env:USERPROFILE 'AppData\LocalLow\Klei\Oxygen Not Included\Player.log'
$utf8 = New-Object System.Text.UTF8Encoding($false)
# Get-GameProcess: the game is OxygenNotIncluded, or OniMoono started with the --saverun arguments; any
# open OniMoono but Moono's crash watcher counts as a running game. The Moono launch helpers too.
. (Join-Path $PSScriptRoot '..\TestRunner\GameProcess.ps1')

if (-not (Test-Path -LiteralPath $Save)) { throw "Save not found: $Save" }
if (-not $DryRun -and (Get-GameProcess -Any)) { throw 'The game is already running; close it first.' }
if ($Runtime -eq 'Stock' -and -not (Test-Path $SteamExe)) { throw "Steam not found at $SteamExe" }
$moono = if ($Runtime -eq 'Moono') { Get-MoonoExe $MoonoExe } else { '' }
if (-not (Test-Path (Join-Path $klei 'mods\local\SaveRunner\SaveRunner.dll'))) { throw 'SaveRunner is not deployed; run: dotnet build tools/SaveRunner -c Release' }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $OutDir "$Label-$stamp"
if (-not $DryRun) { New-Item -ItemType Directory -Force $runDir | Out-Null }
# A copy under a name without spaces: the path travels through Steam's (or Moono's) command line.
$saveCopy = Join-Path $runDir 'colony.sav'
if (-not $DryRun) { Copy-Item -LiteralPath $Save -Destination $saveCopy }
$csv = Join-Path $runDir 'probe.csv'
# A Moono launch logs to the run folder: Moono writes the game's log there and no Player.log.
$moonoLog = if ($Runtime -eq 'Moono') { Join-Path $runDir 'moono.log' } else { '' }
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
        } elseif ($Only) { $m.enabled = $false; $m.enabledForDlc = @() }
    }
    foreach ($id in $enable) {
        if ($seen[$id]) { continue }
        if (-not (Test-Path (Join-Path $klei "mods\local\$id"))) { throw "Local mod folder not found: $id" }
        $json.mods += [pscustomobject]@{
            label = [pscustomobject]@{ distribution_platform = 0; id = $id; title = $id; version = 0 }
            status = 1; enabled = $true; enabledForDlc = @($dlc); crash_count = 0; reinstall_path = $null; staticID = "Isochronous.$id"
        }
    }
    # The game loads mods in mods.json order; the memory probe starts in SaveRunner's OnLoad, so
    # SaveRunner goes first and every other mod loads while it is measuring.
    if ($Probe -eq 'memory') {
        $first = @($json.mods | Where-Object { $_.label.id -eq 'SaveRunner' -and $_.label.distribution_platform -eq 0 })
        $json.mods = $first + @($json.mods | Where-Object { $first -notcontains $_ })
    }
    [IO.File]::WriteAllText($modsJson, ($json | ConvertTo-Json -Depth 10), $utf8)
}

$gameArgs = @("--saverun=$saveCopy", "--saverun-seconds=$Seconds", "--saverun-speed=$Speed", "--saverun-out=$csv", "--saverun-timeout=$TimeoutSec")
if ($Probe) { $gameArgs += "--saverun-probe=$Probe" }
# Stock: Steam starts the game with the arguments. Moono: its player started directly, its own --log first,
# each game argument behind --game-arg (Moono refuses any -- option it does not know); no --game.
if ($Runtime -eq 'Moono') {
    $launchArgs = Get-MoonoArgumentLine $gameArgs $moonoLog
    $launchText = "$(ConvertTo-CommandLineArgument $moono) $launchArgs"
} else {
    $steamArgs = @('-applaunch', "$AppId") + $gameArgs
    $launchText = "$SteamExe $($steamArgs -join ' ')"
}
Write-Host "Loadout: $(if ($Only) { 'only' } else { 'current +' }) SaveRunner$(if ($With) { ' + ' + ($With -join ', ') })$(if ($Without) { ' - ' + ($Without -join ', ') }); mode $(if ($spacedOut) { 'Spaced Out' } else { 'base game' }); runtime $Runtime"
if ($Probe -eq 'memory') { Write-Host "Memory probe: SaveRunner loads first; $Seconds game s at speed $Speed, timeout $TimeoutSec s" }
if ($DryRun) {
    Write-Host "Would launch: $launchText"
    Write-Host "Dry run: $Save not copied, $runDir not made, mods.json untouched, game not launched."
    return
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
$launchFailure = ''
$launcher = $null
$stockBefore = @()
$strayHeld = $false
$proc = $null
$t0 = Get-Date
try {
    Set-Mods
    Write-Host "Launching: $launchText"
    $t0 = Get-Date
    if ($Runtime -eq 'Moono') {
        # First: whatever fails below, a stock game that was already up is not one the launch started.
        $stockBefore = Get-StockGameIds
        $streams = Get-MoonoStreamPaths $moonoLog
        $launcher = Start-Process -FilePath $moono -ArgumentList $launchArgs -WorkingDirectory (Split-Path $moono -Parent) -PassThru -RedirectStandardOutput $streams.Out -RedirectStandardError $streams.Err
        # The game is an OniMoono with the --saverun arguments (in standalone mode the launcher itself, so it
        # goes first). Fails at once if the stock game appears or OniMoono exits with no game up.
        try {
            $proc = Wait-MoonoGame $launcher $stockBefore 120 { Get-GameProcess -Marker '--saverun' | Where-Object { $_.ProcessName -eq 'OniMoono' } | Sort-Object { $_.Id -ne $launcher.Id } } $moonoLog
        } catch {
            $launchFailure = $_.Exception.Message
        }
    } else {
        Start-Process -FilePath $SteamExe -ArgumentList $steamArgs | Out-Null
        while (-not $proc -and ((Get-Date) - $t0).TotalSeconds -lt 120) { Start-Sleep 2; $proc = Get-GameProcess -Marker '--saverun' | Select-Object -First 1 }
        if (-not $proc) { throw 'The game did not start within 120 s.' }
    }
    if ($proc) {
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
    }
    # Only a Player.log written since this launch: an older one is an earlier run's. A Moono launch writes none.
    $logCopied = Copy-FreshLog $playerLog (Join-Path $runDir 'Player.log') $t0
}
finally {
    if ($Runtime -eq 'Moono' -and -not $proc) {
        # No game was found: what the launch started must not outlive the restore. A stock game that is not
        # provably this launch's stays up (it may be the player's) and holds the loadout until it exits.
        $stray = @(Stop-MoonoLaunch $launcher $stockBefore $moonoLog "--saverun-out=$csv")
        $strayWaitSec = 900
        $strayEnd = (Get-Date).AddSeconds($strayWaitSec)
        foreach ($id in $stray) {
            Write-Host "A stock OxygenNotIncluded (pid $id) started during the failed Moono launch; the runner cannot tell it from a game you opened, so it is left running. mods.json holds the run's loadout until it exits (waiting up to $strayWaitSec s). To restore now, close that game, or press Ctrl+C and run: Copy-Item '$backup' '$modsJson'"
            $left = [math]::Max(1, [int]($strayEnd - (Get-Date)).TotalSeconds)
            Wait-Process -Id $id -Timeout $left -ErrorAction SilentlyContinue
            if (Get-Process -Id $id -ErrorAction SilentlyContinue) { $strayHeld = $true }
        }
    }
    if ($strayHeld) {
        Write-Warning "A stock OxygenNotIncluded that started during the failed Moono launch is still running after $strayWaitSec s. mods.json still holds the run's loadout; yours is in $backup (the next run restores it first). Restore it once that game has closed:`n  Copy-Item '$backup' '$modsJson'"
    } else {
        Copy-Item $backup $modsJson -Force
        Remove-Item $backup -Force
        Write-Host 'Restored mods.json.'
    }
}
$status = if (Test-Path $statusFile) { [IO.File]::ReadAllText($statusFile, $utf8).Trim() } else { '(no status file: the run never finished)' }
$rows = if (Test-Path $csv) { @([IO.File]::ReadAllLines($csv, $utf8)).Count - 1 } else { 0 }
# What ran, from line 1 of the game's log: Player.log for a stock run, moono.log (the game's log on a direct Moono launch) otherwise.
$ranLog = if ($logCopied) { Join-Path $runDir 'Player.log' } elseif ($moonoLog -and (Test-Path -LiteralPath $moonoLog)) { $moonoLog } else { '' }
$ran = Get-LogRuntime $ranLog
$ranText = if ($ran.Runtime -eq 'Moono') { "Moono (commit $($ran.Commit)$(if ($ran.Launch) { '; launch: ' + $ran.Launch }))" } else { 'Stock' }
Write-Host $(if ($ran.Runtime) { "Runtime: $Runtime asked; $(Split-Path $ranLog -Leaf) shows $ranText" } else { "Runtime: $Runtime asked; no game log written by this launch" })
if ($ran.Runtime -and $ran.Runtime -ne $Runtime) { Write-Warning "The run asked for $Runtime but the game ran on $($ran.Runtime)." }
if ($Runtime -eq 'Moono') {
    if ($launchFailure) { Write-Host "Launch failed: $launchFailure" }
    elseif (-not (Test-Path $statusFile)) { Write-Host (Get-MoonoExitText $proc $moonoLog "the run wrote its status" $launcher) }
    else { $code = Get-MoonoExitCode $proc $launcher $moonoLog; Write-Host "OniMoono exit code $(if ($null -eq $code) { 'unknown' } else { $code }); Moono's log: $moonoLog" }
}
Write-Host ("Game {0} after {1} s; status {2}; {3} probe row(s) in {4}" -f $launcherStatus, [int]((Get-Date) - $t0).TotalSeconds, $status, $rows, $runDir)
