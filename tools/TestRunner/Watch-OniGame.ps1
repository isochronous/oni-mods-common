<#
.SYNOPSIS
  The warden of a live test session: started detached by Invoke-OniTests.ps1 next to the game it
  launched, it waits for that game to exit, puts the player's mods.json back from the backup, puts
  back any mod config folder the game backed up and did not restore (a game that died mid-run;
  ModConfig.ps1) and removes the live-session folder, so a later launcher knows the session is over.

  The game ends a live session itself (no run for 15 s, or a quit request). If it stops answering,
  the warden ends it: idle in its state file for over a minute, or busy past its own deadline plus
  three minutes.
#>
param(
    [Parameter(Mandatory)][int]$GamePid,
    [Parameter(Mandatory)][string]$ModsJson,
    [Parameter(Mandatory)][string]$Backup,
    [Parameter(Mandatory)][string]$LiveDir,
    [string]$ConfigRoot = ''
)
$ErrorActionPreference = 'Continue'
. (Join-Path $PSScriptRoot 'ModConfig.ps1')
$log = Join-Path (Split-Path $LiveDir -Parent) 'warden.log'
function Write-Log([string]$line) { Add-Content -LiteralPath $log -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') [$GamePid] $line" }

Write-Log 'watching'
$proc = Get-Process -Id $GamePid -ErrorAction SilentlyContinue
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
Write-Log 'game exited'

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
Remove-Item -LiteralPath $LiveDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Log 'live session closed'
