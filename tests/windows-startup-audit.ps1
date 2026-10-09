# Run after signing into Windows and opening Codex. Read-only system checks.
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$app = Join-Path $env:LOCALAPPDATA 'Programs\CodexDashboard\CodexDashboard.exe'
$prefs = Get-Content (Join-Path $env:LOCALAPPDATA 'CodexDashboard\settings.json') -Raw | ConvertFrom-Json
$runKey = Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$actual = $runKey.CodexDashboard
$expected = if ($prefs.Startup -eq 'manual') { $null } else { '"' + $app + '"' + $(if ($prefs.Startup -eq 'codex') { if ($prefs.CloseMode -eq 'codex') { ' --launch-with-codex' } else { ' --watch-codex' } } else { '' }) }
$dashboard = @(Get-Process -Name CodexDashboard -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $app })
$desktop = @(Get-Process -Name ChatGPT,Codex -ErrorAction SilentlyContinue | Where-Object { $_.Path -like '*\WindowsApps\OpenAI.Codex_*' -and $_.MainWindowHandle -ne 0 })
$result = [ordered]@{
 CheckedAt = (Get-Date).ToString('o')
 LastWindowsBoot = (Get-CimInstance Win32_OperatingSystem).LastBootUpTime.ToString('o')
 InstalledVersion = (Get-Item $app).VersionInfo.FileVersion
 StartupMode = $prefs.Startup
 StartupCommandMatches = $actual -eq $expected
 ScheduledStartupRequired = $prefs.Startup -eq 'codex' -and $prefs.CloseMode -eq 'codex'
 ScheduledStartupRegistered = [bool](Get-ScheduledTask -TaskName 'CodexDashboard-CodexStartup' -ErrorAction SilentlyContinue)
 DashboardRunning = $dashboard.Count -gt 0
 CodexWindowOpen = $desktop.Count -gt 0
}
$reports = Join-Path $root 'build\reports'
New-Item -ItemType Directory -Force $reports | Out-Null
$result | ConvertTo-Json | Set-Content (Join-Path $reports 'windows-startup-audit.json')
$result | ConvertTo-Json
if (-not $result.StartupCommandMatches) { throw 'Windows startup registration does not match saved settings.' }
if ($result.ScheduledStartupRequired -and -not $result.ScheduledStartupRegistered) { throw 'Scheduled Codex startup is missing.' }
if ($prefs.Startup -ne 'manual' -and (-not $result.ScheduledStartupRequired -or $result.CodexWindowOpen) -and -not $result.DashboardRunning) { throw 'Automatic startup is selected but Dashboard is not running.' }
