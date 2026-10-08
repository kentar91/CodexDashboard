param([switch]$Verify)
$ErrorActionPreference='Stop'
$logPath=Join-Path (Split-Path -Parent $PSScriptRoot) 'work\CodexDashboard-avvio.log'
function Write-MonitorLog([string]$message) {
 try { Add-Content -LiteralPath $logPath -Value ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss')+' '+$message) -Encoding UTF8 } catch {}
}
function Find-CodexApp {
 Get-CimInstance Win32_Process -Filter "name='ChatGPT.exe' OR name='Codex.exe'" | Where-Object {
  $_.ExecutablePath -match '\\OpenAI\.Codex_[^\\]+\\app\\|\\OpenAI\\Codex\\app\\|\\Codex\\Codex\.exe$' -and $_.CommandLine -notmatch '--type='
 } | Select-Object -First 1
}
if($Verify) {
 $app=Find-CodexApp
 if(!$app){throw 'App Codex non rilevata.'}
 Write-Output ('App Codex rilevata: '+$app.ProcessId+'; '+$app.ExecutablePath)
 exit
}
$guard=New-Object Threading.Mutex($false,'Local\CodexDashboardAutoStart')
$owned=$false
try {$owned=$guard.WaitOne(0)} catch [Threading.AbandonedMutexException] {$owned=$true}
if(!$owned){$guard.Dispose();exit}
$stopWidget=New-Object Threading.EventWaitHandle($false,[Threading.EventResetMode]::AutoReset,'Local\CodexDashboardWidgetClose')
$sessionId=$null
$widget=$null
Write-MonitorLog 'Monitor avviato.'
try {
 while($true) {
  try {
   $app=Find-CodexApp
   if($app -and $app.ProcessId -ne $sessionId) {
    [void]$stopWidget.Reset()
    $widgetPath=Join-Path $PSScriptRoot 'CodexDashboard.ps1'
    $widget=Start-Process -FilePath 'powershell.exe' -ArgumentList ('-NoProfile -STA -ExecutionPolicy Bypass -WindowStyle Hidden -File "'+$widgetPath+'"') -WindowStyle Hidden -PassThru
    $sessionId=$app.ProcessId
    Write-MonitorLog ('Codex '+$sessionId+' rilevato; widget '+$widget.Id+' avviato.')
   } elseif(!$app -and $sessionId) {
    [void]$stopWidget.Set()
    $sessionId=$null
    Write-MonitorLog 'Codex chiuso; widget chiuso.'
   }
  } catch {
   Write-MonitorLog ('Errore: '+$_.Exception.Message)
  }
  Start-Sleep -Seconds 3
 }
} finally {
 $stopWidget.Dispose()
 $guard.ReleaseMutex();$guard.Dispose()
}
