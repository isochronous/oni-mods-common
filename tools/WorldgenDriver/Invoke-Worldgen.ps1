<#
.SYNOPSIS
Generates a list of coordinates in the real game, unattended, and collects one result line per coordinate.

.DESCRIPTION
Coordinates are grouped by mode (base game or Spaced Out) and run in sessions of up to -PerSession
each: one game start per session, the driver mod returning to the main menu between coordinates.
A hang or crash ends a session; the launcher starts another for what is left. Before each session
the script sets the mode in kplayerprefs.yaml and rewrites mods.json so that only the WorldgenDriver
mod plus the mods named in -Mods are enabled. Both files are backed up first and restored at the end.
Results go to -OutDir\<Label>-<stamp>.jsonl (one JSON object per coordinate) with a Player.log copy
per session, and a summary is printed.

.EXAMPLE
.\Invoke-Worldgen.ps1 CERS-A-100-0-0-0 -Mods NaturalBackwalls
.EXAMPLE
.\Invoke-Worldgen.ps1 -ListFile seeds.txt -Mods NaturalBackwalls013 -Label nb013
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)][string[]]$Coordinate = @(),
    [string]$ListFile,
    [string[]]$Mods = @(),            # local mod folder names and/or Workshop item ids to enable besides the driver
    [int]$TimeoutSec = 300,           # no progress for this long = hang (inside the game)
    [int]$PerSession = 25,            # coordinates per game start
    [switch]$NoGraphics,
    [switch]$Load,
    [ValidateSet('', 'all')][string]$Stories = '',            # force every story trait on (after the coordinate)
    [ValidateSet('', 'all', 'guarantee')][string]$Mixing = '',  # force every DLC/world/subworld mixing on
    [ValidateSet('auto', 'base', 'spacedout')][string]$Mode = 'auto',
    [string]$Label = 'batch',
    [string]$OutDir = (Join-Path $PSScriptRoot 'runs'),
    [string]$SteamExe = 'C:\Program Files (x86)\Steam\steam.exe',
    [int]$AppId = 457140
)
$ErrorActionPreference = 'Stop'
$klei = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Klei\OxygenNotIncluded'
$modsJson = Join-Path $klei 'mods\mods.json'
$prefs = Join-Path $klei 'kplayerprefs.yaml'
$playerLog = Join-Path $env:USERPROFILE 'AppData\LocalLow\Klei\Oxygen Not Included\Player.log'
$resultFile = Join-Path $klei 'worldgen-driver\results.jsonl'
$listPath = Join-Path $klei 'worldgen-driver\list.txt'
$utf8 = New-Object System.Text.UTF8Encoding($false)

$all = @($Coordinate)
if ($ListFile) { $all += Get-Content $ListFile | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') } }
$all = @($all | Select-Object -Unique)
if (-not $all) { throw 'No coordinates given (positional or -ListFile).' }
if (Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue) { throw 'The game is already running; close it first.' }
if (-not (Test-Path $SteamExe)) { throw "Steam not found at $SteamExe" }
if (-not (Test-Path (Join-Path $klei 'mods\local\WorldgenDriver\WorldgenDriver.dll'))) { throw 'WorldgenDriver is not deployed; run: dotnet build tools/WorldgenDriver -c Release' }
New-Item -ItemType Directory -Force $OutDir | Out-Null
New-Item -ItemType Directory -Force (Split-Path $resultFile) | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$master = Join-Path $OutDir "$Label-$stamp.jsonl"

function Set-Mode([bool]$spacedOut) {
    $lines = [IO.File]::ReadAllLines($prefs, $utf8)
    $value = if ($spacedOut) { 1 } else { 0 }
    $done = $false
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\s*EXPANSION1_ID\.ENABLED:') { $lines[$i] = "  EXPANSION1_ID.ENABLED: $value"; $done = $true }
    }
    if (-not $done) {
        $idx = [Array]::IndexOf($lines, ($lines | Where-Object { $_ -match '^ints:' } | Select-Object -First 1))
        if ($idx -lt 0) { throw 'kplayerprefs.yaml has no ints: section' }
        $lines = $lines[0..$idx] + @("  EXPANSION1_ID.ENABLED: $value") + $lines[($idx + 1)..($lines.Count - 1)]
    }
    [IO.File]::WriteAllLines($prefs, $lines, $utf8)
}

function Set-Mods([bool]$spacedOut, [string[]]$wanted) {
    $dlc = if ($spacedOut) { 'EXPANSION1_ID' } else { '' }
    $json = [IO.File]::ReadAllText($modsJson, $utf8) | ConvertFrom-Json
    $seen = @{}
    foreach ($m in $json.mods) {
        $on = $wanted -contains $m.label.id   # local folder names or Workshop item ids
        if ($on) { $seen[$m.label.id] = $true }
        $m.enabled = $on
        $m.enabledForDlc = @(if ($on) { $dlc })
        if ($on) {
            # A rebuilt local mod is left ReinstallPending (3) by the game; the boot-time reinstall
            # then blocks the main menu. Installed (1) makes the game just load it.
            $m.crash_count = 0
            if ($m.label.distribution_platform -eq 0) { $m.status = 1; $m.reinstall_path = $null }
        }
    }
    foreach ($id in $wanted) {
        if ($seen[$id]) { continue }
        if (-not (Test-Path (Join-Path $klei "mods\local\$id"))) { throw "Local mod folder not found: $id" }
        $json.mods += [pscustomobject]@{
            label = [pscustomobject]@{ distribution_platform = 0; id = $id; title = $id; version = 0 }
            status = 1; enabled = $true; enabledForDlc = @($dlc); crash_count = 0; reinstall_path = $null; staticID = "Isochronous.$id"
        }
    }
    [IO.File]::WriteAllText($modsJson, ($json | ConvertTo-Json -Depth 10), $utf8)
}

