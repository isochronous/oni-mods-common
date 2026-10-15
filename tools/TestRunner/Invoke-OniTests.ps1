<#
.SYNOPSIS
  Build a mod and its in-game tests, run them in the real game with nobody at the keyboard, and report.

.DESCRIPTION
  For each mod named in -Mods, finds <repo>/src/<Mod>.Tests/<Mod>.Tests.csproj (or <repo>/mods/<name>/src/...) under the workspace
  root (the folder above this common repo), builds it (which builds and deploys the mod and the
  TestRunner harness), checks the test DLL loads against the game assemblies (tools/ModCheck), and
  copies it into a run folder. Then rewrites mods.json so that only TestRunner, the mods under test
  and -With are enabled, launches the game through Steam with the --onitest arguments, waits for it
  to quit on its own, restores mods.json and prints the results. The harness blocks every save to
  disk outside the run folder, so the fixture and the player's colonies are untouched.

.PARAMETER Mods
  Mod project names (the folder under src/), e.g. DirectAutomationBridges.
.PARAMETER Filter
  Only run tests whose "Assembly/Class.Method" or display name matches the glob(s), ';'-separated.
.PARAMETER Fixture
  The save to load. Default: fixtures/testbed-so.sav when the game is in Spaced Out mode, else
  fixtures/testbed-base.sav (see fixtures/README.md).
.PARAMETER With
  Extra mods to enable (local folder names), e.g. a mod the tests expect alongside.
.PARAMETER Steam
  Workshop mods to enable as well, by item id, e.g. -Steam 3724635599 to run the tests alongside
  another author's mod the player uses. The item must already be installed (in mods.json).
.PARAMETER BuildArgs
  Extra arguments for the dotnet build of the test project (and so of the mod), e.g.
  -BuildArgs '-p:PLibVersion=4.25.1','-p:RestoreAdditionalProjectSources=C:eed'.
.PARAMETER BuildOnly
  Build the test project (and so the mod) and check it against the game assemblies, then stop: no
  game session. Like every run, it waits its turn behind any other run already going.
.PARAMETER KeepGame
  Leave the colony open when the tests are done; the script waits until the game is closed by hand.
.PARAMETER TimeoutSec
  Real seconds the whole session may take before the harness, and failing that this script, ends it.

.EXAMPLE
  pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods DirectAutomationBridges
  pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods DirectAutomationBridges -Filter "*DragFromEnd*" -KeepGame
