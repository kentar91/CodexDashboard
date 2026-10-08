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
 public string ResetText {get { if(!Reset.HasValue) return "Rinnovo non disponibile"; try { var zone=TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); return "Si rinnova "+TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(Reset.Value),zone).ToString("dd/MM · HH:mm"); } catch { return "Rinnovo non disponibile"; } }}
}
public class Snapshot {
 public Quota Primary,Secondary; public DateTime? Updated; public string Plan=""; public string Error="Connessione a Codex…";
 public bool Online {get {return Updated.HasValue && Error==null && (DateTime.UtcNow-Updated.Value).TotalSeconds<90;}}
 public static Snapshot Read(object data) {
  var buckets=Json.Get(data,"rateLimitsByLimitId");
  var limits=Json.Get(buckets,"codex")??Json.Get(data,"rateLimits");
  if(limits==null) throw new Exception("Limiti non disponibili per questo account.");
  return new Snapshot {Primary=Quota.Read(Json.Get(limits,"primary")),Secondary=Quota.Read(Json.Get(limits,"secondary")),Plan=Json.Str(limits,"planType"),Updated=DateTime.UtcNow,Error=null};
 }
}
sealed class Meter : IDisposable {
 readonly object gate=new object(); Process child; System.Threading.Timer timer; bool ready,pending,disposed; int requestId=10; DateTime last=DateTime.MinValue; Snapshot state=new Snapshot();
 public Snapshot State {get {lock(gate) return state;}}
 public string Diagnostic {get {lock(gate)return "child="+(child==null?"none":child.Id.ToString())+" ready="+ready+" pending="+pending+" request="+requestId;}}
 public Meter() { timer=new System.Threading.Timer(delegate {Tick();},null,0,1000); }
 static string FindCodex() {
  var root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"OpenAI","Codex","bin");
  if(Directory.Exists(root)) {var f=Directory.GetFiles(root,"codex.exe",SearchOption.AllDirectories).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();if(f!=null)return f;}
  foreach(var dir in (Environment.GetEnvironmentVariable("PATH")??"").Split(';')) { try {var f=Path.Combine(dir.Trim('"'),"codex.exe");if(File.Exists(f))return f;}catch{} }
  throw new Exception("Codex non trovato. Installa Codex e accedi al tuo account.");
 }
 void Fail(string message) {state=new Snapshot {Primary=state.Primary,Secondary=state.Secondary,Updated=state.Updated,Plan=state.Plan,Error=message};}
 void StopChild() {if(child!=null){try{if(!child.HasExited)child.Kill();}catch{}child.Dispose();child=null;}ready=false;pending=false;}
 void Tick() {lock(gate) {if(disposed)return;try {
  if(child==null||child.HasExited) {if((DateTime.UtcNow-last).TotalSeconds<15)return;StopChild();last=DateTime.UtcNow;
   var p=new Process {StartInfo=new ProcessStartInfo(FindCodex(),"app-server --listen stdio://") {UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8}};
   child=p;p.OutputDataReceived+=(s,e)=>{if(e.Data!=null)Handle(p,e.Data);};p.ErrorDataReceived+=(s,e)=>{};
   p.Start();p.BeginOutputReadLine();p.BeginErrorReadLine();
   Send(new {id=1,method="initialize",@params=new {clientInfo=new {name="codex_dashboard",title="Codex Dashboard",version="1.0.0"}}});
  } else if((!ready||pending)&&(DateTime.UtcNow-last).TotalSeconds>25) {StopChild();last=DateTime.UtcNow;Fail("Codex non risponde · riconnessione automatica");}
  else if(ready&&!pending&&(DateTime.UtcNow-last).TotalSeconds>=60)Request();
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
   state=Snapshot.Read(Json.Get(m,method=="account/rateLimits/updated"?"params":"result"));
  }
 }catch(Exception ex){pending=false;Fail(ex.Message);}}}
 public void Dispose() {lock(gate){disposed=true;timer.Dispose();StopChild();}}
}
static class Program {
 [STAThread] static int Main(string[] args) {
  try {
   if(args.Contains("--self-test")) {SelfTest();return 0;}
   if(args.Contains("-port")) {Plugin.Run(args).GetAwaiter().GetResult();return 0;}
   if(args.Contains("--verify")) {using(var meter=new Meter()){var until=DateTime.UtcNow.AddSeconds(35);while(!meter.State.Online&&DateTime.UtcNow<until){Thread.Sleep(200);File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"verify-progress.txt"),meter.Diagnostic);}File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"verification.json"),Json.Stringify(meter.State),Encoding.UTF8);return meter.State.Online?0:1;}}
   bool owned;using(var mutex=new Mutex(true,"Local\\CodexDashboardNative",out owned)) {
    if(!owned){using(var show=EventWaitHandle.OpenExisting("Local\\CodexDashboardNativeShow"))show.Set();return 0;}
    using(var meter=new Meter())using(var show=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\CodexDashboardNativeShow"))using(var close=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\CodexDashboardWidgetClose")) {
     var app=new Application();var window=Widget.Create(meter,show,close);app.Run(window);
    }
   } return 0;
  }catch(Exception ex){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"CodexDashboard-error.log"),ex.ToString());if(!args.Contains("-port"))MessageBox.Show(ex.Message,"Codex Dashboard");return 1;}
 }
 static void SelfTest() {
  var s=Snapshot.Read(Json.Parse("{\"rateLimits\":{\"primary\":{\"usedPercent\":25,\"windowDurationMins\":300},\"secondary\":null}}"));
  if(s.Primary.Remaining!=75||s.Secondary!=null)throw new Exception("Quota conversion failed");
  s=Snapshot.Read(Json.Parse("{\"rateLimits\":{\"primary\":{\"usedPercent\":0,\"windowDurationMins\":300}},\"rateLimitsByLimitId\":{\"codex\":{\"primary\":{\"usedPercent\":120,\"windowDurationMins\":10080}}}}"));
  if(s.Primary.Remaining!=0||s.Primary.Label!="SETTIMANA")throw new Exception("Bucket selection failed");
  if(Quota.Read(Json.Parse("{\"usedPercent\":null,\"windowDurationMins\":300}"))!=null)throw new Exception("Missing data failed");
  if(Quota.Read(Json.Parse("{\"usedPercent\":-10,\"windowDurationMins\":300}")).Remaining!=100)throw new Exception("Clamp failed");
  File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"self-test.txt"),"PASS: remaining, missing quota, bucket priority, clamping, window labels.");
 }
}
static class Widget {
 static Brush Color(string hex) {return (Brush)new BrushConverter().ConvertFromString(hex);}
 public static Window Create(Meter meter,EventWaitHandle show,EventWaitHandle close) {
  string xaml;using(var reader=new StreamReader(typeof(Widget).Assembly.GetManifestResourceStream("Widget.xaml")))xaml=reader.ReadToEnd();
  var w=(Window)System.Windows.Markup.XamlReader.Parse(xaml);var ui=new Dictionary<string,object>();
  foreach(var name in new[]{"Controls","Close","Refresh","DragArea","Status","PrimaryValue","PrimaryLabel","PrimaryBar","PrimaryCard","SecondaryValue","SecondaryLabel","SecondaryBar","SecondaryCard"})ui[name]=w.FindName(name);
  var area=SystemParameters.WorkArea;w.Left=area.Right-w.Width-24;w.Top=area.Bottom-w.Height-24;
  string settings=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CodexDashboard","position.json");
  try{var p=Json.Parse(File.ReadAllText(settings));w.Left=Convert.ToDouble(Json.Get(p,"left"));w.Top=Convert.ToDouble(Json.Get(p,"top"));}catch{}
  var virtualArea=new Rect(SystemParameters.VirtualScreenLeft,SystemParameters.VirtualScreenTop,SystemParameters.VirtualScreenWidth,SystemParameters.VirtualScreenHeight);
  if(!virtualArea.Contains(new Rect(w.Left,w.Top,w.Width,w.Height))){w.Left=area.Right-w.Width-24;w.Top=area.Bottom-w.Height-24;}
  w.MouseEnter+=(s,e)=>{w.Opacity=1;((StackPanel)ui["Controls"]).Opacity=1;};w.MouseLeave+=(s,e)=>{w.Opacity=.82;((StackPanel)ui["Controls"]).Opacity=0;};
  ((Button)ui["Close"]).Click+=(s,e)=>w.Close();((Button)ui["Refresh"]).Click+=(s,e)=>meter.Refresh();
  ((Grid)ui["DragArea"]).MouseLeftButtonDown+=(s,e)=>{if(e.OriginalSource is TextBlock||e.OriginalSource==ui["DragArea"])w.DragMove();};
  var tray=new System.Windows.Forms.NotifyIcon {Icon=System.Drawing.SystemIcons.Application,Text="Codex Dashboard",Visible=true};
  var menu=new System.Windows.Forms.ContextMenuStrip();menu.Items.Add("Mostra widget",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>{w.Show();w.Activate();})));menu.Items.Add("Aggiorna",null,(s,e)=>meter.Refresh());menu.Items.Add("Esci",null,(s,e)=>w.Dispatcher.Invoke(new Action(()=>w.Close())));tray.ContextMenuStrip=menu;tray.DoubleClick+=(s,e)=>{w.Show();w.Activate();};
  var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(250)};timer.Tick+=(s,e)=>{
   if(close.WaitOne(0)){w.Close();return;}if(show.WaitOne(0)){w.Show();w.Activate();}
   var data=meter.State;
   foreach(var prefix in new[]{"Primary","Secondary"}) {
    var q=prefix=="Primary"?data.Primary:data.Secondary;var value=(TextBlock)ui[prefix+"Value"];var bar=(ProgressBar)ui[prefix+"Bar"];
    value.Text=q==null?"—":q.Remaining.ToString("0.#")+"%";bar.Value=q==null?0:q.Remaining;
    ((TextBlock)ui[prefix+"Label"]).Text=q==null?"NON DISP.":q.Label;
    bar.Foreground=Color(q!=null&&q.Remaining<=15?"#FF7F50":prefix=="Primary"?"#FCEE0A":"#00E6F6");
    ((StackPanel)ui[prefix+"Card"]).ToolTip=q==null?"Dato non disponibile":value.Text+" disponibile · "+q.ResetText;
   }
   var status=(TextBlock)ui["Status"];status.Text=data.Online?"●":"!";status.Foreground=Color(data.Online?"#00E6F6":"#FF7F50");
   status.ToolTip=data.Online?"Aggiornato alle "+data.Updated.Value.ToLocalTime().ToString("HH:mm:ss")+" · "+data.Plan:"Dati non aggiornati · "+(data.Error??"In attesa di aggiornamento");
  };timer.Start();
  w.Closed+=(s,e)=>{timer.Stop();tray.Dispose();menu.Dispose();try{Directory.CreateDirectory(Path.GetDirectoryName(settings));File.WriteAllText(settings,Json.Stringify(new {left=w.Left,top=w.Top}));}catch{}};
  return w;
 }
}
static class Plugin {
 static readonly SemaphoreSlim sendLock=new SemaphoreSlim(1,1);
 static async Task Send(ClientWebSocket ws,object value,CancellationToken ct) {var bytes=Encoding.UTF8.GetBytes(Json.Stringify(value));await sendLock.WaitAsync(ct);try{await ws.SendAsync(new ArraySegment<byte>(bytes),WebSocketMessageType.Text,true,ct);}finally{sendLock.Release();}}
 static string Image(Quota q,bool online,bool launcher) {
  string color=q!=null&&q.Remaining<=15?"#FF7F50":q!=null&&q.Minutes==10080?"#00E6F6":"#FCEE0A";
  string label=launcher?"CODEX DASH":q==null?"CODEX":q.Label;
  string value=launcher?"APRI":q==null?"—":q.Remaining.ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)+"%";
  string bottom=launcher?"WIDGET":!online?"OFFLINE":q==null?"NON DISP.":"DISPONIBILE";
  string svg=String.Format(System.Globalization.CultureInfo.InvariantCulture,"<svg xmlns='http://www.w3.org/2000/svg' width='144' height='144'><rect width='144' height='144' rx='12' fill='#0B1017'/><path d='M0 3H132L141 12' fill='none' stroke='{0}' stroke-width='5'/><text x='72' y='30' text-anchor='middle' fill='{0}' font-family='Consolas,monospace' font-size='15'>{1}</text><text x='72' y='84' text-anchor='middle' fill='{0}' font-family='Consolas,monospace' font-size='38' font-weight='bold'>{2}</text><rect x='12' y='99' width='120' height='4' fill='#26313C'/><rect x='12' y='99' width='{3}' height='4' fill='{0}'/><text x='72' y='125' text-anchor='middle' fill='{4}' font-family='Consolas,monospace' font-size='12'>{5}</text></svg>",color,label,value,q==null?0:q.Remaining*1.2,online||launcher?"#96A3AD":"#FF7F50",bottom);
  return "data:image/svg+xml;base64,"+Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
 }
 public static async Task Run(string[] args) {
  var options=new Dictionary<string,string>();for(int i=0;i+1<args.Length;i+=2)options[args[i]]=args[i+1];
  int port=int.Parse(options["-port"]);if(port<1||port>65535)throw new Exception("Invalid Stream Deck port");
  using(var ws=new ClientWebSocket())using(var ct=new CancellationTokenSource())using(var meter=new Meter()) {
   await ws.ConnectAsync(new Uri("ws://127.0.0.1:"+port),ct.Token);
   await Send(ws,new { @event=options["-registerEvent"],uuid=options["-pluginUUID"]},ct.Token);
   var actions=new Dictionary<string,string>();var actionGate=new object();
   var update=Task.Run(async delegate {try{while(ws.State==WebSocketState.Open&&!ct.IsCancellationRequested){KeyValuePair<string,string>[] current;lock(actionGate)current=actions.ToArray();var data=meter.State;foreach(var a in current){bool launch=a.Value.EndsWith(".open");var q=a.Value.EndsWith(".weekly")?new[]{data.Primary,data.Secondary}.FirstOrDefault(x=>x!=null&&x.Minutes==10080):new[]{data.Primary,data.Secondary}.FirstOrDefault(x=>x!=null&&x.Minutes==300);await Send(ws,new {@event="setImage",context=a.Key,payload=new {image=Image(q,data.Online,launch),target=0}},ct.Token);await Send(ws,new {@event="setTitle",context=a.Key,payload=new {title="",target=0}},ct.Token);}await Task.Delay(2000,ct.Token);}}catch{ct.Cancel();ws.Abort();}});
   try{var buffer=new byte[8192];while(ws.State==WebSocketState.Open){using(var message=new MemoryStream()){WebSocketReceiveResult received;do{received=await ws.ReceiveAsync(new ArraySegment<byte>(buffer),ct.Token);if(received.MessageType==WebSocketMessageType.Close)return;message.Write(buffer,0,received.Count);if(message.Length>1048576)throw new Exception("Stream Deck message too large");}while(!received.EndOfMessage);
    var m=Json.Parse(Encoding.UTF8.GetString(message.ToArray()));var ev=Json.Str(m,"event");var context=Json.Str(m,"context");var action=Json.Str(m,"action");
    if(ev=="willAppear") {lock(actionGate)actions[context]=action;}
    else if(ev=="willDisappear"){lock(actionGate)actions.Remove(context);}
    else if(ev=="keyDown"){if(action.EndsWith(".open")){Process.Start(new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location,"--widget"){UseShellExecute=false,CreateNoWindow=true});}else meter.Refresh();}
   }}}finally{ct.Cancel();ws.Abort();try{update.GetAwaiter().GetResult();}catch{}}
  }
 }
}
}
