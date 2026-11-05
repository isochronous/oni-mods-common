# The test game's process, for the launcher (Invoke-OniTests.ps1) and the warden (Watch-OniGame.ps1).
# Steam starts the game as OxygenNotIncluded, or, with oni-moono's launch options, as OniMoono: Moono's
# player runs the game in its own process (its Restarter only restarts it). An OniMoono process counts
# only when its command line holds the harness's --onitest- arguments, so a Moono session the player
# started by hand is never taken for the test game. With -Id, that process if it is the game.
function Get-GameProcess([int]$Id = 0) {
    $candidates = if ($Id -gt 0) {
        @(Get-Process -Id $Id -ErrorAction SilentlyContinue)
    } else {
        @(Get-Process -Name 'OxygenNotIncluded', 'OniMoono' -ErrorAction SilentlyContinue)
    }
    foreach ($p in $candidates) {
        if ($p.ProcessName -eq 'OxygenNotIncluded') {
            $p
        } elseif ($p.ProcessName -eq 'OniMoono') {
            $line = (Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $($p.Id)" -ErrorAction SilentlyContinue).CommandLine
            if ($line -like '*--onitest-*') { $p }
        }
    }
}
