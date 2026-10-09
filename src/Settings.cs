using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace CodexDashboard {
static class Artifacts {
 public static string Path(string name) {
  string directory=Environment.GetEnvironmentVariable("CODEXDASHBOARD_ARTIFACT_DIR")??System.IO.Path.Combine(SettingsStore.DirectoryPath,"diagnostics");
  Directory.CreateDirectory(directory);return System.IO.Path.Combine(directory,name);
 }
}
public class UserSettings {
 public double Scale=1,Opacity=.82;
 public bool Topmost=true,Notifications=false,ShowUsed=false;
 public int PollSeconds=60,LowThreshold=15;
 public int? ShortLowThreshold,WeeklyLowThreshold;
 public int ThresholdFor(int minutes){return minutes==10080?(WeeklyLowThreshold??LowThreshold):(ShortLowThreshold??LowThreshold);}
 public string Startup="manual",Language="it",CloseMode="manual";
 public string WidgetPosition="custom",MonitorDevice="";
 public int EdgeMargin=24;
 public void Normalize() {
  Scale=double.IsNaN(Scale)||double.IsInfinity(Scale)?1:Math.Max(.8,Math.Min(2,Scale));
  Opacity=double.IsNaN(Opacity)||double.IsInfinity(Opacity)?.82:Math.Max(.4,Math.Min(1,Opacity));
  PollSeconds=Math.Max(30,Math.Min(300,PollSeconds));LowThreshold=Math.Max(1,Math.Min(50,LowThreshold));ShortLowThreshold=Math.Max(1,Math.Min(50,ShortLowThreshold??LowThreshold));WeeklyLowThreshold=Math.Max(1,Math.Min(50,WeeklyLowThreshold??LowThreshold));
  if(Startup!="manual"&&Startup!="windows"&&Startup!="codex")Startup="manual";
  if(Language!="en")Language="it";if(CloseMode!="codex")CloseMode="manual";
  if(Array.IndexOf(Placement.Positions,WidgetPosition)<0)WidgetPosition="custom";
  MonitorDevice=MonitorDevice??"";EdgeMargin=Math.Max(0,Math.Min(120,EdgeMargin));
 }
}
static class Placement {
 public static readonly string[] Positions={"custom","top-left","top-center","top-right","middle-left","center","middle-right","bottom-left","bottom-center","bottom-right"};
 public static Point Calculate(Rect area,double width,double height,string position,int margin){
  double gap=Math.Max(0,margin),left=area.Left+Math.Min(gap,Math.Max(0,area.Width-width)/2),right=area.Right-width-Math.Min(gap,Math.Max(0,area.Width-width)/2);
  double top=area.Top+Math.Min(gap,Math.Max(0,area.Height-height)/2),bottom=area.Bottom-height-Math.Min(gap,Math.Max(0,area.Height-height)/2);
  double x=position.EndsWith("left")?left:position.EndsWith("right")?right:area.Left+(area.Width-width)/2;
  double y=position.StartsWith("top")?top:position.StartsWith("bottom")?bottom:area.Top+(area.Height-height)/2;
  return new Point(Math.Max(area.Left,Math.Min(x,Math.Max(area.Left,area.Right-width))),Math.Max(area.Top,Math.Min(y,Math.Max(area.Top,area.Bottom-height))));
 }
 public static void Apply(Window window,UserSettings prefs){
  var screens=System.Windows.Forms.Screen.AllScreens;var screen=Array.Find(screens,x=>x.DeviceName==prefs.MonitorDevice)??System.Windows.Forms.Screen.PrimaryScreen;
  var source=PresentationSource.FromVisual(window);var transform=source!=null&&source.CompositionTarget!=null?source.CompositionTarget.TransformFromDevice:Matrix.Identity;
  var bounds=screen.WorkingArea;var a=transform.Transform(new Point(bounds.Left,bounds.Top));var b=transform.Transform(new Point(bounds.Right,bounds.Bottom));
  var point=Calculate(new Rect(a,b),window.Width,window.Height,prefs.WidgetPosition,prefs.EdgeMargin);window.Left=point.X;window.Top=point.Y;
 }
 public static string ScreenSignature(){return string.Join("|",Array.ConvertAll(System.Windows.Forms.Screen.AllScreens,s=>s.DeviceName+s.WorkingArea.ToString()));}
}
static class SettingsStore {
 static readonly object Gate=new object();static UserSettings cached=new UserSettings();static DateTime stamp=DateTime.MinValue;
 public static string DirectoryPath {get {return Environment.GetEnvironmentVariable("CODEXDASHBOARD_DATA_DIR")??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CodexDashboard");}}
 public static string FilePath {get {return Path.Combine(DirectoryPath,"settings.json");}}
 public static UserSettings Current {get {lock(Gate){try{var changed=File.GetLastWriteTimeUtc(FilePath);if(changed!=stamp){var value=new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<UserSettings>(File.ReadAllText(FilePath));if(value==null)throw new Exception();value.Normalize();cached=value;stamp=changed;}}catch{}return cached;}}}
 public static void Save(UserSettings value,bool updateStartup=true) {
  value.Normalize();Directory.CreateDirectory(DirectoryPath);
  string previous=null;
  using(var key=updateStartup?Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run"):null) {
   if(key!=null){previous=key.GetValue("CodexDashboard") as string;SetStartup(key,value);}
   try{lock(Gate){var tmp=FilePath+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllText(tmp,Json.Stringify(value));if(File.Exists(FilePath))File.Replace(tmp,FilePath,null);else File.Move(tmp,FilePath);}finally{if(File.Exists(tmp))File.Delete(tmp);}cached=value;stamp=File.GetLastWriteTimeUtc(FilePath);}}
   catch{if(key!=null){if(previous==null)key.DeleteValue("CodexDashboard",false);else key.SetValue("CodexDashboard",previous);}throw;}
  }
 }
 static void SetStartup(RegistryKey key,UserSettings value) {
  if(value.Startup=="manual")key.DeleteValue("CodexDashboard",false);
  else key.SetValue("CodexDashboard",StartupCommand(value,System.Reflection.Assembly.GetExecutingAssembly().Location));
 }
 public static string StartupCommand(UserSettings value,string executable){return value.Startup=="manual"?null:"\""+executable+"\""+(value.Startup=="codex"?" --watch-codex":"");}
 public static bool IsCodexDesktopPath(string path){return !string.IsNullOrEmpty(path)&&(path.IndexOf("\\WindowsApps\\OpenAI.Codex_",StringComparison.OrdinalIgnoreCase)>=0||path.EndsWith("\\Codex\\Codex.exe",StringComparison.OrdinalIgnoreCase)||path.EndsWith("\\Codex\\app\\ChatGPT.exe",StringComparison.OrdinalIgnoreCase));}
 public static bool IsCodexDesktopWindow(string path,IntPtr window){return window!=IntPtr.Zero&&IsCodexDesktopPath(path);}
 public static bool CodexIsOpen() {
  foreach(var name in new[]{"Codex","ChatGPT"})foreach(var p in System.Diagnostics.Process.GetProcessesByName(name))using(p)try{var path=p.MainModule.FileName;if(IsCodexDesktopWindow(path,p.MainWindowHandle))return true;}catch{}
  return false;
 }
}
sealed class LowQuotaAlert {
 readonly System.Collections.Generic.Dictionary<int,bool> low=new System.Collections.Generic.Dictionary<int,bool>();
 readonly System.Collections.Generic.Dictionary<int,int> thresholds=new System.Collections.Generic.Dictionary<int,int>();bool enabled;
 public string Check(Snapshot data,UserSettings settings) {
  if(enabled!=settings.Notifications){low.Clear();thresholds.Clear();enabled=settings.Notifications;}
  if(!data.Online||!settings.Notifications)return null;
  string message=null;
  foreach(var q in new[]{data.Primary,data.Secondary})if(q!=null){int threshold=settings.ThresholdFor(q.Minutes);if(!thresholds.ContainsKey(q.Minutes)||thresholds[q.Minutes]!=threshold){low.Remove(q.Minutes);thresholds[q.Minutes]=threshold;}bool active=q.Remaining<=threshold;bool previous=low.ContainsKey(q.Minutes)&&low[q.Minutes];if(active&&!previous)message=(message==null?"":message+" · ")+L.T(q.Label)+": "+q.Remaining.ToString("0.#")+L.T("% disponibile");low[q.Minutes]=active;}
  return message;
 }
}
static class Configurator {
 static Window active;
 static Brush Color(string hex){return (Brush)new BrushConverter().ConvertFromString(hex);}
 static Slider Slider(double min,double max,double step){return new Slider{Minimum=min,Maximum=max,TickFrequency=step,IsSnapToTickEnabled=true,Width=190,VerticalAlignment=VerticalAlignment.Center};}
 public static Window Create(Window owner,Action resetPosition,bool updateStartup=true) {
  var w=new Window{Title="Codex Dashboard · Impostazioni",Width=500,SizeToContent=SizeToContent.Height,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,Background=Color(Theme.background),Foreground=Color(Theme.cyan),FontFamily=new FontFamily(Theme.font),FontSize=14,WindowStartupLocation=WindowStartupLocation.CenterScreen};
  string styles=@"<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
   <Style TargetType='ComboBoxItem'><Setter Property='Foreground' Value='@cyan'/><Setter Property='Background' Value='@background'/><Setter Property='Padding' Value='8,5'/><Style.Triggers><Trigger Property='IsHighlighted' Value='True'><Setter Property='Background' Value='@hover'/><Setter Property='Foreground' Value='@yellow'/></Trigger></Style.Triggers></Style>
   <Style TargetType='ComboBox'><Setter Property='Foreground' Value='@yellow'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ComboBox'><Grid><ToggleButton Focusable='False' IsChecked='{Binding IsDropDownOpen,RelativeSource={RelativeSource TemplatedParent},Mode=TwoWay}'><ToggleButton.Template><ControlTemplate TargetType='ToggleButton'><Border BorderBrush='@cyan' BorderThickness='1' Background='@background' Padding='8,5'><TextBlock Text='▾' Foreground='@yellow' HorizontalAlignment='Right'/></Border></ControlTemplate></ToggleButton.Template></ToggleButton><ContentPresenter Margin='8,5,24,5' VerticalAlignment='Center' IsHitTestVisible='False' Content='{TemplateBinding SelectionBoxItem}'/><Popup Name='PART_Popup' Placement='Bottom' IsOpen='{TemplateBinding IsDropDownOpen}' AllowsTransparency='True'><Border Background='@background' BorderBrush='@cyan' BorderThickness='1' MinWidth='{Binding ActualWidth,RelativeSource={RelativeSource TemplatedParent}}'><ScrollViewer><ItemsPresenter/></ScrollViewer></Border></Popup></Grid></ControlTemplate></Setter.Value></Setter></Style>
  </ResourceDictionary>";
  styles=styles.Replace("@yellow",Theme.yellow).Replace("@cyan",Theme.cyan).Replace("@background",Theme.background).Replace("@hover",Theme.hover);
  w.Resources=(ResourceDictionary)System.Windows.Markup.XamlReader.Parse(styles);
  if(owner!=null)w.Owner=owner;
  var root=new StackPanel{Margin=new Thickness(22,18,22,20)};w.MaxHeight=Math.Max(300,SystemParameters.WorkArea.Height-24);w.Content=new Border{BorderBrush=Color(Theme.yellow),BorderThickness=new Thickness(1),Child=new ScrollViewer{Content=root,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled}};
  System.Windows.Input.MouseButtonEventHandler dragWindow=(s,e)=>{if(e.ChangedButton==System.Windows.Input.MouseButton.Left&&e.ButtonState==System.Windows.Input.MouseButtonState.Pressed){w.DragMove();e.Handled=true;}};
  var headerLine=new System.Windows.Shapes.Rectangle{Height=4,Fill=Color(Theme.yellow),Margin=new Thickness(0,0,0,14),Cursor=System.Windows.Input.Cursors.SizeAll};headerLine.MouseLeftButtonDown+=dragWindow;root.Children.Add(headerLine);
  var title=new TextBlock{Text="// CODEX DASHBOARD",FontSize=24,FontWeight=FontWeights.Bold,Foreground=Color(Theme.yellow),VerticalAlignment=VerticalAlignment.Center};
  var brandHeader=new StackPanel{Orientation=Orientation.Horizontal,Background=Brushes.Transparent,Cursor=System.Windows.Input.Cursors.SizeAll};brandHeader.MouseLeftButtonDown+=dragWindow;
  using(var stream=typeof(Configurator).Assembly.GetManifestResourceStream("DashboardLogo.png"))brandHeader.Children.Add(new Image{Source=System.Windows.Media.Imaging.BitmapFrame.Create(stream,System.Windows.Media.Imaging.BitmapCreateOptions.None,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad),Width=40,Height=40,Margin=new Thickness(0,0,12,0)});
  brandHeader.Children.Add(title);root.Children.Add(brandHeader);
  var configurationHeader=new TextBlock{Text="CONFIGURAZIONE",Margin=new Thickness(0,3,0,18),Background=Brushes.Transparent,Cursor=System.Windows.Input.Cursors.SizeAll};configurationHeader.MouseLeftButtonDown+=dragWindow;root.Children.Add(configurationHeader);
  var scale=Slider(.8,2,.1);scale.Name="WidgetScale";var opacity=Slider(.4,1,.05);
  var topmost=new CheckBox{Content="Sempre in primo piano",Foreground=Color(Theme.cyan)};
  var notifications=new CheckBox{Content="Notifica quando la quota scende sotto la soglia",Foreground=Color(Theme.cyan)};
  var startup=new ComboBox{Width=215,ItemsSource=new[]{"Solo avvio manuale","Con Windows","Quando apri Codex"}};
  var closeMode=new ComboBox{Name="CloseMode",Width=215,ItemsSource=new[]{"Solo manualmente","Quando chiudi Codex"}};
  var interval=new ComboBox{Width=215,ItemsSource=new[]{"30 secondi","60 secondi","120 secondi","300 secondi"}};
  var threshold=Slider(1,50,1);threshold.Name="ShortThreshold";var weeklyThreshold=Slider(1,50,1);weeklyThreshold.Name="WeeklyThreshold";var used=new ComboBox{Width=215,ItemsSource=new[]{"Quota disponibile","Quota consumata"}};
  var scaleText=new TextBlock{Width=48,Margin=new Thickness(8,0,0,0)};var opacityText=new TextBlock{Width=48,Margin=new Thickness(8,0,0,0)};var weeklyThresholdText=new TextBlock{Width=48,Margin=new Thickness(8,0,0,0)};var thresholdText=new TextBlock{Width=48,Margin=new Thickness(8,0,0,0)};
  Section(root,"ASPETTO");AddRow(root,"Dimensione widget",WithValue(scale,scaleText));AddRow(root,"Opacità a riposo",WithValue(opacity,opacityText));AddRow(root,"Posizione e visibilità",topmost);
  var language=new ComboBox{Width=215,ItemsSource=new[]{"Italiano","English"},Name="SettingsLanguage"};AddRow(root,"Lingua",language);
  var screens=System.Windows.Forms.Screen.AllScreens;
  var monitor=new ComboBox{Width=215};foreach(var screen in screens)monitor.Items.Add(screen.DeviceName.Replace("\\\\.\\","")+(screen.Primary?" ★":""));
  var placement=new ComboBox{Name="WidgetPlacement",Width=215,ItemsSource=new[]{"Personalizzata (trascina)","In alto a sinistra","In alto al centro","In alto a destra","Al centro a sinistra","Centro schermo","Al centro a destra","In basso a sinistra","In basso al centro","In basso a destra"}};
  var margin=Slider(0,120,1);var marginText=new TextBlock{Width=48,Margin=new Thickness(8,0,0,0)};
  Section(root,"POSIZIONE DEL WIDGET");AddRow(root,"Monitor",monitor);AddRow(root,"Posizione del widget",placement);AddRow(root,"Distanza dai bordi",WithValue(margin,marginText));
  var position=Button("Centra widget");position.HorizontalAlignment=HorizontalAlignment.Left;root.Children.Add(position);
  var positioningHelp=new TextBlock{Text="La posizione si applica con Applica o Salva.\nTrascinando il widget passi alla posizione personalizzata.",FontSize=12,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,8,0,0)};root.Children.Add(positioningHelp);
  placement.SelectionChanged+=(s,e)=>{bool preset=placement.SelectedIndex>0;monitor.IsEnabled=preset;margin.IsEnabled=preset;};margin.ValueChanged+=(s,e)=>marginText.Text=margin.Value.ToString("0")+" px";
  Section(root,"QUOTE E AGGIORNAMENTI");AddRow(root,"Avvio automatico",startup);AddRow(root,"Chiusura widget",closeMode);AddRow(root,"Aggiorna dati ogni",interval);AddRow(root,"Percentuale mostrata",used);Section(root,"SOGLIE E NOTIFICHE");AddRow(root,"Quota 5 ore",WithValue(threshold,thresholdText),Theme.yellow);AddRow(root,"Quota settimana",WithValue(weeklyThreshold,weeklyThresholdText),Theme.cyan);root.Children.Add(new Border{Child=notifications,Margin=new Thickness(0,10,0,0)});
  root.Children.Add(new TextBlock{Text="Le quote basse restano rosse. Le notifiche sono facoltative.\nStream Deck riceve le impostazioni entro pochi secondi.",FontSize=12,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,14,0,12)});
  var error=new TextBlock{Foreground=Color(Theme.alert),TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,8)};root.Children.Add(error);
  var buttons=new WrapPanel{HorizontalAlignment=HorizontalAlignment.Right};root.Children.Add(buttons);
  var reset=Button("Ripristina");reset.Margin=new Thickness(0,0,24,0);var cancel=Button("Annulla");var save=Button("Salva");var apply=Button("Applica");reset.Name="ResetSettings";cancel.Name="CancelSettings";save.Name="SaveSettings";apply.Name="ApplySettings";foreach(var b in new[]{reset,cancel,apply,save})buttons.Children.Add(b);
  int[] polls={30,60,120,300};
  Action<UserSettings> fill=s=>{scale.Value=s.Scale;opacity.Value=s.Opacity;topmost.IsChecked=s.Topmost;notifications.IsChecked=s.Notifications;closeMode.SelectedIndex=s.CloseMode=="codex"?1:0;startup.SelectedIndex=s.Startup=="windows"?1:s.Startup=="codex"?2:0;interval.SelectedIndex=Array.IndexOf(polls,s.PollSeconds);if(interval.SelectedIndex<0)interval.SelectedIndex=1;threshold.Value=s.ThresholdFor(300);weeklyThreshold.Value=s.ThresholdFor(10080);used.SelectedIndex=s.ShowUsed?1:0;language.SelectedIndex=s.Language=="en"?1:0;placement.SelectedIndex=Array.IndexOf(Placement.Positions,s.WidgetPosition);monitor.SelectedIndex=Array.FindIndex(screens,x=>x.DeviceName==s.MonitorDevice);if(monitor.SelectedIndex<0)monitor.SelectedIndex=Array.FindIndex(screens,x=>x.Primary);margin.Value=s.EdgeMargin;marginText.Text=s.EdgeMargin+" px";};
  scale.ValueChanged+=(s,e)=>scaleText.Text=(scale.Value*100).ToString("0")+"%";opacity.ValueChanged+=(s,e)=>opacityText.Text=(opacity.Value*100).ToString("0")+"%";weeklyThreshold.ValueChanged+=(s,e)=>weeklyThresholdText.Text=weeklyThreshold.Value.ToString("0")+"%";threshold.ValueChanged+=(s,e)=>thresholdText.Text=threshold.Value.ToString("0")+"%";
  fill(SettingsStore.Current);scaleText.Text=(scale.Value*100).ToString("0")+"%";opacityText.Text=(opacity.Value*100).ToString("0")+"%";thresholdText.Text=threshold.Value.ToString("0")+"%";
  reset.Click+=(s,e)=>{fill(new UserSettings());error.Tag=null;error.Text=L.T("Valori predefiniti caricati. Premi Salva per applicarli.");};
  position.Click+=(s,e)=>placement.SelectedIndex=5;cancel.Click+=(s,e)=>w.Close();
  Action<bool> commit=close=>{try{SettingsStore.Save(new UserSettings{Scale=scale.Value,Opacity=opacity.Value,Topmost=topmost.IsChecked==true,Notifications=notifications.IsChecked==true,Startup=new[]{"manual","windows","codex"}[startup.SelectedIndex],CloseMode=closeMode.SelectedIndex==1?"codex":"manual",PollSeconds=polls[interval.SelectedIndex],LowThreshold=SettingsStore.Current.LowThreshold,ShortLowThreshold=(int)threshold.Value,WeeklyLowThreshold=(int)weeklyThreshold.Value,ShowUsed=used.SelectedIndex==1,Language=language.SelectedIndex==1?"en":"it",WidgetPosition=Placement.Positions[Math.Max(0,placement.SelectedIndex)],MonitorDevice=screens[Math.Max(0,monitor.SelectedIndex)].DeviceName,EdgeMargin=(int)margin.Value},updateStartup);if(close)w.Close();else{L.Apply(root,L.IsEnglish);w.Title=L.T("Codex Dashboard · Impostazioni");error.Tag=null;error.Text=L.T("Impostazioni applicate.");}}catch(Exception ex){error.Text=L.T("Salvataggio non riuscito: ")+ex.Message;}};
  save.Click+=(s,e)=>commit(true);apply.Click+=(s,e)=>commit(false);
  L.Apply(root,L.IsEnglish);w.Title=L.T(w.Title);
  return w;
 }
 public static void Show(Window owner,Action resetPosition){if(active!=null){active.Activate();return;}active=Create(owner,resetPosition);active.Closed+=(s,e)=>active=null;active.Show();}
 public static StackPanel Panel(Window window){return (StackPanel)((ScrollViewer)((Border)window.Content).Child).Content;}
 public static T Control<T>(Window window,string name) where T:FrameworkElement {return FindControl<T>(Panel(window),name);}
 static T FindControl<T>(DependencyObject parent,string name) where T:FrameworkElement {var match=parent as T;if(match!=null&&match.Name==name)return match;foreach(var child in LogicalTreeHelper.GetChildren(parent)){var element=child as DependencyObject;if(element==null)continue;var found=FindControl<T>(element,name);if(found!=null)return found;}return null;}
 static void Section(Panel root,string text){root.Children.Add(new TextBlock{Text=text,Foreground=Color(Theme.yellow),FontWeight=FontWeights.Bold,FontSize=13,Margin=new Thickness(0,14,0,6)});}
 static UIElement WithValue(Slider slider,TextBlock label){var panel=new StackPanel{Orientation=Orientation.Horizontal};panel.Children.Add(slider);panel.Children.Add(label);return panel;}
 static void AddRow(Panel root,string text,UIElement control,string accent=null){var row=new Grid{Margin=new Thickness(0,5,0,5)};row.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(185)});row.ColumnDefinitions.Add(new ColumnDefinition());row.Children.Add(new TextBlock{Text=text,VerticalAlignment=VerticalAlignment.Center,Foreground=Color(accent??Theme.cyan)});Grid.SetColumn(control,1);row.Children.Add(control);root.Children.Add(row);}
 static Button Button(string text){return new Button{Content=text,Background=Color(Theme.hover),Foreground=Color(Theme.yellow),BorderBrush=Color(Theme.yellow),Padding=new Thickness(11,7,11,7),Margin=new Thickness(0,0,8,0)};}
}
}
