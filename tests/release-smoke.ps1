param([string]$Node='C:\Users\Administrator\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$report=Join-Path $root 'build\reports'
New-Item -ItemType Directory -Path $report -Force|Out-Null
$env:CODEXDASHBOARD_ARTIFACT_DIR=$report
$registryPath='HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
$before=(Get-ItemProperty -LiteralPath $registryPath -Name CodexDashboard -ErrorAction SilentlyContinue).CodexDashboard
foreach($flag in '--self-test','--settings-test','--features-test','--placement-test','--lifecycle-test') {
 $process=Start-Process -FilePath (Join-Path $root 'dist\app\CodexDashboard.exe') -ArgumentList $flag -WindowStyle Hidden -PassThru
 if(-not $process.WaitForExit(40000)){throw "Timeout: $flag"}
 if($process.ExitCode -ne 0){throw "Failed: $flag"}
}
$after=(Get-ItemProperty -LiteralPath $registryPath -Name CodexDashboard -ErrorAction SilentlyContinue).CodexDashboard
if($before -ne $after){throw 'Tests changed Windows startup settings.'}
$extract=Join-Path $report ('installer-extract-'+[Guid]::NewGuid().ToString('N'))
$setup=Join-Path $root 'dist\installer\CodexDashboard-Setup-2.0.1.exe'
$process=Start-Process -FilePath $setup -ArgumentList @('--extract-test',('"'+$extract+'"')) -WindowStyle Hidden -Wait -PassThru
if($process.ExitCode -ne 0){throw 'Installer extraction failed.'}
foreach($pair in @(@('CodexDashboard.exe','dist\app\CodexDashboard.exe'),@('CodexDashboard.ico','dist\app\CodexDashboard.ico'),@('com.codexdashboard.monitor.streamDeckPlugin','dist\streamdeck\com.codexdashboard.monitor.streamDeckPlugin'))) {
 if((Get-FileHash -LiteralPath (Join-Path $extract $pair[0])).Hash -ne (Get-FileHash -LiteralPath (Join-Path $root $pair[1])).Hash){throw "Installer payload mismatch: $($pair[0])"}
}
$env:CODEXDASHBOARD_DATA_DIR=Join-Path $report ('plugin-test-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $env:CODEXDASHBOARD_DATA_DIR -Force|Out-Null
'{"Language":"it"}'|Set-Content (Join-Path $env:CODEXDASHBOARD_DATA_DIR 'settings.json') -Encoding UTF8
& $Node (Join-Path $PSScriptRoot 'streamdeck-smoke.cjs')
if($LASTEXITCODE -ne 0){throw 'Stream Deck test failed.'}
'PASS: app/settings/localization/history tests, startup registry unchanged, installer payload hashes match app/plugin/icon, Stream Deck settings and PNG rendering.'|Set-Content (Join-Path $report 'release-smoke.txt')
Write-Output 'PASS: release checks.'
