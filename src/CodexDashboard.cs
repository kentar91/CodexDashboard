using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CodexDashboard {
static class Json {
 public static Dictionary<string,object> Parse(string s) { return new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(s); }
 public static string Stringify(object o) { return new JavaScriptSerializer().Serialize(o); }
 public static object Get(object o,string key) { var d=o as Dictionary<string,object>; return d!=null && d.ContainsKey(key)?d[key]:null; }
 public static string Str(object o,string key) { return Convert.ToString(Get(o,key)); }
}
public class Quota {
 public double Remaining; public int Minutes; public long? Reset;
 public string Label { get { return Minutes==10080?"SETTIMANA":Minutes>=60?(Minutes/60.0).ToString("0.#")+" ORE":Minutes+" MIN"; } }
 public static Quota Read(object o) {
  if(Json.Get(o,"usedPercent")==null || Json.Get(o,"windowDurationMins")==null) return null;
  double used=Convert.ToDouble(Json.Get(o,"usedPercent"));
  if(double.IsNaN(used)||double.IsInfinity(used)) return null;
  return new Quota {Remaining=Math.Max(0,Math.Min(100,100-used)),Minutes=Convert.ToInt32(Json.Get(o,"windowDurationMins")),Reset=Json.Get(o,"resetsAt")==null?(long?)null:Convert.ToInt64(Json.Get(o,"resetsAt"))};
 }
 public string ResetText {get { if(!Reset.HasValue) return L.T("Rinnovo non disponibile"); try { var zone=TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); return L.T("Si rinnova ")+TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(Reset.Value),zone).ToString("dd/MM · HH:mm"); } catch { return L.T("Rinnovo non disponibile"); } }}
}
public class Snapshot {
 public Quota Primary,Secondary; public int? AvailableResets; public long? NextResetExpiry; public DateTime? Updated; public string Plan=""; public string Error="Connessione a Codex…";
 public bool Online {get {return Updated.HasValue && Error==null && (DateTime.UtcNow-Updated.Value).TotalSeconds<Math.Max(90,SettingsStore.Current.PollSeconds+30);}}
 public string ResetCreditsText {get{return L.T("Reset disponibili: ")+(AvailableResets.HasValue?AvailableResets.Value.ToString():L.T("non disponibili"))+(NextResetExpiry.HasValue?"\n"+L.T("Prossimo reset in scadenza: ")+TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(NextResetExpiry.Value),TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).ToString("dd/MM · HH:mm"):"")+(AvailableResets.HasValue&&!Online?L.T(" (ultimo dato ricevuto)"):"");}}
 public static Snapshot Read(object data) {
  var buckets=Json.Get(data,"rateLimitsByLimitId");
  var limits=Json.Get(buckets,"codex")??Json.Get(data,"rateLimits");
  if(limits==null) throw new Exception("Limiti non disponibili per questo account.");
  var resetCount=Json.Get(Json.Get(data,"rateLimitResetCredits"),"availableCount");
  long? nextExpiry=null;var resetEntries=Json.Get(Json.Get(data,"rateLimitResetCredits"),"credits") as System.Collections.IEnumerable;if(resetEntries!=null)foreach(var entry in resetEntries){long expiry;if(Json.Str(entry,"status")=="available"&&long.TryParse(Json.Str(entry,"expiresAt"),out expiry)&&expiry>DateTimeOffset.UtcNow.ToUnixTimeSeconds()&&(!nextExpiry.HasValue||expiry<nextExpiry.Value))nextExpiry=expiry;}
  return new Snapshot {NextResetExpiry=nextExpiry,AvailableResets=resetCount==null?(int?)null:Math.Max(0,Convert.ToInt32(resetCount)),Primary=Quota.Read(Json.Get(limits,"primary")),Secondary=Quota.Read(Json.Get(limits,"secondary")),Plan=Json.Str(limits,"planType"),Updated=DateTime.UtcNow,Error=null};
 }
}
sealed class Meter : IDisposable {
 readonly object gate=new object(); Process child; System.Threading.Timer timer; bool ready,pending,disposed; int requestId=10; DateTime last=DateTime.MinValue; Snapshot state=new Snapshot();
 public Snapshot State {get {lock(gate) return state;}}
 public string Diagnostic {get {lock(gate)return "child="+(child==null?"none":child.Id.ToString())+" ready="+ready+" pending="+pending+" request="+requestId;}}
 public Meter(bool start=true) { timer=new System.Threading.Timer(delegate {Tick();},null,start?0:Timeout.Infinite,1000); }
 static string FindCodex() {
  var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"OpenAI","Codex","bin");
  if(Directory.Exists(root)) {var f=Directory.GetFiles(root,"codex.exe",SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();if(f!=null)return f;}
  foreach(var dir in (Environment.GetEnvironmentVariable("PATH")??"").Split(';')) { try {var f=Path.Combine(dir.Trim('"'),"codex.exe");if(File.Exists(f))return f;}catch{} }
  throw new Exception("Codex non trovato. Installa Codex e accedi al tuo account.");
 }
 void Fail(string message) {state=new Snapshot {Primary=state.Primary,Secondary=state.Secondary,Updated=state.Updated,Plan=state.Plan,AvailableResets=state.AvailableResets,NextResetExpiry=state.NextResetExpiry,Error=message};}
 void StopChild() {if(child!=null){try{if(!child.HasExited)child.Kill();}catch{}child.Dispose();child=null;}ready=false;pending=false;}
 void Tick() {lock(gate) {if(disposed)return;try {
  if(child==null||child.HasExited) {if((DateTime.UtcNow-last).TotalSeconds<15)return;StopChild();last=DateTime.UtcNow;
   var p=new Process {StartInfo=new ProcessStartInfo(FindCodex(),"app-server --listen stdio://") {UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8}};
   child=p;p.OutputDataReceived+=(s,e)=>{if(e.Data!=null)Handle(p,e.Data);};p.ErrorDataReceived+=(s,e)=>{};
   p.Start();p.BeginOutputReadLine();p.BeginErrorReadLine();
   Send(new {id=1,method="initialize",@params=new {clientInfo=new {name="codex_dashboard",title="Codex Dashboard",version=AppVersion.Value}}});
  } else if((!ready||pending)&&(DateTime.UtcNow-last).TotalSeconds>25) {StopChild();last=DateTime.UtcNow;Fail("Codex non risponde · riconnessione automatica");}
  else if(ready&&!pending&&(DateTime.UtcNow-last).TotalSeconds>=SettingsStore.Current.PollSeconds)Request();
 }catch(Exception ex){StopChild();last=DateTime.UtcNow;Fail(ex.Message);}}}
 void Send(object o) {child.StandardInput.WriteLine(Json.Stringify(o));child.StandardInput.Flush();}
 public void Refresh() {lock(gate){if(disposed)return;if(ready&&!pending)Request();else if(child==null)last=DateTime.MinValue;}}
 void Request() {requestId++;Send(new {id=requestId,method="account/rateLimits/read"});pending=true;last=DateTime.UtcNow;}
 void Handle(Process source,string line) {lock(gate) {if(disposed||source!=child)return;try {
  var m=Json.Parse(line);var id=Json.Get(m,"id");var method=Json.Str(m,"method");
  if(id!=null&&Convert.ToInt32(id)==1) {if(Json.Get(m,"error")!=null)throw new Exception("Connessione rifiutata da Codex.");Send(new {method="initialized",@params=new {}});ready=true;Request();}
  else if((id!=null&&Convert.ToInt32(id)==requestId)||method=="account/rateLimits/updated") {
   if(id!=null)pending=false;
   if(Json.Get(m,"error")!=null)throw new Exception("Limiti non disponibili. Verifica l'accesso a Codex.");
   state=Snapshot.Read(Json.Get(m,method=="account/rateLimits/updated"?"params":"result"));QuotaHistory.Record(state);
  }
 }catch(Exception ex){pending=false;Fail(ex.Message);}}}
 public void Dispose() {lock(gate){disposed=true;timer.Dispose();StopChild();}}
}
static class Program {
 [STAThread] static int Main(string[] args) {
  try {
   if(args.Contains("--shutdown-probe-test")){if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR")))throw new Exception("Separate test directory required");SettingsStore.Save(new UserSettings{Startup="codex",CloseMode="codex"},false);using(var probeMeter=new Meter(false))using(var probeShow=new EventWaitHandle(false,EventResetMode.AutoReset))using(var probeClose=new EventWaitHandle(false,EventResetMode.AutoReset)){var probeApp=new Application();var probeWindow=Widget.Create(probeMeter,probeShow,probeClose,false,true,null,()=>File.ReadAllText(Path.Combine(SettingsStore.DirectoryPath,"codex-open.txt"))=="open");probeApp.Run(probeWindow);}return 0;}
   if(args.Contains("--repair-startup")){SettingsStore.Save(SettingsStore.Current);return 0;}
   bool scheduledLaunch=args.Contains("--launch-with-codex");if(scheduledLaunch&&(SettingsStore.Current.Startup!="codex"||!SettingsStore.CodexIsOpen()))return 0;
   if(args.Contains("--self-test")) {SelfTest();return 0;}
   if(args.Contains("--settings-test")) {SettingsTest();return 0;}
   if(args.Contains("--history-filter-test")){HistoryFilterTest();return 0;}
   if(args.Contains("--lifecycle-test")){LifecycleTest();return 0;}
   if(args.Contains("--features-test")) {FeaturesTest();return 0;}
   if(args.Contains("--placement-test")) {PlacementTest();return 0;}
   if(args.Contains("--history-preview")||args.Contains("--history-current-preview")){if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR")))throw new Exception("Set a separate CODEXDASHBOARD_DATA_DIR for history previews.");Directory.CreateDirectory(SettingsStore.DirectoryPath);if(!args.Contains("--history-current-preview"))File.WriteAllLines(QuotaHistory.FilePath,Enumerable.Range(0,24).Select(i=>Json.Stringify(new HistorySample{At=DateTime.UtcNow.AddHours(i-23).ToString("o"),Short=100-(i%6)*12,Weekly=100-i*2})));var window=QuotaHistory.Show(null);window.UpdateLayout();var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth),(int)Math.Ceiling(window.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(window);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using(var file=File.Create(Artifacts.Path("history-preview.png")))encoder.Save(file);window.Close();return 0;}
   if(args.Contains("--config-preview")) {var config=Configurator.Create(null,null);config.Show();var previewFrame=new DispatcherFrame();var previewTick=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(200)};previewTick.Tick+=(s,e)=>{previewTick.Stop();previewFrame.Continue=false;};previewTick.Start();Dispatcher.PushFrame(previewFrame);config.UpdateLayout();var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(config.ActualWidth),(int)Math.Ceiling(config.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(config);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using(var file=File.Create(Artifacts.Path("CodexDashboard-config-preview.png")))encoder.Save(file);config.Close();return 0;}
   if(args.Contains("--deck-preview")) {Plugin.WritePreview();return 0;}
   if(args.Contains("-port")) {Plugin.Run(args).GetAwaiter().GetResult();return 0;}
   if(args.Contains("--preview")) {
    using(var meter=new Meter())using(var show=new EventWaitHandle(false,EventResetMode.AutoReset))using(var close=new EventWaitHandle(false,EventResetMode.AutoReset)) {
     var w=Widget.Create(meter,show,close,false);w.Show();var until=DateTime.UtcNow.AddSeconds(35);
     while(!meter.State.Online&&DateTime.UtcNow<until){var frame=new DispatcherFrame();var tick=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(300)};tick.Tick+=(s,e)=>{tick.Stop();frame.Continue=false;};tick.Start();Dispatcher.PushFrame(frame);}
     var finalFrame=new DispatcherFrame();var finalTick=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(500)};finalTick.Tick+=(s,e)=>{finalTick.Stop();finalFrame.Continue=false;};finalTick.Start();Dispatcher.PushFrame(finalFrame);w.UpdateLayout();
     ((StackPanel)w.FindName("Controls")).Opacity=1;
     var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(w.ActualWidth),(int)Math.Ceiling(w.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(w);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
     using(var stream=File.Create(Artifacts.Path("CodexDashboard-native-preview.png")))encoder.Save(stream);w.Close();return meter.State.Online?0:1;
    }
   }
   if(args.Contains("--verify")) {using(var meter=new Meter()){var until=DateTime.UtcNow.AddSeconds(35);while(!meter.State.Online&&DateTime.UtcNow<until){Thread.Sleep(200);File.WriteAllText(Artifacts.Path("verify-progress.txt"),meter.Diagnostic);}File.WriteAllText(Artifacts.Path("verification.json"),Json.Stringify(meter.State),Encoding.UTF8);return meter.State.Online?0:1;}}
   using(var show=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\CodexDashboardNativeShow"))using(var configure=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\CodexDashboardSettingsShow"))using(var mutex=new Mutex(false,"Local\\CodexDashboardNative")) {
    bool owned;try{owned=mutex.WaitOne(0);}catch(AbandonedMutexException){owned=true;}
    if(!owned){if(scheduledLaunch)return 0;if(args.Contains("--settings"))configure.Set();else show.Set();return 0;}
    try{using(var meter=new Meter())using(var close=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\CodexDashboardWidgetClose")) {
     var app=new Application();var window=Widget.Create(meter,show,close,true,args.Contains("--watch-codex"),configure);if(args.Contains("--settings"))configure.Set();app.Run(window);
    }}finally{mutex.ReleaseMutex();}
   } return 0;
  }catch(Exception ex){File.WriteAllText(Artifacts.Path("CodexDashboard-error.log"),ex.ToString());if(!args.Contains("-port")&&!args.Any(x=>x.EndsWith("-test")))MessageBox.Show(ex.Message,"Codex Dashboard");return 1;}
 }
 static void HistoryFilterTest(){
  Environment.SetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR",Artifacts.Path("history-filter-test-"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(SettingsStore.DirectoryPath);var now=DateTimeOffset.UtcNow;var rows=new[]{-192,-144,-25,-1,1}.Select(h=>new HistorySample{At=now.AddHours(h).ToString("o"),Short=50,Weekly=80}).ToArray();File.WriteAllLines(QuotaHistory.FilePath,rows.Select(Json.Stringify));if(QuotaHistory.SelectPeriod(rows,1,now).Length!=1||QuotaHistory.SelectPeriod(rows,7,now).Length!=3)throw new Exception("History rolling period filtering failed");
  var window=QuotaHistory.Show(null);try{var coverage=Configurator.Control<TextBlock>(window,"HistoryCoverage");if(!coverage.Text.Contains("1"+L.T(" rilevazioni\nIl grafico usa il periodo con dati disponibili, non riempie le ore mancanti.")))throw new Exception("24-hour history UI failed");Configurator.Control<Button>(window,"HistoryWeekly").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(!coverage.Text.Contains("3"+L.T(" rilevazioni\nIl grafico usa il periodo con dati disponibili, non riempie le ore mancanti.")))throw new Exception("7-day history UI failed");Configurator.Control<Button>(window,"HistoryDaily").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(!coverage.Text.Contains("1"+L.T(" rilevazioni\nIl grafico usa il periodo con dati disponibili, non riempie le ore mancanti.")))throw new Exception("Switch back to daily failed");}finally{window.Close();}File.WriteAllText(Artifacts.Path("history-filter-test.txt"),"PASS: rolling 24h and 7d filters, old and future timestamps excluded; real buttons switch 1 / 3 / 1 samples.");
 }
 static void PumpFor(int milliseconds){var frame=new DispatcherFrame();var tick=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(milliseconds)};tick.Tick+=(s,e)=>{tick.Stop();frame.Continue=false;};tick.Start();Dispatcher.PushFrame(frame);}
 static void LifecycleTest(){
  Environment.SetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR",Artifacts.Path("lifecycle-test-"+Guid.NewGuid().ToString("N")));bool codexOpen=true;
  SettingsStore.Save(new UserSettings{Startup="codex",CloseMode="codex"},false);
  using(var meter=new Meter(false))using(var show=new EventWaitHandle(false,EventResetMode.AutoReset))using(var close=new EventWaitHandle(false,EventResetMode.AutoReset)){
   bool closed=false;var w=Widget.Create(meter,show,close,true,false,null,()=>codexOpen);w.Closed+=(s,e)=>closed=true;w.Show();PumpFor(3300);codexOpen=false;PumpFor(3300);if(!closed||((System.Windows.Forms.NotifyIcon)w.Resources["MonitorTray"]).Visible)throw new Exception("Codex closing must terminate widget and dispose tray");
   codexOpen=true;closed=false;w=Widget.Create(meter,show,close,true,false,null,()=>codexOpen);w.Closed+=(s,e)=>closed=true;w.Show();PumpFor(3300);if(!w.IsVisible)throw new Exception("New launch must reopen widget");SettingsStore.Save(new UserSettings{Startup="codex",CloseMode="manual"},false);codexOpen=false;PumpFor(3300);if(closed||!w.IsVisible)throw new Exception("Manual closing must preserve app");close.Set();PumpFor(500);if(!closed)throw new Exception("Explicit exit failed");
  }
  File.WriteAllText(Artifacts.Path("lifecycle-test.txt"),"PASS: full close and tray disposal after Codex closes, relaunch as a new instance, manual closing and explicit exit.");
 }
 static void PlacementTest() {
  Environment.SetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR",Artifacts.Path("placement-test-"+Guid.NewGuid().ToString("N")));
  var bounds=new Rect(-1920,0,1920,1040);var expected=new[]{new Point(-1896,24),new Point(-1070,24),new Point(-244,24),new Point(-1896,488),new Point(-1070,488),new Point(-244,488),new Point(-1896,952),new Point(-1070,952),new Point(-244,952)};
  for(int i=1;i<Placement.Positions.Length;i++){var point=Placement.Calculate(bounds,220,64,Placement.Positions[i],24);if(point!=expected[i-1])throw new Exception("Incorrect anchor: "+Placement.Positions[i]);}
  var tight=Placement.Calculate(new Rect(0,0,230,70),220,64,"bottom-right",120);if(tight!=new Point(5,3))throw new Exception("Margin must fit a small screen");
  var invalid=new UserSettings{WidgetPosition="invalid",EdgeMargin=999,MonitorDevice=null};invalid.Normalize();if(invalid.WidgetPosition!="custom"||invalid.EdgeMargin!=120||invalid.MonitorDevice!="")throw new Exception("Position settings validation failed");
  SettingsStore.Save(new UserSettings(),false);var dialog=Configurator.Create(null,null,false);dialog.Show();var panel=Configurator.Panel(dialog);var placement=Configurator.Control<ComboBox>(dialog,"WidgetPlacement");placement.SelectedIndex=9;Configurator.Control<Button>(dialog,"ApplySettings").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(!dialog.IsVisible||SettingsStore.Current.WidgetPosition!="bottom-right")throw new Exception("Position Apply failed");placement.SelectedIndex=1;Configurator.Control<Button>(dialog,"CancelSettings").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(SettingsStore.Current.WidgetPosition!="bottom-right")throw new Exception("Cancel changed applied position");
  SettingsStore.Save(new UserSettings{WidgetPosition="center",MonitorDevice="disconnected-monitor"},false);
  using(var meter=new Meter(false))using(var show=new EventWaitHandle(false,EventResetMode.AutoReset))using(var close=new EventWaitHandle(false,EventResetMode.AutoReset)){var widget=Widget.Create(meter,show,close,false);widget.Show();var frame=new DispatcherFrame();var tick=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};tick.Tick+=(s,e)=>{tick.Stop();frame.Continue=false;};tick.Start();Dispatcher.PushFrame(frame);var area=SystemParameters.WorkArea;bool centered=Math.Abs(widget.Left-(area.Left+(area.Width-widget.Width)/2))<2&&Math.Abs(widget.Top-(area.Top+(area.Height-widget.Height)/2))<2;widget.Close();if(!centered)throw new Exception("Live positioning or monitor fallback failed");}
  File.WriteAllText(Artifacts.Path("placement-test.txt"),"PASS: nine anchors, negative monitor coordinates, taskbar work area, small-screen margins, settings validation, Apply, Cancel, live widget movement, disconnected monitor fallback.");
 }
 static void FeaturesTest() {
  Environment.SetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR",Artifacts.Path("features-test-"+Guid.NewGuid().ToString("N")));
  var settings=new UserSettings{Language="invalid"};settings.Normalize();if(settings.Language!="it")throw new Exception("Language validation failed");SettingsStore.Save(settings,false);
  foreach(var mode in new[]{"manual","windows","codex"}){var command=SettingsStore.StartupCommand(new UserSettings{Startup=mode},"C:\\Example folder\\CodexDashboard.exe");if(mode=="manual"?command!=null:command!="\"C:\\Example folder\\CodexDashboard.exe\""+(mode=="codex"?" --watch-codex":""))throw new Exception("Startup command failed");}
  var w=Configurator.Create(null,null,false);w.Show();var root=Configurator.Panel(w);
  w.Height=450;w.UpdateLayout();var fixedSave=Configurator.Control<Button>(w,"SaveSettings");var fixedPoint=fixedSave.TransformToAncestor(w).Transform(new Point(0,0));if(fixedPoint.Y<0||fixedPoint.Y+fixedSave.ActualHeight>w.ActualHeight)throw new Exception("Settings footer must stay visible on short screens");
  var language=Configurator.Control<ComboBox>(w,"SettingsLanguage");language.SelectedIndex=1;Configurator.Control<Slider>(w,"ShortThreshold").Value=12;Configurator.Control<Slider>(w,"WeeklyThreshold").Value=28;Configurator.Control<ComboBox>(w,"CloseMode").SelectedIndex=1;
  var scale=Configurator.Control<Slider>(w,"WidgetScale");scale.Value=1.4;
  Configurator.Control<Button>(w,"ApplySettings").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
  if(!w.IsVisible||SettingsStore.Current.Language!="en"||SettingsStore.Current.Scale!=1.4||SettingsStore.Current.CloseMode!="codex"||SettingsStore.Current.ThresholdFor(300)!=12||SettingsStore.Current.ThresholdFor(10080)!=28||(string)Configurator.Control<Button>(w,"ApplySettings").Content!="Apply")throw new Exception("Apply or live localization failed");
  scale.Value=1.9;language.SelectedIndex=0;Configurator.Control<Button>(w,"CancelSettings").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
  if(SettingsStore.Current.Scale!=1.4||SettingsStore.Current.Language!="en")throw new Exception("Cancel must preserve applied settings and discard edits");
  w=Configurator.Create(null,null,false);w.Show();root=Configurator.Panel(w);language=Configurator.Control<ComboBox>(w,"SettingsLanguage");language.SelectedIndex=0;Configurator.Control<Button>(w,"SaveSettings").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(SettingsStore.Current.Language!="it")throw new Exception("Save language failed");
  foreach(var zoom in new[]{.8,1,1.5,2}){SettingsStore.Save(new UserSettings{Scale=zoom},false);using(var meter=new Meter(false))using(var show=new EventWaitHandle(false,EventResetMode.AutoReset))using(var close=new EventWaitHandle(false,EventResetMode.AutoReset)){var widget=Widget.Create(meter,show,close,false);widget.Show();var frame=new DispatcherFrame();var tick=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(300)};tick.Tick+=(s,e)=>{tick.Stop();frame.Continue=false;};tick.Start();Dispatcher.PushFrame(frame);widget.UpdateLayout();bool valid=Math.Abs(widget.Width-220*zoom)<1&&Math.Abs(widget.Height-64*zoom)<1;widget.Close();if(!valid)throw new Exception("Scale check failed: "+zoom);}}
  var snapshot=new Snapshot{Primary=new Quota{Remaining=42,Minutes=300},Secondary=new Quota{Remaining=85,Minutes=10080},Updated=DateTime.UtcNow,Error=null};QuotaHistory.Record(snapshot);QuotaHistory.Record(snapshot);var rows=QuotaHistory.Load();if(rows.Count!=1||rows[0].Short!=42||rows[0].Weekly!=85)throw new Exception("History or duplicate suppression failed");
  File.AppendAllText(QuotaHistory.FilePath,"\ninvalid-json\n");if(QuotaHistory.Load().Count!=1)throw new Exception("History recovery failed");snapshot.Error="offline";QuotaHistory.Record(snapshot);if(QuotaHistory.Load().Count!=1)throw new Exception("Offline history suppression failed");
  File.WriteAllText(Artifacts.Path("features-test.txt"),"PASS: language validation, Apply stays open, immediate language change, Cancel discards unapplied changes, Save language, history samples/deduplication/corrupt-line recovery/offline exclusion.");
 }
 static void SelfTest() {
  long expires=DateTimeOffset.UtcNow.ToUnixTimeSeconds()+86400;var expirySample=Snapshot.Read(Json.Parse("{\"rateLimits\":{},\"rateLimitResetCredits\":{\"availableCount\":1,\"credits\":[{\"status\":\"used\",\"expiresAt\":"+(expires-10)+"},{\"status\":\"available\",\"expiresAt\":1},{\"status\":\"available\",\"expiresAt\":"+expires+"}]}}"));if(expirySample.NextResetExpiry!=expires)throw new Exception("Available reset expiry selection failed");

  var desktopPath=@"C:\Program Files\WindowsApps\OpenAI.Codex_test\app\ChatGPT.exe";if(SettingsStore.IsCodexDesktopWindow(desktopPath,IntPtr.Zero)||!SettingsStore.IsCodexDesktopWindow(desktopPath,new IntPtr(1)))throw new Exception("Background Codex processes must not count as an open window");

  var legacyThresholds=new UserSettings{LowThreshold=22};legacyThresholds.Normalize();if(legacyThresholds.ThresholdFor(300)!=22||legacyThresholds.ThresholdFor(10080)!=22)throw new Exception("Legacy threshold migration failed");
  var separateThresholds=new UserSettings{Notifications=true,ShortLowThreshold=10,WeeklyLowThreshold=25};separateThresholds.Normalize();var separateAlerts=new LowQuotaAlert();var separateData=new Snapshot{Primary=new Quota{Minutes=300,Remaining=20},Secondary=new Quota{Minutes=10080,Remaining=20},Updated=DateTime.UtcNow,Error=null};var separateMessage=separateAlerts.Check(separateData,separateThresholds);if(separateMessage==null||separateMessage.Contains("5 ")||separateAlerts.Check(separateData,separateThresholds)!=null)throw new Exception("Independent thresholds or duplicate alerts failed");separateData.Primary.Remaining=5;if(separateAlerts.Check(separateData,separateThresholds)==null)throw new Exception("Short quota alert failed");

  foreach(var count in new[]{0,2}){var resetSample=Snapshot.Read(Json.Parse("{\"rateLimits\":{},\"rateLimitResetCredits\":{\"availableCount\":"+count+"}}"));if(resetSample.AvailableResets!=count||!resetSample.ResetCreditsText.Contains(count.ToString()))throw new Exception("Reset credits count failed");}
  if(Snapshot.Read(Json.Parse("{\"rateLimits\":{}}")).AvailableResets.HasValue)throw new Exception("Missing reset count must stay unknown");
  if(!SettingsStore.IsCodexDesktopPath(@"C:\Program Files\WindowsApps\OpenAI.Codex_26.1002_x64__test\app\ChatGPT.exe")||SettingsStore.IsCodexDesktopPath(@"C:\Program Files\WindowsApps\OpenAI.ChatGPT_test\app\ChatGPT.exe")||SettingsStore.IsCodexDesktopPath(@"C:\Users\test\AppData\Local\OpenAI\Codex\bin\test\codex.exe"))throw new Exception("Codex desktop detection failed");
  var s=Snapshot.Read(Json.Parse("{\"rateLimits\":{\"primary\":{\"usedPercent\":25,\"windowDurationMins\":300},\"secondary\":null}}"));
  if(s.Primary.Remaining!=75||s.Secondary!=null)throw new Exception("Quota conversion failed");
  s=Snapshot.Read(Json.Parse("{\"rateLimits\":{\"primary\":{\"usedPercent\":0,\"windowDurationMins\":300}},\"rateLimitsByLimitId\":{\"codex\":{\"primary\":{\"usedPercent\":120,\"windowDurationMins\":10080}}}}"));
  if(s.Primary.Remaining!=0||s.Primary.Label!="SETTIMANA")throw new Exception("Bucket selection failed");
  if(Quota.Read(Json.Parse("{\"usedPercent\":null,\"windowDurationMins\":300}"))!=null)throw new Exception("Missing data failed");
  if(Quota.Read(Json.Parse("{\"usedPercent\":-10,\"windowDurationMins\":300}")).Remaining!=100)throw new Exception("Clamp failed");
  File.WriteAllText(Artifacts.Path("self-test.txt"),"PASS: remaining, missing quota, bucket priority, clamping, window labels.");
 }
 static void SettingsTest() {
  Environment.SetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR",Artifacts.Path("settings-test"));
  var invalid=new UserSettings{CloseMode="invalid",Scale=double.NaN,Opacity=5,PollSeconds=1,LowThreshold=100,Startup="invalid"};invalid.Normalize();
  if(invalid.Scale!=1||invalid.Opacity!=1||invalid.PollSeconds!=30||invalid.LowThreshold!=50||invalid.Startup!="manual"||invalid.CloseMode!="manual")throw new Exception("Settings validation failed");
  var saved=new UserSettings{Scale=1.5,Opacity=.6,PollSeconds=300,LowThreshold=20,ShowUsed=true};SettingsStore.Save(saved,false);
  if(SettingsStore.Current.Scale!=1.5||SettingsStore.Current.PollSeconds!=300||!SettingsStore.Current.ShowUsed)throw new Exception("Settings persistence failed");
  var alerts=new LowQuotaAlert();var options=new UserSettings{Notifications=true,LowThreshold=15};var snapshot=new Snapshot{Primary=new Quota{Remaining=10,Minutes=300},Updated=DateTime.UtcNow,Error=null};
  if(alerts.Check(snapshot,options)==null||alerts.Check(snapshot,options)!=null)throw new Exception("Duplicate alert prevention failed");
  snapshot.Primary.Remaining=20;alerts.Check(snapshot,options);snapshot.Primary.Remaining=10;if(alerts.Check(snapshot,options)==null)throw new Exception("Alert rearming failed");
  snapshot.Error="offline";if(alerts.Check(snapshot,options)!=null)throw new Exception("Offline alert suppression failed");
  var dialog=Configurator.Create(null,null,false);var panel=Configurator.Panel(dialog);
  var scale=Configurator.Control<Slider>(dialog,"WidgetScale");scale.Value=1.8;
  Configurator.Control<Button>(dialog,"SaveSettings").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
  if(Math.Abs(SettingsStore.Current.Scale-1.8)>.001)throw new Exception("Configurator save failed");
  using(var meter=new Meter(false))using(var show=new EventWaitHandle(false,EventResetMode.AutoReset))using(var close=new EventWaitHandle(false,EventResetMode.AutoReset)) {
   var widget=Widget.Create(meter,show,close,false);widget.Show();var frame=new DispatcherFrame();var tick=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(500)};tick.Tick+=(s,e)=>{tick.Stop();frame.Continue=false;};tick.Start();Dispatcher.PushFrame(frame);widget.UpdateLayout();
   bool valid=Math.Abs(widget.Width-396)<1&&Math.Abs(widget.Height-115.2)<1&&((Grid)widget.Content).ActualWidth<221;
   File.WriteAllText(Artifacts.Path("settings-scaling.txt"),"Width="+widget.Width+" Height="+widget.Height+" ContentWidth="+((Grid)widget.Content).ActualWidth);
   var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(widget.ActualWidth),(int)Math.Ceiling(widget.ActualHeight),96,96,PixelFormats.Pbgra32);bitmap.Render(widget);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using(var file=File.Create(Artifacts.Path("settings-scaling.png")))encoder.Save(file);
   widget.Close();if(!valid)throw new Exception("Live widget scaling failed");
  }
  var old=new Snapshot{Updated=DateTime.UtcNow.AddSeconds(-200),Error=null};if(!old.Online)throw new Exception("Configured freshness interval failed");
  File.WriteAllText(Artifacts.Path("settings-test.txt"),"PASS: bounds, persistence, shared settings, configurator save, live widget scaling, polling freshness, notification deduplication, rearming, offline suppression.");
 }
}
sealed class HudMenuRenderer : System.Windows.Forms.ToolStripProfessionalRenderer {
 protected override void OnRenderToolStripBackground(System.Windows.Forms.ToolStripRenderEventArgs e) {e.Graphics.Clear(System.Drawing.ColorTranslator.FromHtml(Theme.background));}
 protected override void OnRenderImageMargin(System.Windows.Forms.ToolStripRenderEventArgs e) {using(var b=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(Theme.background)))e.Graphics.FillRectangle(b,e.AffectedBounds);}
 protected override void OnRenderMenuItemBackground(System.Windows.Forms.ToolStripItemRenderEventArgs e) {if(e.Item.Selected){using(var b=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(Theme.hover)))e.Graphics.FillRectangle(b,new System.Drawing.Rectangle(System.Drawing.Point.Empty,e.Item.Size));}using(var p=new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(Theme.yellow),2))e.Graphics.DrawLine(p,1,2,1,e.Item.Height-2);}
}
static class Widget {
 static Brush Color(string hex) {return (Brush)new BrushConverter().ConvertFromString(hex);}
 public static Window Create(Meter meter,EventWaitHandle show,EventWaitHandle close,bool persist=true,bool watchCodex=false,EventWaitHandle configure=null,Func<bool> codexDetector=null) {
  string xaml;using(var reader=new StreamReader(typeof(Widget).Assembly.GetManifestResourceStream("Widget.xaml")))xaml=reader.ReadToEnd();
  var w=(Window)System.Windows.Markup.XamlReader.Parse(xaml);var ui=new Dictionary<string,object>();
  foreach(var name in new[]{"Controls","Close","Refresh","DragArea","Status","PrimaryValue","PrimaryLabel","PrimaryBar","PrimaryCard","SecondaryValue","SecondaryLabel","SecondaryBar","SecondaryCard"})ui[name]=w.FindName(name);
  var area=SystemParameters.WorkArea;w.Left=area.Right-w.Width-24;w.Top=area.Bottom-w.Height-24;
  string settings=Path.Combine(SettingsStore.DirectoryPath,"position.json");
  try{var p=Json.Parse(File.ReadAllText(settings));w.Left=Convert.ToDouble(Json.Get(p,"left"));w.Top=Convert.ToDouble(Json.Get(p,"top"));}catch{}
  var virtualArea=new Rect(SystemParameters.VirtualScreenLeft,SystemParameters.VirtualScreenTop,SystemParameters.VirtualScreenWidth,SystemParameters.VirtualScreenHeight);
  if(!virtualArea.Contains(new Rect(w.Left,w.Top,w.Width,w.Height))){w.Left=area.Right-w.Width-24;w.Top=area.Bottom-w.Height-24;}
  string appliedPlacement=null;bool hovered=false;w.MouseEnter+=(s,e)=>{hovered=true;w.Opacity=1;((StackPanel)ui["Controls"]).Opacity=1;};w.MouseLeave+=(s,e)=>{hovered=false;w.Opacity=SettingsStore.Current.Opacity;((StackPanel)ui["Controls"]).Opacity=0;};
  ((Button)ui["Close"]).Click+=(s,e)=>{if((persist||watchCodex)&&SettingsStore.Current.Startup=="codex")w.Hide();else w.Close();};((Button)ui["Refresh"]).Click+=(s,e)=>meter.Refresh();
  ((Grid)ui["DragArea"]).MouseLeftButtonDown+=(s,e)=>{if(e.OriginalSource is TextBlock||e.OriginalSource==ui["DragArea"]) {double left=w.Left,top=w.Top;w.DragMove();if(persist&&(Math.Abs(w.Left-left)>.5||Math.Abs(w.Top-top)>.5)){var prefs=new JavaScriptSerializer().Deserialize<UserSettings>(Json.Stringify(SettingsStore.Current));prefs.WidgetPosition="custom";SettingsStore.Save(prefs,false);}}};
  System.Drawing.Icon icon;using(var stream=typeof(Widget).Assembly.GetManifestResourceStream("Dashboard.ico"))using(var original=new System.Drawing.Icon(stream))icon=(System.Drawing.Icon)original.Clone();
  var tray=new System.Windows.Forms.NotifyIcon {Icon=icon,Text="Codex Dashboard",Visible=true};w.Resources["MonitorTray"]=tray;
  var menu=new System.Windows.Forms.ContextMenuStrip {BackColor=System.Drawing.ColorTranslator.FromHtml(Theme.background),ForeColor=System.Drawing.ColorTranslator.FromHtml(Theme.cyan),Renderer=new HudMenuRenderer(),Font=new System.Drawing.Font(Theme.font,10)};menu.Items.Add("Mostra widget",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>{w.Show();w.Activate();})));menu.Items.Add("Aggiorna",null,(s,e)=>meter.Refresh());menu.Items.Add("Esci",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>w.Close())));tray.ContextMenuStrip=menu;tray.DoubleClick+=(s,e)=>{w.Show();w.Activate();};
  Action center=()=>{var desktop=SystemParameters.WorkArea;w.Left=desktop.Left+(desktop.Width-w.Width)/2;w.Top=desktop.Top+(desktop.Height-w.Height)/2;w.Show();};
  menu.Items.Insert(1,new System.Windows.Forms.ToolStripMenuItem("Impostazioni",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>Configurator.Show(w,center)))));
  var context=new ContextMenu();var configItem=new MenuItem{Header="Impostazioni…"};configItem.Click+=(s,e)=>Configurator.Show(w,center);context.Items.Add(configItem);w.ContextMenu=context;
  menu.Items.Insert(3,new System.Windows.Forms.ToolStripMenuItem("Storico quote",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>QuotaHistory.Show(w)))));
  menu.Items.Insert(4,new System.Windows.Forms.ToolStripMenuItem("Informazioni e diagnostica",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>Info.Show(w,meter.State)))));
  foreach(var item in menu.Items.OfType<System.Windows.Forms.ToolStripItem>())item.Tag=item.Text;
  var historyItem=new MenuItem{Header="Storico quote"};historyItem.Click+=(s,e)=>QuotaHistory.Show(w);context.Items.Add(historyItem);
  var aboutItem=new MenuItem{Header="Informazioni e diagnostica"};aboutItem.Click+=(s,e)=>Info.Show(w,meter.State);context.Items.Add(aboutItem);
  var alerts=new LowQuotaAlert();double appliedScale=0;DateTime watched=DateTime.MinValue;bool wasOpen=false,seenCodex=false;
  var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(250)};timer.Tick+=(s,e)=>{
   if(close.WaitOne(0)){w.Close();return;}if(show.WaitOne(0)){w.Show();w.Activate();}
   if(configure!=null&&configure.WaitOne(0))Configurator.Show(w,center);
   var prefs=SettingsStore.Current;if(prefs.Startup!="codex"||prefs.CloseMode!="codex")tray.Visible=true;((Button)ui["Close"]).ToolTip=L.T(prefs.Startup=="codex"?"Nascondi widget · il monitor resta attivo":"Chiudi programma");
   foreach(var item in menu.Items.OfType<System.Windows.Forms.ToolStripItem>())item.Text=L.T((string)item.Tag);
   configItem.Header=L.T("Impostazioni…");historyItem.Header=L.T("Storico quote");aboutItem.Header=L.T("Informazioni e diagnostica");
   if(appliedScale!=prefs.Scale){appliedScale=prefs.Scale;((Grid)w.Content).LayoutTransform=new ScaleTransform(prefs.Scale,prefs.Scale);w.Width=220*prefs.Scale;w.Height=64*prefs.Scale;var desktop=SystemParameters.WorkArea;w.Left=Math.Max(SystemParameters.VirtualScreenLeft,Math.Min(w.Left,SystemParameters.VirtualScreenLeft+SystemParameters.VirtualScreenWidth-w.Width));w.Top=Math.Max(SystemParameters.VirtualScreenTop,Math.Min(w.Top,SystemParameters.VirtualScreenTop+SystemParameters.VirtualScreenHeight-w.Height));}
   string placementKey=prefs.WidgetPosition+"|"+prefs.MonitorDevice+"|"+prefs.EdgeMargin+"|"+prefs.Scale+"|"+Placement.ScreenSignature();if(appliedPlacement!=placementKey){appliedPlacement=placementKey;if(prefs.WidgetPosition!="custom")Placement.Apply(w,prefs);}
   w.Topmost=prefs.Topmost;if(!hovered)w.Opacity=prefs.Opacity;
   var data=meter.State;
   ((Grid)ui["DragArea"]).ToolTip=data.ResetCreditsText+"\n"+L.T("Trascina per spostare · dati ogni ")+prefs.PollSeconds+L.T(" s · tasto destro per impostazioni");
   if(((persist||watchCodex)&&prefs.Startup=="codex"||prefs.CloseMode=="codex")&&(DateTime.UtcNow-watched).TotalSeconds>=3){watched=DateTime.UtcNow;bool open=codexDetector!=null?codexDetector():SettingsStore.CodexIsOpen();bool followStartup=(persist||watchCodex)&&prefs.Startup=="codex";tray.Visible=!(followStartup&&prefs.CloseMode=="codex"&&!open);if(open){seenCodex=true;if(followStartup&&!wasOpen)w.Show();}else if(followStartup&&!seenCodex)w.Hide();else if(seenCodex&&prefs.CloseMode=="codex"){{w.Close();return;}}wasOpen=open;}
   string notification=alerts.Check(data,prefs);if(persist&&notification!=null){tray.BalloonTipTitle=L.T("Codex Dashboard · Quota bassa");tray.BalloonTipText=notification;tray.ShowBalloonTip(5000);}
   foreach(var prefix in new[]{"Primary","Secondary"}) {
    var q=prefix=="Primary"?data.Primary:data.Secondary;var value=(TextBlock)ui[prefix+"Value"];var bar=(ProgressBar)ui[prefix+"Bar"];
    double display=q==null?0:prefs.ShowUsed?100-q.Remaining:q.Remaining;value.Text=q==null?"—":display.ToString("0.#")+"%";bar.Value=display;
    ((TextBlock)ui[prefix+"Label"]).Text=L.T(q==null?"NON DISP.":q.Label);
    bar.Foreground=Color(q!=null&&q.Remaining<=prefs.ThresholdFor(q.Minutes)?Theme.alert:prefix=="Primary"?Theme.yellow:Theme.cyan);
    value.Foreground=Color(q!=null&&q.Remaining<=prefs.ThresholdFor(q.Minutes)?Theme.alert:prefix=="Primary"?Theme.yellow:Theme.cyan);
    ((StackPanel)ui[prefix+"Card"]).ToolTip=q==null?L.T("Dato non disponibile"):value.Text+L.T(prefs.ShowUsed?" consumata · ":" disponibile · ")+q.ResetText;((StackPanel)ui[prefix+"Card"]).ToolTip+="\n"+data.ResetCreditsText;
   }
   var status=(TextBlock)ui["Status"];status.Text=data.Online?"●":"!";status.Foreground=Color(data.Online?Theme.cyan:Theme.alert);
   status.ToolTip=data.Online?L.T("Aggiornato alle ")+data.Updated.Value.ToLocalTime().ToString("HH:mm:ss")+" · "+data.Plan+L.T(prefs.ShowUsed?" · Quota consumata":" · Quota disponibile"):L.T("Dati non aggiornati · ")+L.T(data.Error??"In attesa di aggiornamento");status.ToolTip+="\n"+data.ResetCreditsText;
  };timer.Start();
  w.Closed+=(s,e)=>{timer.Stop();tray.Dispose();icon.Dispose();menu.Font.Dispose();menu.Dispose();if(persist)try{Directory.CreateDirectory(Path.GetDirectoryName(settings));File.WriteAllText(settings,Json.Stringify(new {left=w.Left,top=w.Top}));}catch{}};
  return w;
 }
}
static class Plugin {
 static object ReadKeySettings(Dictionary<string,object> settings,object gate,string context){lock(gate){object value;return settings.TryGetValue(context,out value)?value:null;}}
 static readonly SemaphoreSlim sendLock=new SemaphoreSlim(1,1);
 static async Task Send(ClientWebSocket ws,object value,CancellationToken ct) {var bytes=Encoding.UTF8.GetBytes(Json.Stringify(value));await sendLock.WaitAsync(ct);try{await ws.SendAsync(new ArraySegment<byte>(bytes),WebSocketMessageType.Text,true,ct);}finally{sendLock.Release();}}
 static string Image(Quota q,bool online,bool launcher,bool weekly=false,object settings=null) {
  var prefs=SettingsStore.Current;var mode=Json.Str(settings,"mode");bool used=mode=="used"||mode!="available"&&prefs.ShowUsed;
  var language=Json.Str(settings,"language");bool english=language=="en"||language!="it"&&L.IsEnglish;
  int threshold=prefs.ThresholdFor(weekly?10080:300);int custom;if(int.TryParse(Json.Str(settings,"threshold"),out custom))threshold=Math.Max(1,Math.Min(50,custom));
  double display=q==null?0:used?100-q.Remaining:q.Remaining;
  string color=q!=null&&q.Remaining<=threshold?Theme.alert:weekly?Theme.cyan:Theme.yellow;
  string label=launcher?"CODEX":L.T(weekly?"SETT.":"5 ORE",english);
  string value=launcher?L.T("APRI",english):q==null?"—":display.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)+"%";
  string bottom=launcher?"":!online?"OFFLINE":q==null?L.T("NON DISP.",english):"";
  using(var bitmap=new System.Drawing.Bitmap(144,144))using(var g=System.Drawing.Graphics.FromImage(bitmap))using(var accent=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(color)))using(var header=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(weekly?Theme.cyan:Theme.yellow)))using(var yellow=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(Theme.yellow)))using(var frame=new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(Theme.track),1))using(var cyan=new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(Theme.cyan),2))using(var track=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(Theme.track)))using(var alert=new System.Drawing.SolidBrush(System.Drawing.ColorTranslator.FromHtml(Theme.alert)))using(var detail=new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(Theme.cyan),1)) {
   g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
   g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
   g.Clear(System.Drawing.ColorTranslator.FromHtml(Theme.background));
   g.DrawPolygon(frame,new[]{new System.Drawing.Point(0,0),new System.Drawing.Point(128,0),new System.Drawing.Point(143,15),new System.Drawing.Point(143,143),new System.Drawing.Point(13,143),new System.Drawing.Point(0,130)});
   g.FillPolygon(yellow,new[]{new System.Drawing.Point(0,0),new System.Drawing.Point(128,0),new System.Drawing.Point(144,16),new System.Drawing.Point(144,20),new System.Drawing.Point(127,4),new System.Drawing.Point(0,4)});
   g.DrawLines(cyan,new[]{new System.Drawing.Point(1,111),new System.Drawing.Point(1,131),new System.Drawing.Point(13,143),new System.Drawing.Point(39,143)});
   g.DrawLine(detail,141,34,141,45);g.DrawLine(detail,141,49,141,54);
   if(launcher) {
    DrawCentered(g,"CODEX",header,new System.Drawing.RectangleF(10,14,124,25),23);
    DrawCentered(g,"DASHBOARD",header,new System.Drawing.RectangleF(10,38,124,25),21);
    DrawCentered(g,value,accent,new System.Drawing.RectangleF(8,67,128,49),48);
   } else {
    DrawCentered(g,label,header,new System.Drawing.RectangleF(10,14,124,30),25);
    DrawCentered(g,value,accent,new System.Drawing.RectangleF(8,43,128,68),64);
   }
   int barY=bottom.Length==0?125:112;
   if(!launcher){g.FillRectangle(track,8,barY,128,3);if(q!=null)g.FillRectangle(accent,8,barY,(float)(display*1.28),3);}
   if(bottom.Length>0)DrawCentered(g,bottom,alert,new System.Drawing.RectangleF(8,122,128,21),20);
   else {g.DrawLine(detail,42,141,103,141);g.DrawLine(detail,108,141,117,141);}
   using(var stream=new MemoryStream()){bitmap.Save(stream,System.Drawing.Imaging.ImageFormat.Png);return "data:image/png;base64,"+Convert.ToBase64String(stream.ToArray());}
  }
 }
 static void DrawCentered(System.Drawing.Graphics g,string text,System.Drawing.Brush brush,System.Drawing.RectangleF bounds,int maxSize) {
  using(var format=(System.Drawing.StringFormat)System.Drawing.StringFormat.GenericTypographic.Clone()) {
   format.Alignment=System.Drawing.StringAlignment.Center;format.LineAlignment=System.Drawing.StringAlignment.Center;format.FormatFlags=System.Drawing.StringFormatFlags.NoWrap;
   for(int size=maxSize;size>=12;size--)using(var font=new System.Drawing.Font(Theme.font,size,System.Drawing.FontStyle.Bold,System.Drawing.GraphicsUnit.Pixel)) {
    var measured=g.MeasureString(text,font,new System.Drawing.SizeF(1000,1000),format);
    if(measured.Width<=bounds.Width-4&&measured.Height<=bounds.Height-2){g.DrawString(text,font,brush,bounds,format);return;}
   }
  }
 }
 public static void WritePreview() {
  var names=new[]{"short","weekly","open","full","decimal","offline"};
  var quotas=new[]{new Quota{Remaining=8,Minutes=300},new Quota{Remaining=82,Minutes=10080},null,new Quota{Remaining=100,Minutes=300},new Quota{Remaining=99.9,Minutes=300},new Quota{Remaining=0,Minutes=300}};
  for(int i=0;i<names.Length;i++){var data=Image(quotas[i],i!=5,i==2,i==1);File.WriteAllBytes(Artifacts.Path("streamdeck-"+names[i]+"-preview.png"),Convert.FromBase64String(data.Split(',')[1]));}
 }
 public static async Task Run(string[] args) {
  var options=new Dictionary<string,string>();for(int i=0;i+1<args.Length;i+=2)options[args[i]]=args[i+1];
  int port=int.Parse(options["-port"]);if(port<1||port>65535)throw new Exception("Invalid Stream Deck port");
  using(var ws=new ClientWebSocket())using(var ct=new CancellationTokenSource())using(var meter=new Meter()) {
   await ws.ConnectAsync(new Uri("ws://127.0.0.1:"+port),ct.Token);
   await Send(ws,new { @event=options["-registerEvent"],uuid=options["-pluginUUID"]},ct.Token);
   var actions=new Dictionary<string,string>();var keySettings=new Dictionary<string,object>();var actionGate=new object();
   var update=Task.Run(async delegate {try{while(ws.State==WebSocketState.Open&&!ct.IsCancellationRequested){KeyValuePair<string,string>[] current;lock(actionGate)current=actions.ToArray();var data=meter.State;foreach(var a in current){bool launch=a.Value.EndsWith(".open");var q=a.Value.EndsWith(".weekly")?new[]{data.Primary,data.Secondary}.FirstOrDefault(x=>x!=null&&x.Minutes==10080):new[]{data.Primary,data.Secondary}.FirstOrDefault(x=>x!=null&&x.Minutes==300);await Send(ws,new {@event="setImage",context=a.Key,payload=new {image=Image(q,data.Online,launch,a.Value.EndsWith(".weekly"),ReadKeySettings(keySettings,actionGate,a.Key)),target=0}},ct.Token);await Send(ws,new {@event="setTitle",context=a.Key,payload=new {title="",target=0}},ct.Token);}await Task.Delay(2000,ct.Token);}}catch{ct.Cancel();ws.Abort();}});
   try{var buffer=new byte[8192];while(ws.State==WebSocketState.Open){using(var message=new MemoryStream()){WebSocketReceiveResult received;do{received=await ws.ReceiveAsync(new ArraySegment<byte>(buffer),ct.Token);if(received.MessageType==WebSocketMessageType.Close)return;message.Write(buffer,0,received.Count);if(message.Length>1048576)throw new Exception("Stream Deck message too large");}while(!received.EndOfMessage);
    var m=Json.Parse(Encoding.UTF8.GetString(message.ToArray()));var ev=Json.Str(m,"event");var context=Json.Str(m,"context");var action=Json.Str(m,"action");
    if(ev=="willAppear") {lock(actionGate){actions[context]=action;keySettings[context]=Json.Get(Json.Get(m,"payload"),"settings");}}
    else if(ev=="didReceiveSettings"){lock(actionGate)keySettings[context]=Json.Get(Json.Get(m,"payload"),"settings");}
    else if(ev=="willDisappear"){lock(actionGate){actions.Remove(context);keySettings.Remove(context);}}
    else if(ev=="sendToPlugin"||ev=="propertyInspectorDidAppear"){await Send(ws,new {@event="sendToPropertyInspector",action=action,context=context,payload=new {language=SettingsStore.Current.Language,version=AppVersion.Value}},ct.Token);}
    else if(ev=="keyDown"){if(action.EndsWith(".open")){Process.Start(new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location,"--widget"){UseShellExecute=false,CreateNoWindow=true});}else meter.Refresh();}
   }}}finally{ct.Cancel();ws.Abort();try{update.GetAwaiter().GetResult();}catch{}}
  }
 }
}
}
