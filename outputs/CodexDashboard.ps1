param([switch]$Verify)
$ErrorActionPreference = 'Stop'
trap {
 Add-Type -AssemblyName PresentationFramework
 [void][Windows.MessageBox]::Show($_.Exception.Message, 'Codex Dashboard - errore di avvio')
 exit 1
}
$singleton = New-Object Threading.Mutex($false, 'Local\CodexDashboardWidget')
if (!$singleton.WaitOne(0)) { $singleton.Dispose(); exit }
$stopWidget = New-Object Threading.EventWaitHandle($false,[Threading.EventResetMode]::AutoReset,'Local\CodexDashboardWidgetClose')
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
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" Title="Codex Dashboard" Width="180" Height="64" WindowStyle="None" ResizeMode="NoResize" AllowsTransparency="True" Background="Transparent" Topmost="True" ShowInTaskbar="False" Opacity="0.82" FontFamily="Consolas">
 <Window.Resources>
  <Style TargetType="Button">
   <Setter Property="Foreground" Value="#FCEE0A"/><Setter Property="Background" Value="Transparent"/><Setter Property="BorderThickness" Value="0"/><Setter Property="Cursor" Value="Hand"/>
   <Setter Property="Template"><Setter.Value><ControlTemplate TargetType="Button"><Border x:Name="ButtonBg" Background="{TemplateBinding Background}"><ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/></Border><ControlTemplate.Triggers><Trigger Property="IsMouseOver" Value="True"><Setter TargetName="ButtonBg" Property="Background" Value="#30352A"/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter>
  </Style>
 </Window.Resources>
 <Grid>
  <Path Data="M 0,0 L 172,0 180,8 180,64 7,64 0,57 Z" Fill="#0B1017" Stroke="#5B5C2C" StrokeThickness="1"/>
  <Path Data="M 0,0 L 172,0 180,8 180,11 171,3 0,3 Z" Fill="#FCEE0A"/>
  <Path Data="M 0,50 L 2,50 2,56 8,62 18,62 18,64 7,64 0,57 Z" Fill="#00E6F6"/>
  <Grid Margin="10,5,10,5">
   <Grid.RowDefinitions><RowDefinition Height="15"/><RowDefinition Height="*"/></Grid.RowDefinitions>
   <Grid x:Name="DragArea" Cursor="SizeAll" Background="Transparent" ToolTip="Trascina per spostare · dati ogni 60 s">
    <StackPanel Orientation="Horizontal" VerticalAlignment="Center"><TextBlock Text="// CODEX DASH" Foreground="#FCEE0A" FontSize="9" FontWeight="Bold"/><TextBlock x:Name="Status" Text="●" Foreground="#00E6F6" FontSize="8" Margin="8,0,0,0"/></StackPanel>
    <StackPanel x:Name="Controls" Orientation="Horizontal" HorizontalAlignment="Right" Opacity="0"><Button x:Name="Refresh" Content="↻" FontSize="12" Width="17" ToolTip="Aggiorna ora"/><Button x:Name="Close" Content="×" FontSize="14" Width="17" ToolTip="Chiudi"/></StackPanel>
   </Grid>
   <Grid Grid.Row="1">
    <Grid.ColumnDefinitions><ColumnDefinition/><ColumnDefinition Width="12"/><ColumnDefinition/></Grid.ColumnDefinitions>
    <StackPanel x:Name="PrimaryCard">
     <TextBlock x:Name="PrimaryLabel" Text="5 ORE" Foreground="#96A3AD" FontSize="7"/>
     <TextBlock x:Name="PrimaryValue" Text="—" Foreground="#FCEE0A" FontSize="19" FontWeight="Bold" LineHeight="21" LineStackingStrategy="BlockLineHeight"/>
     <ProgressBar x:Name="PrimaryBar" Height="2" Maximum="100" Background="#26313C" Foreground="#FCEE0A" BorderThickness="0"/>
    </StackPanel>
    <Border Grid.Column="1" Width="1" Background="#26313C" Margin="0,2,0,0"/>
    <StackPanel Grid.Column="2" x:Name="SecondaryCard">
     <TextBlock x:Name="SecondaryLabel" Text="SETTIMANA" Foreground="#96A3AD" FontSize="7"/>
     <TextBlock x:Name="SecondaryValue" Text="—" Foreground="#00E6F6" FontSize="19" FontWeight="Bold" LineHeight="21" LineStackingStrategy="BlockLineHeight"/>
     <ProgressBar x:Name="SecondaryBar" Height="2" Maximum="100" Background="#26313C" Foreground="#00E6F6" BorderThickness="0"/>
    </StackPanel>
   </Grid>
   <TextBlock x:Name="PrimaryReset" Visibility="Collapsed"/><TextBlock x:Name="SecondaryReset" Visibility="Collapsed"/><TextBlock x:Name="Plan" Visibility="Collapsed"/>
  </Grid>
 </Grid>