function Read-Results {
    # The game appends lines while we poll: only complete lines count, and a bad one is skipped.
    if (-not (Test-Path $resultFile)) { return }
    foreach ($line in ([IO.File]::ReadAllText($resultFile, $utf8) -split "`n")) {
        $t = $line.Trim()
        if (-not $t.EndsWith('}')) { continue }
        try { $t | ConvertFrom-Json } catch { Write-Warning "unreadable result line skipped: $($t.Substring(0, [Math]::Min(80, $t.Length)))" }
    }
}

Copy-Item $modsJson "$modsJson.worldgen-backup" -Force
Copy-Item $prefs "$prefs.worldgen-backup" -Force
$results = @()
$session = 0
try {
    $groups = @{ $false = [System.Collections.Generic.List[string]]::new(); $true = [System.Collections.Generic.List[string]]::new() }
    foreach ($c in $all) {
        $so = switch ($Mode) { 'base' { $false } 'spacedout' { $true } default { [bool]($c -match '^(.+)-C-\d+-') } }
        $groups[$so].Add($c)
    }
    foreach ($so in @($false, $true)) {
        $pending = $groups[$so]
        if ($pending.Count -eq 0) { continue }
        $wanted = @('WorldgenDriver') + $Mods
        $emptySessions = 0
        while ($pending.Count -gt 0) {
            $session++
            $chunk = @($pending | Select-Object -First $PerSession)
            Set-Mode $so
            Set-Mods $so $wanted
            [IO.File]::WriteAllLines($listPath, $chunk, $utf8)
            Remove-Item $resultFile -ErrorAction SilentlyContinue
            $gameArgs = @('-applaunch', "$AppId")
            if ($NoGraphics) { $gameArgs += @('-batchmode', '-nographics') }
            $gameArgs += @("--worldgen-list=$listPath", "--worldgen-out=$resultFile", "--worldgen-timeout=$TimeoutSec", "--worldgen-expansion1=$([int]$so)")
            if ($Load) { $gameArgs += '--worldgen-load=1' }
            if ($Stories) { $gameArgs += "--worldgen-stories=$Stories" }
            if ($Mixing) { $gameArgs += "--worldgen-mixing=$Mixing" }
            Write-Host ("== session {0}: {1} coordinate(s), {2}, mods: {3}" -f $session, $chunk.Count, $(if ($so) { 'Spaced Out' } else { 'base game' }), ($wanted -join ', '))
            $t0 = Get-Date
            Start-Process -FilePath $SteamExe -ArgumentList $gameArgs | Out-Null
            $proc = $null
            while (-not $proc -and ((Get-Date) - $t0).TotalSeconds -lt 120) { Start-Sleep 2; $proc = Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue }
            $launcherStatus = 'no-process'
            if ($proc) {
                $launcherStatus = 'exited'
                $lastCount = 0; $lastChange = Get-Date
                while (-not $proc.HasExited) {
                    $n = @(Read-Results).Count
                    if ($n -ne $lastCount) {
                        foreach ($r in (Read-Results | Select-Object -Skip $lastCount)) { Write-Host ("   {0,-22} {1,-10} {2,6}s  {3} {4} {5}" -f $r.coordinate, $r.status, $r.secondsSinceStart, $r.stage, $r.applied, $r.detail) }
                        $lastCount = $n; $lastChange = Get-Date
                    }
                    if (((Get-Date) - $lastChange).TotalSeconds -gt ($TimeoutSec + 240)) { Stop-Process -Id $proc.Id -Force; $launcherStatus = 'killed'; Start-Sleep 3; break }
                    Start-Sleep 3
                }
            }
            $sessionLog = Join-Path $OutDir ("{0}-{1}-session{2}.log" -f $Label, $stamp, $session)
            if (Test-Path $playerLog) { Copy-Item $playerLog $sessionLog -Force }
            $got = @(Read-Results)
            foreach ($r in $got | Select-Object -Skip $lastCount) { Write-Host ("   {0,-22} {1,-10} {2,6}s  {3} {4}" -f $r.coordinate, $r.status, $r.secondsSinceStart, $r.stage, $r.detail) }
            if (Test-Path $resultFile) { Add-Content $master (Get-Content $resultFile) -Encoding utf8 }
            foreach ($r in $got) { $r | Add-Member -NotePropertyName session -NotePropertyValue $session -Force; $r | Add-Member -NotePropertyName log -NotePropertyValue $sessionLog -Force; $results += $r; [void]$pending.Remove($r.coordinate) }
            Write-Host ("   session {0} {1}: {2} result(s) in {3} s; log {4}" -f $session, $launcherStatus, $got.Count, [int]((Get-Date) - $t0).TotalSeconds, $sessionLog)
            if ($got.Count -eq 0) {
                $emptySessions++
                if ($emptySessions -ge 2) { Write-Warning ("giving up on {0} coordinate(s) after two sessions without results: {1}" -f $pending.Count, ($pending -join ', ')); break }
            } else { $emptySessions = 0 }
        }
    }
}
finally {
    Copy-Item "$modsJson.worldgen-backup" $modsJson -Force
    Copy-Item "$prefs.worldgen-backup" $prefs -Force
    Write-Host 'Restored mods.json and kplayerprefs.yaml.'
}
Write-Host "Results: $master"
$results | Group-Object status | ForEach-Object { Write-Host ("  {0,-12} {1}" -f $_.Name, $_.Count) }
$results | Where-Object { $_.status -notin @('complete', 'loaded') } | Format-Table coordinate, status, stage, percent, secondsSinceStart, session -AutoSize
$results
