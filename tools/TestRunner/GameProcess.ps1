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

# The command line that starts the game on Moono directly, not through Steam: Moono's own --log FILE
# (a Moono option, listed before the Unity and game arguments in its usage line, so it goes first;
# -LogPath, so a launch that dies leaves Moono's own account of it) and then the game's own arguments
# (the --onitest- ones), each behind its own --game-arg: Moono refuses any -- option it does not know
# ("Unknown option", exit code 2, before it opens its log), and --game-arg ARG is how one reaches the
# game unchanged. There is no --game: in standalone mode Moono gets the install from Steam, which knows
# one, starts Steam itself if it is not running, and sets the app id.
function Get-MoonoArgumentLine([string[]]$GameArgs = @(), [string]$LogPath = '') {
    $all = @(if ($LogPath) { '--log'; $LogPath }) + @($GameArgs | ForEach-Object { '--game-arg'; $_ })
    return ($all | ForEach-Object { ConvertTo-CommandLineArgument $_ }) -join ' '
}

# Where Start-Process sends OniMoono's stdout and stderr for a launch logging to -LogPath. OniMoono has no
# console, so a refusal it prints (an unknown option) is seen only through these files.
function Get-MoonoStreamPaths([string]$LogPath) {
    return [pscustomobject]@{ Out = "$LogPath.out"; Err = "$LogPath.err" }
}

# The last lines of a log, as one indented block for a message; '' when the file is missing or empty.
function Get-LogTail([string]$LogPath, [int]$Lines = 15) {
    if (-not $LogPath -or -not (Test-Path -LiteralPath $LogPath)) { return '' }
    $tail = @(Get-Content -LiteralPath $LogPath -Tail $Lines -ErrorAction SilentlyContinue)
    if ($tail.Count -eq 0) { return '' }
    return ($tail | ForEach-Object { "    $_" }) -join "`n"
}

# The exit code of a process that has exited, or $null when it cannot be read.
function Get-ExitCodeOf([System.Diagnostics.Process]$Process) {
    if (-not $Process) { return $null }
    try { if ($Process.HasExited) { return $Process.ExitCode } } catch { }
    return $null
}

# What to say when OniMoono is gone before the game's session wrote its status: the exit code and the
# tail of Moono's log. -Process is the game process found (it may be the launcher itself).
function Get-MoonoExitText($Process, [string]$LogPath, [string]$Before = "the game's session wrote its status") {
    $code = Get-ExitCodeOf $Process
    $text = "OniMoono exited with code $(if ($null -eq $code) { 'unknown' } else { $code }) before $Before."
    $tail = Get-LogTail $LogPath
    if ($tail) { $text += "`nEnd of Moono's log ($LogPath):`n$tail" }
    elseif ($LogPath) { $text += "`nMoono wrote no log at $LogPath." }
    if ($LogPath) {
        $streams = Get-MoonoStreamPaths $LogPath
        foreach ($s in @(@{ Name = 'stderr'; Path = $streams.Err }, @{ Name = 'stdout'; Path = $streams.Out })) {
            $t = Get-LogTail $s.Path
            if ($t) { $text += "`nOniMoono's $($s.Name) ($($s.Path)):`n$t" }
        }
    }
    return $text
}

# Copies a Player.log to the run folder only if it was written at or after -Since (the launch);
# a log last written before the launch belongs to an earlier run and is never taken for this one.
# Returns $true when copied.
function Copy-FreshLog([string]$Source, [string]$Destination, [datetime]$Since) {
    if (-not (Test-Path -LiteralPath $Source)) { return $false }
    if ((Get-Item -LiteralPath $Source).LastWriteTime -lt $Since) { return $false }
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
    return $true
}

# The pids of the stock game now running, taken before a Moono launch: any OxygenNotIncluded that
# appears afterwards is one Moono did not start itself.
function Get-StockGameIds {
    return @(Get-Process -Name 'OxygenNotIncluded' -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })
}

# Waits for the game a Moono launch starts: -Find is a script block that returns the OniMoono game process
# once it is up. Fails at once, instead of at the timeout, when a stock OxygenNotIncluded appears that was
# not in -StockBefore (the stock game was started, so the leg would not be on Moono), or when the launcher
# (-Launcher, from Start-Process -PassThru) exits with no game up.
function Wait-MoonoGame([System.Diagnostics.Process]$Launcher, [int[]]$StockBefore, [int]$TimeoutSec, [scriptblock]$Find, [string]$LogPath = '') {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    $exitSeen = $null
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 500
        $game = & $Find | Select-Object -First 1
        if ($game) { return $game }
        $stock = @(Get-Process -Name 'OxygenNotIncluded' -ErrorAction SilentlyContinue | Where-Object { $StockBefore -notcontains $_.Id })
        if ($stock.Count -gt 0) {
            throw "The stock game (OxygenNotIncluded, pid $($stock[0].Id)) started during the Moono launch; Moono did not start only itself. The leg is failed."
        }
        # The launcher may hand the game to a process of its own: its exit counts only if no game shows up within 3 s.
        if ($Launcher -and $Launcher.HasExited) {
            if (-not $exitSeen) { $exitSeen = Get-Date }
            elseif (((Get-Date) - $exitSeen).TotalSeconds -ge 3) {
                throw (Get-MoonoExitText $Launcher $LogPath 'the game started')
            }
        }
    }
    throw "The game did not start within $TimeoutSec s."
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
