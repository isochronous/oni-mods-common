# The process of the game a tool launched, for the launchers in tools/ (TestRunner's Invoke-OniTests.ps1
# and Watch-OniGame.ps1, SaveRunner, WorldgenDriver, ModTest), each telling its game by its -Marker
# (ModTest, which launches with no arguments, uses -Any and the start time instead).
# The stock game runs as OxygenNotIncluded. oni-moono's player, started directly (TestRunner's and
# ModTest's -Runtime Moono, Start-MoonoGame below) or from Steam's launch options, runs the game in its
# own process, OniMoono (its Restarter only restarts it). An OniMoono process counts
# only when its command line holds the -Marker arguments (by default TestRunner's --onitest-), so a
# Moono session the player started by hand is never taken for the launched game. With -Id, that
# process if it is the game. With -Any, every OniMoono counts: the checks for an open game, before a
# launch or a restore, use it, so a Moono game the player started blocks a run as an open
# OxygenNotIncluded does.
# The other launchers in tools/ dot-source this file too and pass their own arguments' prefix as
# -Marker (SaveRunner --saverun, WorldgenDriver --worldgen-).
# The OniMoono pids whose command line could not be read, each warned about once (the wait loops
# call this every few hundred milliseconds).
$GameProcessUnread = @{}
function Get-GameProcess([int]$Id = 0, [switch]$Any, [string]$Marker = '--onitest-') {
    $candidates = if ($Id -gt 0) {
        @(Get-Process -Id $Id -ErrorAction SilentlyContinue)
    } else {
        @(Get-Process -Name 'OxygenNotIncluded', 'OniMoono' -ErrorAction SilentlyContinue)
    }
    foreach ($p in $candidates) {
        if ($p.ProcessName -eq 'OxygenNotIncluded') {
            $p
        } elseif ($p.ProcessName -eq 'OniMoono' -and $Any) {
            $p
        } elseif ($p.ProcessName -eq 'OniMoono') {
            $line = (Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $($p.Id)" -ErrorAction SilentlyContinue).CommandLine
            if ($line -like "*$Marker*") { $p }
            elseif (-not $line -and -not $p.HasExited -and -not $GameProcessUnread.ContainsKey($p.Id)) {
                $GameProcessUnread[$p.Id] = $true
                Write-Warning "Cannot read the command line of OniMoono process $($p.Id); it is not taken for the launched game."
            }
        }
    }
}

# The game's install folder: ONI_GAME_FOLDER, else the first Steam library that has it.
function Get-GameFolder {
    if ($env:ONI_GAME_FOLDER -and (Test-Path $env:ONI_GAME_FOLDER)) { return $env:ONI_GAME_FOLDER }
    foreach ($candidate in 'F:\SteamLibrary\steamapps\common\OxygenNotIncluded', 'C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded') {
        if (Test-Path $candidate) { return $candidate }
    }
    throw 'Game folder not found; set ONI_GAME_FOLDER.'
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

# oni-moono's player: -Path (a tool's -MoonoExe), else ONI_MOONO_EXE, else F:\modding\oni-moono\player.
function Get-MoonoExe([string]$Path = '') {
    $exe = if ($Path) { $Path } elseif ($env:ONI_MOONO_EXE) { $env:ONI_MOONO_EXE } else { 'F:\modding\oni-moono\player\OniMoono.exe' }
    if (-not (Test-Path -LiteralPath $exe)) { throw "oni-moono's player not found at $exe; pass -MoonoExe or set ONI_MOONO_EXE." }
    return $exe
}

# The command line that starts the game on Moono directly, not through Steam: --game names the install
# (so Moono never asks which one), and every other argument is passed to the game. Moono starts Steam
# itself if it is not running, and sets the app id.
function Get-MoonoArgumentLine([string]$GameFolder, [string[]]$GameArgs = @()) {
    return ((@('--game', $GameFolder) + @($GameArgs)) | ForEach-Object { ConvertTo-CommandLineArgument $_ }) -join ' '
}

# What ran, from the first lines of a Player.log: Moono writes "[engine] Moono starting: commit=<sha> ..."
# on line 1 and its launch mode on line 2; the stock game writes neither.
function Get-LogRuntime([string]$LogPath) {
    $head = @(if ($LogPath -and (Test-Path -LiteralPath $LogPath)) { Get-Content -LiteralPath $LogPath -TotalCount 2 })
    if ($head.Count -eq 0) { return [pscustomobject]@{ Runtime = ''; Commit = ''; Launch = ''; Line1 = '' } }
    $m = [regex]::Match($head[0], 'Moono starting: commit=(\w+)')
    if (-not $m.Success) { return [pscustomobject]@{ Runtime = 'Stock'; Commit = ''; Launch = ''; Line1 = $head[0] } }
    $launch = if ($head.Count -gt 1 -and $head[1] -match 'launch: (.+)$') { $Matches[1] } else { '' }
    return [pscustomobject]@{ Runtime = 'Moono'; Commit = $m.Groups[1].Value; Launch = $launch; Line1 = $head[0] }
}
