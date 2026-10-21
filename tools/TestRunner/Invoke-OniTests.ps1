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
  disk outside the run folder, and the colony summaries and timelapse images the game writes to
  RetiredColonies, so the fixture and the player's colonies are untouched; the summary has a line
  saying whether anything new appeared in save_files, cloud_save_files or RetiredColonies. The shared
  config folder of each mod in -Mods and -With (mods\config\<name>) is backed up by the game when
  the run starts and put back when it ends, so a test may write it (ModConfig.cs, ModConfig.ps1).

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
.PARAMETER Mode
  auto (default: the mode the player left the game in), base or spacedout. Another mode than the
  player's is set in kplayerprefs.yaml for the run and put back when the game exits (GameMode.ps1).
  A mode with no fixture save runs Worldgen and NoColony tests only.

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
    [ValidateSet('auto', 'base', 'spacedout')][string]$Mode = 'auto',
    [string]$OutDir = (Join-Path $PSScriptRoot 'runs'),
    [string]$SteamExe = 'C:\Program Files (x86)\Steam\steam.exe',
    [int]$AppId = 457140
)
$ErrorActionPreference = 'Stop'
# Started with pwsh -File, a comma-separated argument arrives as one string: split the list parameters.
$Mods = @($Mods | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$With = @($With | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$Steam = @($Steam | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
# Each name is also a folder under mods\local and mods\config that the run backs up and puts back:
# "..", "." or a path would make that another folder.
foreach ($name in @($Mods) + @($With)) {
    if ($name -notmatch '^[A-Za-z0-9_-]+(\.[A-Za-z0-9_-]+)*$') { throw "'$name' is not a mod folder name (letters, digits, '_' and '-', with dots only between them)." }
}

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
$configRoot = Join-Path $klei 'mods\config'
$utf8 = New-Object System.Text.UTF8Encoding($false)
. (Join-Path $PSScriptRoot 'ModConfig.ps1')
. (Join-Path $PSScriptRoot 'GameMode.ps1')
# PLib names a mod's shared config folder after its assembly, which is the local folder name here.
$configNames = @($Mods + $With | Select-Object -Unique)

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

# --- the live session --------------------------------------------------------------------------
# A game this script launched stays up after its run and takes further runs with the same loadout
# from $liveDir\queue (see Session.cs); its warden (Watch-OniGame.ps1) puts the player's mods.json
# back when it exits and removes $liveDir. Outside every repo, so every checkout of common shares it.
$liveDir = Join-Path $env:LOCALAPPDATA 'IsochronousOniTests\live'
$livePidFile = Join-Path $liveDir 'game.pid'
function Get-LiveGame {
    if (-not (Test-Path -LiteralPath $livePidFile)) { return $null }
    $id = 0
    if (-not [int]::TryParse((Get-Content -LiteralPath $livePidFile -Raw).Trim(), [ref]$id)) { return $null }
    return Get-Process -Id $id -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -eq 'OxygenNotIncluded' }
}
# A config backup with no game to put it back: a run that died before its end. It holds the
# player's settings, so it goes back before anything reads or hashes the config.
function Restore-LeftoverConfig {
    foreach ($name in @(Restore-ModConfigBackup $configRoot)) {
        Write-Warning "Leftover config backup of $name from an unfinished run; restored it."
    }
}
# Starts the warden outside this script's process tree. A run stopped from outside (this pwsh killed
# with its tree) must not take the warden with it: it is the one that puts mods.json, the config, the
# mode and the save pointer back once the game exits. A child made with Start-Process is in the tree;
# a process made through WMI's Win32_Process.Create has WmiPrvSE as its parent, so it is not.
# Returns the warden's process id.
function Start-Warden([string[]]$Arguments) {
    $pwsh = (Get-Command pwsh).Source
    $quoted = ($Arguments | ForEach-Object { ConvertTo-CommandLineArgument $_ }) -join ' '
    try {
        $startup = New-CimInstance -ClassName Win32_ProcessStartup -ClientOnly -Property @{ ShowWindow = [uint16]0 }
        $made = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = "$(ConvertTo-CommandLineArgument $pwsh) $quoted"; ProcessStartupInformation = $startup }
        if ($made.ReturnValue -eq 0) { return [int]$made.ProcessId }
        Write-Warning "WMI could not start the warden (code $($made.ReturnValue)); starting it as a child of this script."
    } catch {
        Write-Warning "WMI could not start the warden ($_); starting it as a child of this script."
    }
    return (Start-Process -FilePath $pwsh -WindowStyle Hidden -ArgumentList $quoted -PassThru).Id
}

# One argument as the Windows command-line parser reads it back: quoted when it holds a space, a tab
# or a quote, or is empty; inside quotes a quote is escaped, and backslashes are doubled where they
# come before a quote, the closing one included ("C:\dir\" would otherwise swallow it).
function ConvertTo-CommandLineArgument([string]$Value) {
    if ($Value -ne '' -and $Value -notmatch '[\s"]') { return $Value }
    $escaped = [regex]::Replace($Value, '(\\*)"', [Text.RegularExpressions.MatchEvaluator]{ param($m) $m.Groups[1].Value * 2 + '\"' })
    $escaped = [regex]::Replace($escaped, '(\\+)$', [Text.RegularExpressions.MatchEvaluator]{ param($m) $m.Groups[1].Value * 2 })
    return '"' + $escaped + '"'
}

# The wardens running now (pwsh processes running Watch-OniGame.ps1), from any checkout of common.
function Get-Warden {
    return @(Get-CimInstance -ClassName Win32_Process -Filter "Name = 'pwsh.exe'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -like '*Watch-OniGame.ps1*' })
}
# What Restore-GameMode and Restore-SaveKey did, said as a warning (a leftover) or a line.
function Write-PrefsRestore($Mode, $Key, [switch]$Leftover) {
    $say = if ($Leftover) { { param($text) Write-Warning $text } } else { { param($text) Write-Host $text } }
    $from = if ($Leftover) { ' left by an unfinished run' } else { '' }
    if ($Mode.Action -eq 'restored') { & $say "Put the game's mode$from back (EXPANSION1_ID.ENABLED $($Mode.Value))." }
    elseif ($Mode.Action -eq 'dropped') { Write-Warning "A kept game mode ($($Mode.Value))$from was not put back: kplayerprefs.yaml holds $($Mode.Current), not the mode the run set, so it was chosen since." }
    if ($Key.Action -eq 'restored') { & $say "Put the player's last-save pointer (SaveFilenameKey/)$from back." }
    elseif ($Key.Action -eq 'dropped') {
        $now = if ($Key.Current.StartsWith('present:')) { 'points at' + $Key.Current.Substring('present:'.Length) } else { 'has no SaveFilenameKey/ line' }
        Write-Warning "A kept last-save pointer$from was not put back: kplayerprefs.yaml $now, not a test save, so it was changed since."
    }
}

# --- leftovers of a run that died ----------------------------------------------------------------
# Put back only while no game runs at all. A running game holds kplayerprefs.yaml in memory and
# writes it back over a restore, and a game whose launcher was killed may still be running its tests
# on the config it backed up. A warden with no game is one whose launcher was killed while the game
# was starting, or one finishing its restore: it owns the player's files until it exits.
if (@(Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue).Count -eq 0 -and (Get-Warden).Count -gt 0) {
    Write-Host "$(Get-Date -Format HH:mm:ss) A test session's warden is still running with no game; waiting for it to finish."
    $deadline = (Get-Date).AddSeconds(200)
    while ((Get-Warden).Count -gt 0 -and @(Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue).Count -eq 0 -and (Get-Date) -lt $deadline) { Start-Sleep 1 }
}
if (@(Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue).Count -eq 0) {
    if (Test-Path -LiteralPath $liveDir) {
        # A live session whose game is gone and whose warden did not clear up: the leftover-backup
        # checks restore the player's loadout.
        Write-Warning "Stale live-session folder $liveDir (its game is gone); removing it."
        Remove-Item -LiteralPath $liveDir -Recurse -Force
    }
    Restore-LeftoverConfig
    Write-PrefsRestore (Restore-GameMode $prefs) (Restore-SaveKey $prefs) -Leftover
}

# --- what to run -------------------------------------------------------------------------------
$running = Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue
$live = Get-LiveGame
if (-not $BuildOnly -and $running -and -not ($live -and @($running).Count -eq 1 -and @($running)[0].Id -eq $live.Id)) { throw 'The game is already running; close it first.' }
if (-not (Test-Path $SteamExe)) { throw "Steam not found at $SteamExe" }
$managed = Join-Path (Get-GameFolder) 'OxygenNotIncluded_Data\Managed'

$spacedOut = switch ($Mode) { 'base' { $false } 'spacedout' { $true } default { Get-PlayerSpacedOut $prefs } }
$dlc = if ($spacedOut) { 'EXPANSION1_ID' } else { '' }
$noFixture = $false
if (-not $Fixture) {
    # Fixture saves are not committed, so a submodule checkout of this repo has none: fall back to
    # the workspace's own oni-mods-common checkout.
    $name = 'fixtures\' + $(if ($spacedOut) { 'testbed-so.sav' } else { 'testbed-base.sav' })
    foreach ($dir in $PSScriptRoot, (Join-Path $workspace 'oni-mods-common\tools\TestRunner')) {
        $candidate = Join-Path $dir $name
        if (Test-Path -LiteralPath $candidate) { $Fixture = $candidate; break }
    }
    if (-not $Fixture) {
        # Worldgen and NoColony tests need no save; a test that does ends the run as no-save.
        $noFixture = $true
        Write-Warning "No fixture save for $(if ($spacedOut) { 'Spaced Out' } else { 'the base game' }) ($name, see fixtures/README.md): only Worldgen and NoColony tests can run."
    }
} elseif (-not (Test-Path -LiteralPath $Fixture)) { throw "Fixture save not found: $Fixture (see fixtures/README.md)" }
if (-not $noFixture) {
    $header = [Text.Encoding]::UTF8.GetString([IO.File]::ReadAllBytes($Fixture)[0..8191])
    $fixtureSpacedOut = $header -match '"dlcIds":\[[^\]]*EXPANSION1_ID'
    if ($fixtureSpacedOut -ne $spacedOut) {
        throw "The fixture is a $(if ($fixtureSpacedOut) { 'Spaced Out' } else { 'base game' }) save but the run is in $(if ($spacedOut) { 'Spaced Out' } else { 'base game' }) mode; pass -Mode or another -Fixture."
    }
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
if (-not $noFixture) { Copy-Item -LiteralPath $Fixture -Destination $fixtureCopy }
$resultsFile = Join-Path $runDir 'results.jsonl'
$statusFile = "$resultsFile.status"

# --- loadout -----------------------------------------------------------------------------------
# A mods.json entry for a downloaded Workshop item the game has not registered yet, or $null: its
# folder under mods\Steam, its title and staticID from mod.yaml, and as label.version the item's
# "timeupdated" from Steam's download record (appworkshop_457140.acf), which is what the game stores.
function New-WorkshopEntry([string]$id) {
    $folder = Join-Path $klei "mods\Steam\$id"
    if (-not (Test-Path -LiteralPath $folder)) { return $null }
    $acf = Join-Path (Split-Path (Split-Path (Get-GameFolder) -Parent) -Parent) 'workshop\appworkshop_457140.acf'
    if (-not (Test-Path -LiteralPath $acf)) { return $null }
    $m = [regex]::Match((Get-Content -LiteralPath $acf -Raw), '"' + $id + '"\s*\{[^}]*?"timeupdated"\s*"(\d+)"')
    if (-not $m.Success) { return $null }
    $title = $id; $staticID = $id
    $yaml = Join-Path $folder 'mod.yaml'
    if (Test-Path -LiteralPath $yaml) {
        $text = Get-Content -LiteralPath $yaml -Raw
        $v = Get-YamlScalar $text 'title';    if ($v) { $title = $v }
        $v = Get-YamlScalar $text 'staticID'; if ($v) { $staticID = $v }
    }
    return [pscustomobject]([ordered]@{
        label = [ordered]@{ distribution_platform = 1; id = $id; title = $title; version = [int64]$m.Groups[1].Value }
        status = 1; enabled = $true; enabledForDlc = @($dlc); crash_count = 0; reinstall_path = $null; staticID = $staticID
    })
}

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
    $list = New-Object System.Collections.ArrayList
    foreach ($m in $json.mods) { [void]$list.Add($m) }
    foreach ($id in $Steam) {
        if ($seen["steam:$id"]) { continue }
        # Subscribed and downloaded, but the game has not been started since: mods.json has no
        # entry yet. Registered as the game would register it, with Steam's time-updated as its version.
        $entry = New-WorkshopEntry $id
        if ($null -eq $entry) { throw "Workshop mod $id is not installed (not in mods.json, and no mods\Steam\$id folder with a Steam download record)." }
        [void]$list.Add($entry)
        Write-Host "Registered Workshop mod $id ($($entry.label.title)), not yet in mods.json"
    }
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

# What the running game would have to match to take this run: the mode, the mods and Workshop
# items enabled, the trace switch (patches made at boot), and every file of each local mod and of
# the harness as deployed, with each mod's shared config. Test assemblies are not part of it: the
# live game loads them fresh for every run.
function Get-LoadoutKey {
    $parts = @("dlc=$dlc", "trace=$([bool]$Trace)", "mods=$((@($Mods) | Sort-Object) -join ',')", "with=$((@($With) | Sort-Object) -join ',')", "steam=$((@($Steam) | Sort-Object) -join ',')")
    foreach ($m in (@('TestRunner') + $Mods + $With | Sort-Object -Unique)) {
        $roots = @((Join-Path $klei "mods\local\$m"), (Join-Path $configRoot $m))
        foreach ($root in $roots) {
            if (-not (Test-Path -LiteralPath $root)) { continue }
            foreach ($f in Get-ChildItem -LiteralPath $root -Recurse -File | Sort-Object FullName) {
                $parts += "$($f.FullName.Substring($klei.Length))=$((Get-FileHash -LiteralPath $f.FullName -Algorithm SHA1).Hash)"
            }
        }
    }
    return ($parts -join "`n")
}

# The run's request for a live game, written whole and then renamed so the game never reads half.
function Write-Request([string]$name, [string[]]$lines) {
    $queue = Join-Path $liveDir 'queue'
    New-Item -ItemType Directory -Force $queue | Out-Null
    $tmp = Join-Path $queue "$name.tmp"
    [IO.File]::WriteAllLines($tmp, $lines, $utf8)
    Move-Item -LiteralPath $tmp -Destination (Join-Path $queue "$name.req") -Force
    return (Join-Path $queue "$name.taken")
}

# Ends the live session (a quit request, then the process if it does not go) and waits for its
# warden to put the player's loadout back and remove the folder.
function Stop-LiveSession([string]$why) {
    $game = Get-LiveGame
    Write-Host "Ending the live game session: $why"
    if ($game) {
        [void](Write-Request 'quit' @('quit'))
        if (-not $game.WaitForExit(60000)) { Stop-Process -Id $game.Id -Force -ErrorAction SilentlyContinue }
    }
    $deadline = (Get-Date).AddSeconds(30)
    while ((Test-Path -LiteralPath $liveDir) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250 }
    if (Test-Path -LiteralPath $liveDir) { Remove-Item -LiteralPath $liveDir -Recurse -Force }
}

# The harness blocks saves, colony summaries and timelapse images outside the run folder
# (Patches.cs). A listing of the player's save and colony folders before and after the run shows
# anything new all the same, in the summary; nothing in them is ever deleted.
$playerFolders = @('save_files', 'cloud_save_files', 'RetiredColonies') | ForEach-Object { Join-Path $klei $_ }
function Get-PlayerEntries {
    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    foreach ($root in $playerFolders) {
        if (-not (Test-Path -LiteralPath $root)) { continue }
        foreach ($e in Get-ChildItem -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue) { [void]$set.Add($e.FullName) }
    }
    return , $set
}
$playerBefore = Get-PlayerEntries

$t0 = Get-Date
$launcherStatus = 'not-started'
$loadoutKey = Get-LoadoutKey
$submitted = $false
$wardenStarted = $false
$wardenPid = 0
$proc = $null
$requestLines = @("dir=$runDir", "save=$fixtureCopy", "filter=$Filter", "timeout=$TimeoutSec", "test-timeout=$TestTimeoutSec", "trace=$(if ($Trace) { '1' } else { '0' })", "config=$($configNames -join ';')")

# --- a live game with this loadout takes the run -----------------------------------------------
if (Get-LiveGame) {
    $keyFile = Join-Path $liveDir 'key.txt'
    $liveKey = if (Test-Path -LiteralPath $keyFile) { [IO.File]::ReadAllText($keyFile, $utf8) } else { '' }
    $state = $(try { (Get-Content -LiteralPath (Join-Path $liveDir 'state') -Raw).Trim() } catch { '' })
    if ($KeepGame) {
        Stop-LiveSession '-KeepGame starts a game of its own'
    } elseif ($liveKey -ne $loadoutKey) {
        Stop-LiveSession 'this run needs another loadout (mods, mode, or a rebuilt mod)'
    } elseif ($state -notlike 'idle*') {
        Stop-LiveSession "the game is not idle ($state)"
    } else {
        $taken = Write-Request $runDir.Split('\')[-1] $requestLines
        $deadline = (Get-Date).AddSeconds(5)
        while (-not (Test-Path -LiteralPath $taken) -and (Get-Date) -lt $deadline -and (Get-LiveGame)) { Start-Sleep -Milliseconds 100 }
        if (Test-Path -LiteralPath $taken) {
            $submitted = $true
            Write-Host "The running game took the run (same loadout); no restart."
        } else {
            Stop-LiveSession 'the game did not take the run (it was ending)'
        }
    }
}

if ($submitted) {
    $launcherStatus = 'reused'
    $game = Get-LiveGame
    $deadline = (Get-Date).AddSeconds($TimeoutSec + 120)
    while (-not (Test-Path -LiteralPath $statusFile) -and $game -and -not $game.HasExited -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250; $game.Refresh() }
    if (-not (Test-Path -LiteralPath $statusFile)) { $launcherStatus = if ($game -and -not $game.HasExited) { 'reused, no status in time' } else { 'reused, exited' } }
    if (Test-Path $playerLog) { Copy-Item $playerLog (Join-Path $runDir 'Player.log') -Force }
} else {
    # --- a fresh game ------------------------------------------------------------------------------
    # A leftover backup means an earlier run died before its restore: it holds the real loadout,
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
    # A live game ended above may have left a config backup (its warden normally puts it back); the
    # key the new game is given must hash the player's config.
    Restore-LeftoverConfig
    $loadoutKey = Get-LoadoutKey
    try {
        # The mode left by a live game ended above is the player's again (its warden put it back);
        # one still switched is put back first, so the value kept below is the player's.
        Write-PrefsRestore (Restore-GameMode $prefs) (Restore-SaveKey $prefs) -Leftover
        Save-SaveKey $prefs
        if (Switch-GameMode $prefs $spacedOut) { Write-Host "Switched the game to $(if ($spacedOut) { 'Spaced Out' } else { 'the base game' }) for this run; the player's mode goes back when it exits." }
        Set-Mods
        $gameArgs = @('-applaunch', "$AppId", "--onitest-dir=$runDir", "--onitest-save=$fixtureCopy", "--onitest-timeout=$TimeoutSec", "--onitest-test-timeout=$TestTimeoutSec")
        if ($Filter) { $gameArgs += "--onitest-filter=$Filter" }
        if ($configNames) { $gameArgs += "--onitest-config=$($configNames -join ';')" }
        if ($KeepGame) { $gameArgs += '--onitest-keep=1' }
        if ($Trace) { $gameArgs += '--onitest-trace=1' }
        if (-not $KeepGame) {
            New-Item -ItemType Directory -Force (Join-Path $liveDir 'queue') | Out-Null
            [IO.File]::WriteAllText((Join-Path $liveDir 'key.txt'), $loadoutKey, $utf8)
            $gameArgs += "--onitest-server=$liveDir"
        }
        Write-Host "Loadout: TestRunner + $($Mods -join ', ')$(if ($With) { ' + ' + ($With -join ', ') })$(if ($Steam) { ' + Workshop ' + ($Steam -join ', ') }); mode $(if ($spacedOut) { 'Spaced Out' } else { 'base game' }); fixture $(if ($noFixture) { 'none' } else { $Fixture })"
        if (-not $KeepGame) {
            # The warden starts before the game, so a launcher killed while the game is starting
            # still leaves one: it finds the game by the pid file or as the newest game process, and
            # restores once the game exits (or at once if no game comes up).
            $wardenPid = Start-Warden @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $PSScriptRoot 'Watch-OniGame.ps1'),
                '-PidFile', $livePidFile, '-StartTimeoutSec', '150', '-ModsJson', $modsJson, '-Backup', $backup, '-LiveDir', $liveDir, '-ConfigRoot', $configRoot, '-Prefs', $prefs)
            $wardenStarted = $true
        }
        Write-Host "Launching: $($gameArgs -join ' ')"
        Start-Process -FilePath $SteamExe -ArgumentList $gameArgs | Out-Null
        while (-not $proc -and ((Get-Date) - $t0).TotalSeconds -lt 120) { Start-Sleep 2; $proc = Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue | Select-Object -First 1 }
        if (-not $proc) { throw 'The game did not start within 120 s.' }
        $launcherStatus = 'exited'
        if (-not $KeepGame) {
            # From here the warden owns the restore: the game outlives this script.
            [IO.File]::WriteAllText($livePidFile, "$($proc.Id)", $utf8)
            $launcherStatus = 'live'
            while (-not (Test-Path -LiteralPath $statusFile) -and -not $proc.HasExited -and ((Get-Date) - $t0).TotalSeconds -lt ($TimeoutSec + 180)) { Start-Sleep -Milliseconds 250; $proc.Refresh() }
            if ($proc.HasExited) { $launcherStatus = 'exited' }
        } else {
            while (-not $proc.HasExited) { Start-Sleep 3; $proc.Refresh() }
        }
        Start-Sleep 1
        if (Test-Path $playerLog) { Copy-Item $playerLog (Join-Path $runDir 'Player.log') -Force }
    }
    finally {
        if ($wardenStarted -and -not $proc) {
            # No game came up (or this script failed before it saw one): the restore is this script's,
            # and the warden, still waiting for a game, must not make it a second time.
            if ($wardenPid -gt 0) { Stop-Process -Id $wardenPid -Force -ErrorAction SilentlyContinue }
            $wardenStarted = $false
        }
        if (-not $wardenStarted) {
            Copy-Item $backup $modsJson -Force
            # A copy keeps the backup's old date; dated now, the file shows when the loadout came back.
            (Get-Item $modsJson).LastWriteTime = Get-Date
            if ((Get-FileHash $modsJson).Hash -ne (Get-FileHash $backup).Hash) {
                Write-Warning "mods.json does not match the backup after the restore; the backup is left at $backup."
            } else {
                Remove-Item $backup -Force
                Write-Host 'Restored mods.json.'
            }
            if (Test-Path -LiteralPath $liveDir) { Remove-Item -LiteralPath $liveDir -Recurse -Force -ErrorAction SilentlyContinue }
            # The game puts the config back at the end of its run; one that died did not.
            if (-not (Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue)) {
                Restore-LeftoverConfig
                Write-PrefsRestore (Restore-GameMode $prefs) (Restore-SaveKey $prefs)
            }
        } else {
            Write-Host "The game stays up $([int]15) s for another run with this loadout; its warden puts mods.json back when it exits."
        }
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
$playerAfter = Get-PlayerEntries
$playerNew = @($playerAfter | Where-Object { -not $playerBefore.Contains($_) } | Sort-Object)
if ($playerNew.Count -eq 0) {
    $lines += "Player's folders: nothing new in $(($playerFolders | ForEach-Object { Split-Path $_ -Leaf }) -join ', ')"
} else {
    $lines += "Player's folders: $($playerNew.Count) new entr$(if ($playerNew.Count -eq 1) { 'y' } else { 'ies' }) since the run started (not deleted; check whether this run wrote them):"
    $lines += @($playerNew | ForEach-Object { "         $($_.Substring($klei.Length + 1))" })
}
$lines += "Game $launcherStatus after $([int]((Get-Date) - $t0).TotalSeconds) s; results in $runDir"
$lines | ForEach-Object { Write-Host $_ }
[IO.File]::WriteAllLines((Join-Path $runDir 'summary.txt'), $lines, $utf8)

if (-not $status) { exit 2 }
if ($status.status -ne 'done') { exit 2 }
$bad = @($results | Where-Object { $_.status -in 'fail', 'error', 'timeout' }).Count
if ($bad -gt 0 -or $status.total -eq 0) { exit 1 }
exit 0
