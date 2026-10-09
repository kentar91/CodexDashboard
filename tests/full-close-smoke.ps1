$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$testDir = Join-Path $root ('build\reports\full-close-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $testDir | Out-Null
$oldData = $env:CODEXDASHBOARD_DATA_DIR
$oldReports = $env:CODEXDASHBOARD_ARTIFACT_DIR
try {
 $env:CODEXDASHBOARD_DATA_DIR = $testDir
 $env:CODEXDASHBOARD_ARTIFACT_DIR = $testDir
 $state = Join-Path $testDir 'codex-open.txt'
 foreach ($attempt in 1,2) {
  [IO.File]::WriteAllText($state,'open')
  $probe = Start-Process (Join-Path $root 'dist\app\CodexDashboard.exe') -ArgumentList '--shutdown-probe-test' -WindowStyle Hidden -PassThru
  try {
   Start-Sleep -Seconds 4
   if ($probe.HasExited) { throw 'App exited while Codex was open.' }
   [IO.File]::WriteAllText($state,'closed')
   if (-not $probe.WaitForExit(10000)) { throw 'App process did not exit after Codex closed.' }
   if ($probe.ExitCode -ne 0) { throw "App exit failed: $($probe.ExitCode)" }
  } finally { if (-not $probe.HasExited) { $probe.Kill(); $probe.WaitForExit() } }
 }
 'PASS: real app processes stay alive while Codex is simulated open, fully exit after closing, and relaunch as a new process.' | Set-Content (Join-Path $testDir 'result.txt')
 Write-Output 'PASS: full process shutdown and new process relaunch.'
} finally {
 $env:CODEXDASHBOARD_DATA_DIR = $oldData
 $env:CODEXDASHBOARD_ARTIFACT_DIR = $oldReports
}
