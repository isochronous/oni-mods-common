<#
.SYNOPSIS
  The warden of a live test session: started detached by Invoke-OniTests.ps1 next to the game it
  launched, it waits for that game to exit, puts the player's mods.json back from the backup, puts
  back any mod config folder the game backed up and did not restore (a game that died mid-run;
  ModConfig.ps1), puts back the game's mode and last-save pointer if the run changed them
  (GameMode.ps1) and removes the live-session folder, so a later launcher knows the session is over.

  The launcher starts it before it starts the game, with no -GamePid: the warden then waits up to
  -StartTimeoutSec for the game, by the pid the launcher writes to -PidFile or, if the launcher was
  killed before it could, by an OxygenNotIncluded process started after the warden (whose pid it then
  writes there itself). No game in that time: it restores at once. A launcher that gave up on the
  game itself ends the warden and restores on its own.

  The game ends a live session itself (no run for 15 s, or a quit request). If it stops answering,
  the warden ends it: idle in its state file for over a minute, or busy past its own deadline plus
  three minutes.
#>
param(
    [int]$GamePid = 0,
    [Parameter(Mandatory)][string]$ModsJson,
    [Parameter(Mandatory)][string]$Backup,
    [Parameter(Mandatory)][string]$LiveDir,
    [string]$ConfigRoot = '',
    [string]$Prefs = '',
    [string]$PidFile = '',
    [int]$StartTimeoutSec = 150
)
$ErrorActionPreference = 'Continue'
. (Join-Path $PSScriptRoot 'ModConfig.ps1')
. (Join-Path $PSScriptRoot 'GameMode.ps1')
$log = Join-Path (Split-Path $LiveDir -Parent) 'warden.log'
function Write-Log([string]$line) { Add-Content -LiteralPath $log -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') [$GamePid] $line" }

# A game started by the launcher: the pid it wrote, else an OxygenNotIncluded process younger than
# this warden (the launcher found no game running before it started one).
function Find-Game([datetime]$Since) {
    if ($PidFile -and (Test-Path -LiteralPath $PidFile)) {
        $id = 0
        if ([int]::TryParse(((Get-Content -LiteralPath $PidFile -Raw -ErrorAction SilentlyContinue) + '').Trim(), [ref]$id)) {
            $p = Get-Process -Id $id -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -eq 'OxygenNotIncluded' }
            if ($p) { return $p }
        }
    }
    return Get-Process OxygenNotIncluded -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -ge $Since } | Select-Object -First 1
}

if ($GamePid -gt 0) {
    Write-Log 'watching'
    $proc = Get-Process -Id $GamePid -ErrorAction SilentlyContinue
} else {
    $since = (Get-Date).AddSeconds(-5)
    $deadline = (Get-Date).AddSeconds($StartTimeoutSec)
    Write-Log "waiting up to $StartTimeoutSec s for the game"
    $proc = $null
    while (-not $proc -and (Get-Date) -lt $deadline) {
        $proc = Find-Game $since
        if (-not $proc) { Start-Sleep -Milliseconds 500 }
    }
    if ($proc) {
        $GamePid = $proc.Id
        Write-Log 'watching'
        if ($PidFile -and -not (Test-Path -LiteralPath $PidFile) -and (Test-Path -LiteralPath (Split-Path $PidFile -Parent))) {
            # The launcher was killed before it wrote it; a later launcher finds the session by it.
            try { [IO.File]::WriteAllText($PidFile, "$GamePid") } catch { Write-Log "cannot write $PidFile`: $_" }
        }
    } else {
        Write-Log "no game started within $StartTimeoutSec s; restoring"
    }
}
$stateFile = Join-Path $LiveDir 'state'
while ($proc -and -not $proc.HasExited) {
    Start-Sleep -Milliseconds 250
    $now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $state = if (Test-Path -LiteralPath $stateFile) { (Get-Content -LiteralPath $stateFile -Raw -ErrorAction SilentlyContinue) } else { '' }
    if ($state -match '^idle (\d+)' -and $now - [long]$Matches[1] -gt 60) {
        Write-Log 'idle for over a minute without quitting; ending it'
        Stop-Process -Id $GamePid -Force -ErrorAction SilentlyContinue
    } elseif ($state -match '^busy (\d+)' -and $now - [long]$Matches[1] -gt 180) {
        Write-Log 'busy three minutes past its deadline; ending it'
        Stop-Process -Id $GamePid -Force -ErrorAction SilentlyContinue
    }
    $proc.Refresh()
}
if ($GamePid -gt 0) { Write-Log 'game exited' }

# The same restore as the launcher's: the backup back in place, dated now, checked, then removed.
if (Test-Path -LiteralPath $Backup) {
    Copy-Item -LiteralPath $Backup -Destination $ModsJson -Force
    (Get-Item -LiteralPath $ModsJson).LastWriteTime = Get-Date
    if ((Get-FileHash -LiteralPath $ModsJson).Hash -eq (Get-FileHash -LiteralPath $Backup).Hash) {
        Remove-Item -LiteralPath $Backup -Force
        Write-Log 'restored mods.json'
    } else {
        Write-Log "mods.json does not match the backup after the restore; the backup is left at $Backup"
    }
} else {
    Write-Log "no backup at $Backup to restore"
}
if ($ConfigRoot) {
    try {
        foreach ($name in @(Restore-ModConfigBackup $ConfigRoot)) { Write-Log "restored the config of $name (the game did not)" }
    } catch {
        Write-Log "cannot restore the config backups in $ModConfigBackupRoot`: $_"
    }
}
if ($Prefs) {
    try {
        $mode = Restore-GameMode $Prefs
        if ($mode.Action -eq 'restored') { Write-Log "put the game's mode back (EXPANSION1_ID.ENABLED $($mode.Value))" }
        elseif ($mode.Action -eq 'dropped') { Write-Log "kept mode $($mode.Value) dropped: kplayerprefs.yaml holds $($mode.Current), not the run's mode, so it was set since" }
    } catch {
        Write-Log "cannot put the game's mode back: $_"
    }
    try {
        $key = Restore-SaveKey $Prefs
        if ($key.Action -eq 'restored') { Write-Log "put the player's last-save pointer back (SaveFilenameKey/)" }
        elseif ($key.Action -eq 'dropped') { Write-Log "kept last-save pointer dropped: kplayerprefs.yaml holds $($key.Current), not a test save, so the player's game wrote it since" }
    } catch {
        Write-Log "cannot put the last-save pointer back: $_"
    }
}
Remove-Item -LiteralPath $LiveDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Log 'live session closed'