</Window>

'@
$window = [Windows.Markup.XamlReader]::Load((New-Object System.Xml.XmlNodeReader $xaml))
$ui = @{}
foreach ($name in 'Close','DragArea','Controls','Refresh','Plan','PrimaryCard','SecondaryCard','PrimaryLabel','PrimaryValue','PrimaryBar','PrimaryReset','SecondaryLabel','SecondaryValue','SecondaryBar','SecondaryReset','Status') { $ui[$name]=$window.FindName($name) }
$window.Add_MouseEnter({ $window.Opacity=1; $ui.Controls.Opacity=1 })
$window.Add_MouseLeave({ $window.Opacity=0.82; $ui.Controls.Opacity=0 })
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
function Set-State([string]$detail,[bool]$online=$false) {
 $ui.Status.ToolTip=$detail
 if($online) {
  $ui.Status.Text='●'
  $ui.Status.Foreground=[Windows.Media.BrushConverter]::new().ConvertFromString('#00E6F6')
 } else {
  $ui.Status.Text='!'
  $ui.Status.Foreground=[Windows.Media.Brushes]::Coral
 }
}
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
  $ui[$prefix+'Value'].Text='—'; $ui[$prefix+'Bar'].Value=0; $ui[$prefix+'Reset'].Text='Dato non disponibile'; $ui[$prefix+'Card'].ToolTip='Dato non disponibile'; return
 }
 $remaining=[Math]::Max(0,[Math]::Min(100,100-[double]$quota.usedPercent))
 $ui[$prefix+'Value'].Text=('{0:0.#}%' -f $remaining)
 $ui[$prefix+'Bar'].Value=$remaining
 if ($remaining -le 15) { $ui[$prefix+'Bar'].Foreground=[Windows.Media.Brushes]::Coral }
 else { $ui[$prefix+'Bar'].Foreground=[Windows.Media.BrushConverter]::new().ConvertFromString($(if($prefix -eq 'Primary'){'#FCEE0A'}else{'#00E6F6'})) }
 $mins=$quota.windowDurationMins
 if ($mins -eq 10080) { $label='SETTIMANA' } elseif ($mins -ge 60) { $label=($mins/60).ToString()+' ORE' } else { $label=$mins.ToString()+' MIN' }
 $ui[$prefix+'Label'].Text=$label
 if ($null -ne $quota.resetsAt) {
  $zone=[TimeZoneInfo]::FindSystemTimeZoneById('W. Europe Standard Time')
  $reset=[TimeZoneInfo]::ConvertTime([DateTimeOffset]::FromUnixTimeSeconds([long]$quota.resetsAt),$zone)
  $ui[$prefix+'Reset'].Text='Si rinnova '+$reset.ToString('dd/MM · HH:mm')
 } else { $ui[$prefix+'Reset'].Text='Rinnovo non disponibile' }
 $ui[$prefix+'Card'].ToolTip=('{0:0.#}% disponibile · ' -f $remaining)+$ui[$prefix+'Reset'].Text
}
$ui.Refresh.Add_Click({ Request-Limits })
$timer=New-Object Windows.Threading.DispatcherTimer
$timer.Interval=[TimeSpan]::FromMilliseconds(250)
$timer.Add_Tick({
 if ($stopWidget.WaitOne(0)) { $window.Close(); return }
 try {
  if (!$script:meter -or $script:meter.Child.HasExited) {
   Set-State 'Offline · riconnessione a Codex'
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
    Set-State ('Aggiornato alle '+$script:lastSuccess.ToString('HH:mm:ss')+' · '+$ui.Plan.Text) $true
   }
  }
  if (((Get-Date)-$script:lastRequest).TotalSeconds -ge 60) { Request-Limits }
  if ((!$script:ready -or $script:pending) -and ((Get-Date)-$script:lastRequest).TotalSeconds -gt 25) {
   $script:meter.Dispose()
   Set-State 'Offline · nuovo tentativo tra 60 s'
  }
 } catch {
  Set-State $_.Exception.Message
  if ($script:lastSuccess) { Set-State ('Dati precedenti · '+$script:lastSuccess.ToString('HH:mm:ss')+' · riprovo') }
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
 $bitmap=New-Object Windows.Media.Imaging.RenderTargetBitmap 180,64,96,96,([Windows.Media.PixelFormats]::Pbgra32)
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
$stopWidget.Dispose()
