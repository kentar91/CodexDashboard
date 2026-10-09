$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$report = Join-Path $root ('build\reports\installer-components-' + [Guid]::NewGuid().ToString('N'))
$setup = Join-Path $root 'dist\installer\CodexDashboard-Setup-2.0.1.exe'
$sources = @{
 'CodexDashboard.exe' = 'dist\app\CodexDashboard.exe'
 'CodexDashboard.ico' = 'dist\app\CodexDashboard.ico'
 'com.codexdashboard.monitor.streamDeckPlugin' = 'dist\streamdeck\com.codexdashboard.monitor.streamDeckPlugin'
}
foreach ($mode in 'app','plugin','both') {
 $target = Join-Path $report $mode
 $process = Start-Process $setup -ArgumentList @('--extract-test',('"'+$target+'"'),$mode) -WindowStyle Hidden -Wait -PassThru
 if ($process.ExitCode -ne 0) { throw "Extraction failed: $mode" }
 $expected = if ($mode -eq 'app') { @('CodexDashboard.exe','CodexDashboard.ico') } elseif ($mode -eq 'plugin') { @('com.codexdashboard.monitor.streamDeckPlugin') } else { @($sources.Keys) }
 if (@(Get-ChildItem $target -File).Count -ne $expected.Count) { throw "Unexpected files: $mode" }
 foreach ($name in $expected) {
  if ((Get-FileHash (Join-Path $target $name)).Hash -ne (Get-FileHash (Join-Path $root $sources[$name])).Hash) { throw "Payload mismatch: $mode / $name" }
 }
 # Selecting another component must leave the existing component intact.
 $otherMode = if ($mode -eq 'app') { 'plugin' } else { 'app' }
 $process = Start-Process $setup -ArgumentList @('--extract-test',('"'+$target+'"'),$otherMode) -WindowStyle Hidden -Wait -PassThru
 if ($process.ExitCode -ne 0) { throw "Update failed: $mode" }
 foreach ($name in $sources.Keys) {
  if ((Get-FileHash (Join-Path $target $name)).Hash -ne (Get-FileHash (Join-Path $root $sources[$name])).Hash) { throw "Existing component changed: $mode / $name" }
 }
}
'PASS: app only, plugin only, both; embedded payload hashes and preservation of existing components.' | Set-Content (Join-Path $report 'result.txt')
Write-Output 'PASS: installer component selection.'
