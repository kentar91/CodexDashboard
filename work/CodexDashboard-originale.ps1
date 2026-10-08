param([switch]$Verify)
$ErrorActionPreference = 'Stop'
trap {
 Add-Type -AssemblyName PresentationFramework
 [void][Windows.MessageBox]::Show($_.Exception.Message, 'Codex Dashboard - errore di avvio')
 exit 1
}
$singleton = New-Object Threading.Mutex($false, 'Local\CodexDashboardWidget')
if (!$singleton.WaitOne(0)) { $singleton.Dispose(); exit }
Add-Type -AssemblyName PresentationFramework, PresentationCore, WindowsBase
Add-Type -TypeDefinition @'
using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
public class CodexDashboardMeter : IDisposable {
 public ConcurrentQueue<string> Lines = new ConcurrentQueue<string>();
 public Process Child;
 public DateTime Started = DateTime.UtcNow;
 public void Start(string exe) {
  Child = new Process();
  Child.StartInfo = new ProcessStartInfo(exe, "app-server --listen stdio://") {
   UseShellExecute=false, CreateNoWindow=true, RedirectStandardInput=true,
   RedirectStandardOutput=true, RedirectStandardError=true
  };
  Child.Start();
  Task.Run(async () => { string line; while ((line=await Child.StandardOutput.ReadLineAsync())!=null) Lines.Enqueue(line); });
  Task.Run(async () => { while (await Child.StandardError.ReadLineAsync()!=null) {} });
  Send("{\"id\":1,\"method\":\"initialize\",\"params\":{\"clientInfo\":{\"name\":\"codex_dashboard\",\"title\":\"Codex Dashboard\",\"version\":\"1.0.0\"}}}");
 }
 public void Send(string line) { Child.StandardInput.WriteLine(line); Child.StandardInput.Flush(); }
 public void Dispose() { try { if(Child!=null && !Child.HasExited) Child.Kill(); } catch {} if(Child!=null) Child.Dispose(); }
}
'@
[xml]$xaml = @'
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" Title="Codex Dashboard" Width="328" Height="370" WindowStyle="None" ResizeMode="NoResize" AllowsTransparency="True" Background="Transparent" Topmost="True" ShowInTaskbar="True">
 <Border x:Name="Shell" CornerRadius="24" BorderBrush="#B497E9" BorderThickness="1" Padding="22">
  <Border.Background><LinearGradientBrush StartPoint="0,0" EndPoint="1,1"><GradientStop Color="#102B63"/><GradientStop Color="#31245C" Offset="0.65"/><GradientStop Color="#432858" Offset="1"/></LinearGradientBrush></Border.Background>
  <Grid>
   <Grid.RowDefinitions><RowDefinition Height="45"/><RowDefinition Height="55"/><RowDefinition Height="83"/><RowDefinition Height="83"/><RowDefinition Height="*"/></Grid.RowDefinitions>
   <Grid x:Name="DragArea" Cursor="SizeAll">
    <StackPanel><TextBlock Text="✦  CODEX DASHBOARD" Foreground="#FFE0A0" FontSize="17" FontWeight="Bold" FontFamily="Georgia"/><TextBlock Text="Il tuo utilizzo di Codex" Foreground="#B8C7EF" FontSize="11" Margin="0,6,0,0"/></StackPanel>
    <Button x:Name="Close" Content="×" HorizontalAlignment="Right" VerticalAlignment="Top" Background="Transparent" BorderThickness="0" Foreground="#CCCAEB" FontSize="22" Width="25" ToolTip="Chiudi il widget"/>
   </Grid>
   <Grid Grid.Row="1" Margin="0,4,0,0">
    <Canvas Width="90" Height="44" HorizontalAlignment="Left">
     <Path Fill="#91B9F4" Data="M 5,40 L 5,22 12,22 12,14 18,6 24,14 24,22 33,22 33,17 40,17 40,9 46,1 52,9 52,17 59,17 59,22 68,22 68,14 74,6 80,14 80,22 87,22 87,40 Z"/>
     <Path Stroke="#FFE0A0" StrokeThickness="1.5" Data="M 1,19 C 20,-8 66,-8 89,19"/>
     <TextBlock Text="✧" Foreground="#FFE0A0" FontSize="17" Canvas.Left="83" Canvas.Top="-7"/>
    </Canvas>
    <StackPanel HorizontalAlignment="Right" VerticalAlignment="Center"><TextBlock x:Name="Plan" Text="COLLEGAMENTO…" Foreground="#DFD5F6" FontSize="10" HorizontalAlignment="Right"/><TextBlock Text="Quota disponibile" Foreground="White" FontSize="14" FontWeight="SemiBold" Margin="0,5,0,0"/></StackPanel>
   </Grid>
   <StackPanel Grid.Row="2" Margin="0,8,0,0">
    <DockPanel><TextBlock x:Name="PrimaryLabel" Text="Finestra di 5 ore" Foreground="#CFD9F3" FontSize="12"/><TextBlock x:Name="PrimaryValue" Text="—" Foreground="#FFE0A0" FontSize="24" FontWeight="Bold" HorizontalAlignment="Right" TextAlignment="Right"/></DockPanel>
    <ProgressBar x:Name="PrimaryBar" Height="7" Maximum="100" Background="#354673" Foreground="#FFD581" BorderThickness="0" Margin="0,5,0,7"/>
    <TextBlock x:Name="PrimaryReset" Text="In attesa dei dati reali" Foreground="#AEBFE4" FontSize="11"/>
   </StackPanel>
   <StackPanel Grid.Row="3" Margin="0,8,0,0">
    <DockPanel><TextBlock x:Name="SecondaryLabel" Text="Settimana" Foreground="#CFD9F3" FontSize="12"/><TextBlock x:Name="SecondaryValue" Text="—" Foreground="#B8C4FF" FontSize="24" FontWeight="Bold" HorizontalAlignment="Right" TextAlignment="Right"/></DockPanel>
    <ProgressBar x:Name="SecondaryBar" Height="7" Maximum="100" Background="#354673" Foreground="#ACAAFF" BorderThickness="0" Margin="0,5,0,7"/>
    <TextBlock x:Name="SecondaryReset" Text="In attesa dei dati reali" Foreground="#AEBFE4" FontSize="11"/>
   </StackPanel>
   <StackPanel Grid.Row="4" Margin="0,11,0,0">
    <DockPanel><Button x:Name="Refresh" Content="↻" FontSize="18" Width="26" Background="Transparent" Foreground="#FFE0A0" BorderThickness="0" DockPanel.Dock="Right" ToolTip="Aggiorna ora"/><TextBlock x:Name="Status" Text="✦ Connessione a Codex…" Foreground="#C3D7E9" FontSize="11" VerticalAlignment="Center"/></DockPanel>
    <TextBlock Text="Limiti del piano · aggiornamento ogni 60 s" Foreground="#98ADD1" FontSize="10" Margin="0,3,0,0"/>
   </StackPanel>
  </Grid>
 </Border>