#>
param(
    [Parameter(Mandatory)][string[]]$Mods,
    [string]$Filter = '',
    [string]$Fixture = '',
    [string[]]$With = @(),
    [string[]]$Steam = @(),
    [switch]$NoBuild,
    [switch]$NoCheck,
    [switch]$BuildOnly,
    [switch]$KeepGame,
    [switch]$Trace,
    [string[]]$BuildArgs = @(),
    [int]$TimeoutSec = 600,
    [int]$TestTimeoutSec = 60,
    [string]$Label = 'run',
    [string]$OutDir = (Join-Path $PSScriptRoot 'runs'),
    [string]$SteamExe = 'C:\Program Files (x86)\Steam\steam.exe',
    [int]$AppId = 457140
)
$ErrorActionPreference = 'Stop'
# Started with pwsh -File, a comma-separated argument arrives as one string: split the list parameters.
$Mods = @($Mods | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$With = @($With | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$Steam = @($Steam | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })

$commonRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
# The workspace holds every mod repo and the oni-mods-common checkout; this script may be running
# from a mod's common/ submodule, one level deeper, so walk up until oni-mods-common is a sibling.
$workspace = $commonRoot
while ($workspace -and -not (Test-Path (Join-Path $workspace 'oni-mods-common'))) { $workspace = Split-Path $workspace -Parent }
if (-not $workspace) { $workspace = (Resolve-Path (Join-Path $commonRoot '..')).Path }
$klei = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Klei\OxygenNotIncluded'
$modsJson = Join-Path $klei 'mods\mods.json'
$prefs = Join-Path $klei 'kplayerprefs.yaml'
$playerLog = Join-Path $env:USERPROFILE 'AppData\LocalLow\Klei\Oxygen Not Included\Player.log'
$utf8 = New-Object System.Text.UTF8Encoding($false)

function Get-GameFolder {
    if ($env:ONI_GAME_FOLDER -and (Test-Path $env:ONI_GAME_FOLDER)) { return $env:ONI_GAME_FOLDER }
    foreach ($candidate in 'F:\SteamLibrary\steamapps\common\OxygenNotIncluded', 'C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded') {
        if (Test-Path $candidate) { return $candidate }
    }
    throw 'Game folder not found; set ONI_GAME_FOLDER.'
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

function Get-YamlScalar([string]$text, [string]$key) {
    $m = [regex]::Match($text, '(?m)^' + $key + '\s*:\s*(?:"([^"]*)"|''([^'']*)''|([^\r\n#]+))')
    if (-not $m.Success) { return $null }
    foreach ($g in 1..3) { if ($m.Groups[$g].Success) { return $m.Groups[$g].Value.Trim() } }
    return $null
}

# --- one run at a time ---------------------------------------------------------------------------
# Runs started side by side (several agents at once) would share the one game, mods.json and the
# harness's build output; a machine-wide mutex queues them, held until this process exits (an
# exit, a throw or a kill all release it). -BuildOnly takes it too, for the shared build output.
$runLock = [System.Threading.Mutex]::new($false, 'Global\IsochronousOniTests')
try {
    if (-not $runLock.WaitOne(0)) {
        Write-Host "$(Get-Date -Format HH:mm:ss) Another test run holds the game; waiting for it to finish."
        [void]$runLock.WaitOne()
        Write-Host "$(Get-Date -Format HH:mm:ss) Our turn."
    }
} catch [System.Threading.AbandonedMutexException] {
    # The previous holder died without releasing; the mutex is ours now.
}

# --- what to run -------------------------------------------------------------------------------
if (-not $BuildOnly -and (Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue)) { throw 'The game is already running; close it first.' }
if (-not (Test-Path $SteamExe)) { throw "Steam not found at $SteamExe" }
$managed = Join-Path (Get-GameFolder) 'OxygenNotIncluded_Data\Managed'

$spacedOut = ([IO.File]::ReadAllText($prefs, $utf8) -match '(?m)^\s*EXPANSION1_ID\.ENABLED:\s*1')
$dlc = if ($spacedOut) { 'EXPANSION1_ID' } else { '' }
if (-not $Fixture) {
    # Fixture saves are not committed, so a submodule checkout of this repo has none: fall back to
    # the workspace's own oni-mods-common checkout.
    $name = 'fixtures\' + $(if ($spacedOut) { 'testbed-so.sav' } else { 'testbed-base.sav' })
    foreach ($dir in $PSScriptRoot, (Join-Path $workspace 'oni-mods-common\tools\TestRunner')) {
        $candidate = Join-Path $dir $name
        if (Test-Path -LiteralPath $candidate) { $Fixture = $candidate; break }
    }
    if (-not $Fixture) { $Fixture = Join-Path $PSScriptRoot $name }
}
if (-not (Test-Path -LiteralPath $Fixture)) { throw "Fixture save not found: $Fixture (see fixtures/README.md)" }
$header = [Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($Fixture)[0..8191])
$fixtureSpacedOut = $header -match '"dlcIds":\[[^\]]*EXPANSION1_ID'
if ($fixtureSpacedOut -ne $spacedOut) {
    throw "The fixture is a $(if ($fixtureSpacedOut) { 'Spaced Out' } else { 'base game' }) save but the game is in $(if ($spacedOut) { 'Spaced Out' } else { 'base game' }) mode; switch modes in the game or pass another -Fixture."
}

$projects = @()
foreach ($mod in $Mods) {
    # A mod is a repo of its own in the workspace, or a mods/<name> folder of a repo that holds
    # several (the repo this common/ is checked out in first).
    $repos = @(Get-ChildItem -LiteralPath $workspace -Directory)
    $modFolders = @(Split-Path $commonRoot -Parent) + @($repos.FullName) | Select-Object -Unique |
        ForEach-Object { $modsDir = Join-Path $_ 'mods'; if (Test-Path -LiteralPath $modsDir) { (Get-ChildItem -LiteralPath $modsDir -Directory).FullName } }
    $found = @(@($modFolders) + @($repos.FullName) | ForEach-Object { Join-Path $_ "src\$mod.Tests\$mod.Tests.csproj" } | Where-Object { Test-Path -LiteralPath $_ })
    if ($found.Count -eq 0) { throw "No test project src/$mod.Tests/$mod.Tests.csproj under any repo or mods/ folder in $workspace" }
    $projects += [pscustomobject]@{ Mod = $mod; Csproj = $found[0]; Dll = Join-Path (Split-Path $found[0]) "bin\Release\$mod.Tests.dll" }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir = Join-Path $OutDir "$Label-$stamp"
$testsDir = Join-Path $runDir 'tests'
New-Item -ItemType Directory -Force $testsDir | Out-Null
if ($runDir -match '\s') { throw "The run folder path must not contain spaces (it travels through Steam's command line): $runDir" }

foreach ($p in $projects) {
    if (-not $NoBuild) {
        Write-Host "Building $($p.Csproj)"
        & dotnet build $p.Csproj -c Release -nologo -v q @BuildArgs
        if ($LASTEXITCODE -ne 0) { Write-Host "Build failed for $($p.Mod)"; exit 3 }
    }
    if (-not (Test-Path -LiteralPath $p.Dll)) { throw "Test assembly not found after build: $($p.Dll)" }
    if (-not $NoCheck) {
        # ModCheck resolves the test DLL's references from its own folder, so the harness and the mod go next to a copy.
        $checkDir = Join-Path $runDir "check\$($p.Mod)"
        New-Item -ItemType Directory -Force $checkDir | Out-Null
        Copy-Item -LiteralPath $p.Dll -Destination $checkDir
        Copy-Item -LiteralPath (Join-Path $klei 'mods\local\TestRunner\TestRunner.dll') -Destination $checkDir
        $modDll = Join-Path $klei "mods\local\$($p.Mod)\$($p.Mod).dll"
        if (Test-Path -LiteralPath $modDll) { Copy-Item -LiteralPath $modDll -Destination $checkDir }
        Write-Host "Checking $($p.Mod).Tests.dll against the game assemblies"
        $check = & dotnet run -c Release --project (Join-Path $commonRoot 'tools\ModCheck') -- $managed (Join-Path $checkDir "$($p.Mod).Tests.dll") 2>&1 | ForEach-Object { "$_" }
        if ($LASTEXITCODE -ne 0) { Write-Host ($check -join "`n"); Write-Host "ModCheck failed for $($p.Mod).Tests.dll"; exit 3 }
    }
    Copy-Item -LiteralPath $p.Dll -Destination $testsDir
}
if ($BuildOnly) { Write-Host 'Built and checked against the game assemblies (-BuildOnly: no game session).'; exit 0 }
foreach ($mod in $Mods + 'TestRunner') {
    if (-not (Test-Path (Join-Path $klei "mods\local\$mod\$mod.dll"))) { throw "Mod $mod is not deployed to mods\local; build it first." }
}
# A copy under a name without spaces: the path travels through Steam's command line.
$fixtureCopy = Join-Path $runDir 'fixture.sav'
Copy-Item -LiteralPath $Fixture -Destination $fixtureCopy
$resultsFile = Join-Path $runDir 'results.jsonl'
$statusFile = "$resultsFile.status"

# --- loadout -----------------------------------------------------------------------------------
function Set-Mods {
    $json = [IO.File]::ReadAllText($modsJson, $utf8) | ConvertFrom-Json
    $enable = @('TestRunner') + $Mods + $With
    $seen = @{}
    foreach ($m in $json.mods) {
        $id = $m.label.id
        if ($m.label.distribution_platform -eq 0 -and $enable -contains $id) {
            $seen[$id] = $true
            $m.enabled = $true
            $m.enabledForDlc = @($dlc)
            $m.crash_count = 0
            # A rebuilt local mod is left ReinstallPending (3) by the game; the boot-time reinstall
            # then blocks the main menu. Installed (1) makes the game just load it.
            $m.status = 1; $m.reinstall_path = $null
        } elseif ($m.label.distribution_platform -ne 0 -and $Steam -contains $id) {
            # A Workshop item, enabled as the game has it installed.
            $seen["steam:$id"] = $true
            $m.enabled = $true
            $m.enabledForDlc = @($dlc)
            $m.crash_count = 0
        } else {
            $m.enabled = $false
            $m.enabledForDlc = @()
        }
    }
    foreach ($id in $Steam) {
        if (-not $seen["steam:$id"]) { throw "Workshop mod $id is not installed (not in mods.json)." }
    }
    $list = New-Object System.Collections.ArrayList
    foreach ($m in $json.mods) { [void]$list.Add($m) }
    foreach ($id in $enable) {
        if ($seen[$id]) { continue }
        $folder = Join-Path $klei "mods\local\$id"
        if (-not (Test-Path $folder)) { throw "Local mod folder not found: $id" }
        $title = $id; $staticID = "Isochronous.$id"
        $yaml = Join-Path $folder 'mod.yaml'
        if (Test-Path -LiteralPath $yaml) {
            $text = Get-Content -LiteralPath $yaml -Raw
            $v = Get-YamlScalar $text 'title';    if ($null -ne $v) { $title = $v }
            $v = Get-YamlScalar $text 'staticID'; if ($null -ne $v) { $staticID = $v }
        }
        [void]$list.Add([pscustomobject]([ordered]@{
            label = [ordered]@{ distribution_platform = 0; id = $id; title = $title; version = Get-DotNetStringHash $id }
            status = 1; enabled = $true; enabledForDlc = @($dlc); crash_count = 0; reinstall_path = $null; staticID = $staticID
        }))
        Write-Host "Registered new local mod '$id' ($title)"
    }
    $json.mods = $list.ToArray()
    $json.mod_load_in_progress = $false
    [IO.File]::WriteAllText($modsJson, ($json | ConvertTo-Json -Depth 10), $utf8)
    [IO.File]::WriteAllText((Join-Path $runDir 'mods.json.used'), ($json | ConvertTo-Json -Depth 10), $utf8)
}

# A leftover backup means an earlier run died before its finally block: it holds the real loadout,
# so put it back first instead of overwriting it.
$backup = "$modsJson.onitest-backup"
if (Test-Path $backup) {
    Write-Warning "Leftover backup $backup from an unfinished run; restoring it before starting."
    Copy-Item $backup $modsJson -Force
} else {
    Copy-Item $modsJson $backup
}
# The game marks mods.json "mod_load_in_progress" as it boots and clears it at the main menu; a game
# closed before then leaves it set, and the next boot starts in mod safe mode with every mod off.
# A launched game closed early is the usual cause here, so neither the test loadout nor the player's
# loadout put back afterwards carries it.
$backupText = [IO.File]::ReadAllText($backup, $utf8)
[IO.File]::WriteAllText($backup, ($backupText -replace '("mod_load_in_progress"\s*:\s*)true', '${1}false'), $utf8)
# The real loadout, kept with the run, so a list lost later can be told apart from one this run lost.
Copy-Item $backup (Join-Path $runDir 'mods.json.real') -Force
$launcherStatus = 'not-started'
$t0 = Get-Date
try {
    Set-Mods
    $gameArgs = @('-applaunch', "$AppId", "--onitest-dir=$runDir", "--onitest-save=$fixtureCopy", "--onitest-timeout=$TimeoutSec", "--onitest-test-timeout=$TestTimeoutSec")
    if ($Filter) { $gameArgs += "--onitest-filter=$Filter" }
    if ($KeepGame) { $gameArgs += '--onitest-keep=1' }
    if ($Trace) { $gameArgs += '--onitest-trace=1' }
    Write-Host "Loadout: TestRunner + $($Mods -join ', ')$(if ($With) { ' + ' + ($With -join ', ') })$(if ($Steam) { ' + Workshop ' + ($Steam -join ', ') }); mode $(if ($spacedOut) { 'Spaced Out' } else { 'base game' }); fixture $Fixture"
    Write-Host "Launching: $($gameArgs -join ' ')"
    Start-Process -FilePath $SteamExe -ArgumentList $gameArgs | Out-Null
    $proc = $null
    while (-not $proc -and ((Get-Date) - $t0).TotalSeconds -lt 120) { Start-Sleep 2; $proc = Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue }
    if (-not $proc) { throw 'The game did not start within 120 s.' }
    $launcherStatus = 'exited'
    while (-not $proc.HasExited) {
        Start-Sleep 3
        if (-not $KeepGame -and (Test-Path $statusFile) -and ((Get-Date) - (Get-Item $statusFile).LastWriteTime).TotalSeconds -gt 30) {
            Stop-Process -Id $proc.Id -Force; $launcherStatus = 'killed-after-status'; break
        }
        if (-not $KeepGame -and ((Get-Date) - $t0).TotalSeconds -gt ($TimeoutSec + 180)) { Stop-Process -Id $proc.Id -Force; $launcherStatus = 'killed'; break }
        $proc.Refresh()
    }
    Start-Sleep 3
    if (Test-Path $playerLog) { Copy-Item $playerLog (Join-Path $runDir 'Player.log') -Force }
}
finally {
    Copy-Item $backup $modsJson -Force
    # A copy keeps the backup's old date; dated now, the file shows when the loadout came back.
    (Get-Item $modsJson).LastWriteTime = Get-Date
    if ((Get-FileHash $modsJson).Hash -ne (Get-FileHash $backup).Hash) {
        Write-Warning "mods.json does not match the backup after the restore; the backup is left at $backup."
    } else {
        Remove-Item $backup -Force
        Write-Host 'Restored mods.json.'
    }
}

# --- report ------------------------------------------------------------------------------------
$results = @()
if (Test-Path $resultsFile) {
    $results = @([IO.File]::ReadAllLines($resultsFile, $utf8) | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json })
}
$status = if (Test-Path $statusFile) { [IO.File]::ReadAllText($statusFile, $utf8).Trim() | ConvertFrom-Json } else { $null }
$lines = @()
foreach ($r in $results) {
    $lines += ('{0,-8} {1} ({2:F1} s){3}' -f $r.status.ToUpper(), $r.id, $r.durationSeconds, $(if ($r.message) { ': ' + $r.message } else { '' }))
    foreach ($e in $r.logErrors) { $lines += "         logged: $e" }
}
if ($status) {
    $lines += "Session $($status.status)$(if ($status.detail) { ': ' + $status.detail }); $($status.passed) passed, $($status.failed) failed, $($status.errors) errors, $($status.timeouts) timeouts, $($status.skipped) skipped of $($status.total); $($status.realSeconds) s real time$(if ($status.reloads) { "; fixture loaded again $($status.reloads) time(s)" })"
} else {
    $lines += 'No status file: the session never finished (launch, load or crash). See Player.log in the run folder.'
}
$lines += "Game $launcherStatus after $([int]((Get-Date) - $t0).TotalSeconds) s; results in $runDir"
$lines | ForEach-Object { Write-Host $_ }
[IO.File]::WriteAllLines((Join-Path $runDir 'summary.txt'), $lines, $utf8)

if (-not $status) { exit 2 }
if ($status.status -ne 'done') { exit 2 }
$bad = @($results | Where-Object { $_.status -in 'fail', 'error', 'timeout' }).Count
if ($bad -gt 0 -or $status.total -eq 0) { exit 1 }
exit 0