</Window>
'@
$window = [Windows.Markup.XamlReader]::Load((New-Object System.Xml.XmlNodeReader $xaml))
$ui = @{}
foreach ($name in 'Close','DragArea','Refresh','Plan','PrimaryLabel','PrimaryValue','PrimaryBar','PrimaryReset','SecondaryLabel','SecondaryValue','SecondaryBar','SecondaryReset','Status') { $ui[$name]=$window.FindName($name) }
$window.Left = [System.Windows.SystemParameters]::WorkArea.Right - $window.Width - 24
$window.Top = [System.Windows.SystemParameters]::WorkArea.Bottom - $window.Height - 24
$ui.Close.Add_Click({ $window.Close() })
$ui.DragArea.Add_MouseLeftButtonDown({ if ($_.OriginalSource -isnot [System.Windows.Controls.Button]) { $window.DragMove() } })
$script:meter = $null
$script:ready = $false
$script:lastRequest = [datetime]::MinValue
$script:lastSuccess = $null
$script:pending = $false
$script:requestId = 10
function Connect-Meter {
 if ($script:meter) { $script:meter.Dispose() }
 $script:ready=$false; $script:pending=$false
 $cmd = Get-Command codex.exe -ErrorAction SilentlyContinue
 if (!$cmd) {
  $cmd = Get-ChildItem -LiteralPath "$env:LOCALAPPDATA\OpenAI\Codex\bin" -Filter codex.exe -Recurse -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  if (!$cmd) { throw 'Codex non trovato: apri o installa Codex.' }
  $exe=$cmd.FullName
 } else { $exe=$cmd.Source }
 $script:meter = New-Object CodexDashboardMeter
 $script:meter.Start($exe)
 $script:lastRequest=Get-Date
}
function Request-Limits {
 if ($script:ready -and !$script:pending) {
  $script:requestId++
  $script:meter.Send(('{"id":'+$script:requestId+',"method":"account/rateLimits/read"}'))
  $script:pending=$true; $script:lastRequest=Get-Date
 }
}
function Set-Quota($prefix,$quota) {
 if ($null -eq $quota -or $null -eq $quota.usedPercent) {
  $ui[$prefix+'Value'].Text='—'; $ui[$prefix+'Bar'].Value=0; $ui[$prefix+'Reset'].Text='Dato non disponibile'; return
 }
 $remaining=[Math]::Max(0,[Math]::Min(100,100-[double]$quota.usedPercent))
 $ui[$prefix+'Value'].Text=('{0:0.#}%' -f $remaining)
 $ui[$prefix+'Bar'].Value=$remaining
 if ($remaining -le 15) { $ui[$prefix+'Bar'].Foreground=[Windows.Media.Brushes]::Coral }
 else { $ui[$prefix+'Bar'].Foreground=[Windows.Media.BrushConverter]::new().ConvertFromString($(if($prefix -eq 'Primary'){'#FFD581'}else{'#ACAAFF'})) }
 $mins=$quota.windowDurationMins
 if ($mins -eq 10080) { $label='Settimana' } elseif ($mins -ge 60) { $label='Finestra di '+($mins/60)+' ore' } else { $label='Finestra di '+$mins+' minuti' }
 $ui[$prefix+'Label'].Text=$label
 if ($null -ne $quota.resetsAt) {
  $zone=[TimeZoneInfo]::FindSystemTimeZoneById('W. Europe Standard Time')
  $reset=[TimeZoneInfo]::ConvertTime([DateTimeOffset]::FromUnixTimeSeconds([long]$quota.resetsAt),$zone)
  $ui[$prefix+'Reset'].Text='Si rinnova '+$reset.ToString('dd/MM · HH:mm')
 } else { $ui[$prefix+'Reset'].Text='Rinnovo non disponibile' }
}
$ui.Refresh.Add_Click({ Request-Limits })
$timer=New-Object Windows.Threading.DispatcherTimer
$timer.Interval=[TimeSpan]::FromMilliseconds(250)
$timer.Add_Tick({
 try {
  if (!$script:meter -or $script:meter.Child.HasExited) {
   $ui.Status.Text='Offline · riconnessione a Codex'
   if (((Get-Date)-$script:lastRequest).TotalSeconds -ge 60 -or !$script:meter) { Connect-Meter }
   return
  }
  $line=$null
  while ($script:meter.Lines.TryDequeue([ref]$line)) {
   $msg=$line | ConvertFrom-Json
   if ($msg.id -eq 1) {
    if ($msg.error) { throw 'Connessione rifiutata da Codex.' }
    $script:meter.Send('{"method":"initialized","params":{}}')
    $script:ready=$true; Request-Limits
   } elseif (($msg.id -eq $script:requestId) -or ($msg.method -eq 'account/rateLimits/updated')) {
    if ($msg.id) { $script:pending=$false }
    if ($msg.error) { throw 'Dati non disponibili. Verifica l''accesso a Codex.' }
    $data=$msg.result; if ($msg.method) { $data=$msg.params }
    $limits=$data.rateLimits
    if ($data.rateLimitsByLimitId.codex) { $limits=$data.rateLimitsByLimitId.codex }
    if (!$limits) { throw 'Limiti non disponibili per questo account.' }
    Set-Quota 'Primary' $limits.primary
    Set-Quota 'Secondary' $limits.secondary
    $ui.Plan.Text='CODEX · '+([string]$limits.planType).ToUpper()
    $script:lastSuccess=Get-Date
    $ui.Status.Text='✦ Aggiornato alle '+$script:lastSuccess.ToString('HH:mm:ss')
   }
  }
  if (((Get-Date)-$script:lastRequest).TotalSeconds -ge 60) { Request-Limits }
  if ((!$script:ready -or $script:pending) -and ((Get-Date)-$script:lastRequest).TotalSeconds -gt 25) {
   $script:meter.Dispose()
   $ui.Status.Text='Offline · nuovo tentativo tra 60 s'
  }
 } catch {
  $ui.Status.Text=$_.Exception.Message
  if ($script:lastSuccess) { $ui.Status.Text='Dati precedenti · '+$script:lastSuccess.ToString('HH:mm:ss')+' · riprovo' }
  $script:pending=$false
  $script:lastRequest=Get-Date
 }
})
$window.Add_Closed({ $timer.Stop(); if($script:meter){$script:meter.Dispose()} })
Connect-Meter
$timer.Start()
if ($Verify) {
 $window.Show()
 $until=(Get-Date).AddSeconds(30)
 while (!$script:lastSuccess -and (Get-Date) -lt $until) {
  $frame=New-Object Windows.Threading.DispatcherFrame
  $tick=New-Object Windows.Threading.DispatcherTimer
  $tick.Interval=[TimeSpan]::FromMilliseconds(300)
  $tick.Add_Tick({$frame.Continue=$false;$tick.Stop()})
  $tick.Start(); [Windows.Threading.Dispatcher]::PushFrame($frame)
 }
 $window.UpdateLayout()
 $bitmap=New-Object Windows.Media.Imaging.RenderTargetBitmap 328,370,96,96,([Windows.Media.PixelFormats]::Pbgra32)
 $bitmap.Render($window)
 $encoder=New-Object Windows.Media.Imaging.PngBitmapEncoder
 $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
 $stream=[IO.File]::Create((Join-Path $PSScriptRoot 'CodexDashboard-anteprima.png'))
 $encoder.Save($stream); $stream.Dispose()
 $verified=$null -ne $script:lastSuccess
 Write-Output ('Live data verified: '+$verified+'; '+$ui.PrimaryValue.Text+' / '+$ui.SecondaryValue.Text+'; '+$ui.Status.Text)
 $window.Close()
 if (!$verified) { exit 1 }
} else { [void]$window.ShowDialog() }
$singleton.ReleaseMutex()
$singleton.Dispose()
