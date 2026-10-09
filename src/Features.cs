using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;

[assembly:System.Reflection.AssemblyVersion("2.0.7.0")]
[assembly:System.Reflection.AssemblyFileVersion("2.0.7.0")]
[assembly:System.Reflection.AssemblyProduct("Codex Dashboard")]

namespace CodexDashboard {
static class AppVersion {public const string Value="2.0.7";}
static class L {
 static readonly Dictionary<string,string> English=new Dictionary<string,string> {
  {"Reset disponibili: ","Available resets: "},{"non disponibili","unavailable"},{" (ultimo dato ricevuto)"," (last received data)"},{"Prossimo reset in scadenza: ","Next reset expires: "},{"Nascondi widget · il monitor resta attivo","Hide widget · monitor stays active"},{"Chiudi programma","Close app"},{"Monitor Codex attivo · X nasconde il widget; Esci termina il monitor.","Codex monitor active · X hides the widget; Exit stops the monitor."},{"Avvio con Windows · X chiude il programma.","Windows startup · X closes the app."},{"Avvio manuale · X chiude il programma.","Manual startup · X closes the app."},{"Avvio Windows registrato.","Windows startup registered."},{"Avvio Windows non registrato: premi Applica per ripristinarlo.","Windows startup not registered: press Apply to restore it."},{"Verifica avvio Windows non disponibile.","Windows startup check unavailable."},{"Dati raccolti: ","Recorded data: "},{" rilevazioni\nIl grafico usa il periodo con dati disponibili, non riempie le ore mancanti."," samples\nThe chart uses the recorded time span; missing hours are not filled in."},{"% disponibile · ultimo dato","% available · latest sample"},{"Nessun dato per questa quota nel periodo selezionato.","No data for this quota in the selected period."},{"Nessun dato nel periodo selezionato. Lo storico si raccoglie mentre il programma è attivo.","No data in the selected period. History is recorded while the app is running."},{"Le linee mostrano la quota ancora disponibile: 100% = tutta, 0% = esaurita.\nUna discesa indica consumo; una salita può indicare un rinnovo o un reset.\nPassa il mouse sui punti per leggere ora e percentuale.","Lines show the remaining quota: 100% = full, 0% = exhausted.\nA decrease means usage; an increase may mean a renewal or a reset.\nHover over a point to see its time and percentage."},{"Periodo selezionato: ","Selected period: "},{"Avvio gestito da Windows (entro 10 secondi). La chiusura di Codex termina completamente il programma.","Windows manages startup (within 10 seconds). Closing Codex fully exits the app."},{"Impostazioni","Settings"},{"Impostazioni…","Settings…"},{"Codex Dashboard · Impostazioni","Codex Dashboard · Settings"},{"CONFIGURAZIONE","SETTINGS"},{"ASPETTO","APPEARANCE"},{"POSIZIONE DEL WIDGET","WIDGET POSITION"},{"QUOTE E AGGIORNAMENTI","QUOTAS AND UPDATES"},{"Monitor","Monitor"},{"Percentuale mostrata","Displayed percentage"},
  {"Schermo del widget","Widget screen"},{"Posizione del widget","Widget position"},{"Distanza dai bordi","Edge spacing"},
  {"Personalizzata (trascina)","Custom (drag widget)"},{"In alto a sinistra","Top left"},{"In alto al centro","Top center"},{"In alto a destra","Top right"},{"Al centro a sinistra","Middle left"},{"Centro schermo","Screen center"},{"Al centro a destra","Middle right"},{"In basso a sinistra","Bottom left"},{"In basso al centro","Bottom center"},{"In basso a destra","Bottom right"},
  {"La posizione si applica con Applica o Salva.\nTrascinando il widget passi alla posizione personalizzata.","Apply or Save to move the widget.\nDragging the widget switches to custom positioning."},
  {"Dimensione widget","Widget size"},{"Opacità a riposo","Idle opacity"},{"Posizione e visibilità","Position and visibility"},{"Sempre in primo piano","Always on top"},
  {"Chiusura widget","Widget closing"},{"Solo manualmente","Manually only"},{"Quando chiudi Codex","When Codex closes"},{"Avvio automatico","Startup"},{"Solo avvio manuale","Manual startup only"},{"Con Windows","With Windows"},{"Quando apri Codex","When Codex opens"},
  {"Aggiorna dati ogni","Refresh data every"},{"30 secondi","30 seconds"},{"60 secondi","60 seconds"},{"120 secondi","120 seconds"},{"300 secondi","300 seconds"},
  {"SOGLIE E NOTIFICHE","THRESHOLDS AND NOTIFICATIONS"},{"Quota 5 ore","5-hour quota"},{"Quota settimana","Weekly quota"},{"Soglia quota bassa","Low quota threshold"},{"Notifica quando la quota scende sotto la soglia","Notify when quota falls below the threshold"},{"Widget e Stream Deck","Widget and Stream Deck"},
  {"Quota disponibile","Available quota"},{"Quota consumata","Used quota"},{"Ripristina","Reset"},{"Centra widget","Center widget"},{"Annulla","Cancel"},{"Salva","Save"},{"Applica","Apply"},{"Lingua","Language"},
  {"Mostra widget","Show widget"},{"Aggiorna","Refresh"},{"Esci","Exit"},{"Chiudi","Close"},{"Aggiorna ora","Refresh now"},{"Informazioni e diagnostica","About and diagnostics"},{"Storico quote","Quota history"},
  {"NON DISP.","N/A"},{"SETTIMANA","WEEK"},{"SETT.","WEEK"},{"5 ORE","5 HOURS"},{"APRI","OPEN"},{"ORE","HOURS"},{"Si rinnova ","Renews "},{"Rinnovo non disponibile","Reset time unavailable"},
  {"Dato non disponibile","Data unavailable"},{" disponibile · "," available · "},{" consumata · "," used · "},{"% disponibile","% available"},{"Codex Dashboard · Quota bassa","Codex Dashboard · Low quota"},
  {"Connessione a Codex…","Connecting to Codex…"},{"Codex non trovato. Installa Codex e accedi al tuo account.","Codex not found. Install Codex and sign in to your account."},
  {"Codex non risponde · riconnessione automatica","Codex is not responding · reconnecting automatically"},{"Connessione rifiutata da Codex.","Codex refused the connection."},
  {"Limiti non disponibili per questo account.","Quotas are unavailable for this account."},{"Limiti non disponibili. Verifica l'accesso a Codex.","Quotas unavailable. Check your Codex sign-in."},
  {"Le quote basse restano rosse. Le notifiche sono facoltative.\nStream Deck riceve le impostazioni entro pochi secondi.","Low quotas stay red. Notifications are optional.\nStream Deck receives settings within a few seconds."},
  {"Valori predefiniti caricati. Premi Salva per applicarli.","Defaults loaded. Press Apply or Save to confirm."},{"Salvataggio non riuscito: ","Could not save: "},{"Impostazioni applicate.","Settings applied."},
  {"Aggiornato alle ","Updated at "},{" · Quota consumata"," · Used quota"},{" · Quota disponibile"," · Available quota"},{"Dati non aggiornati · ","Stale data · "},{"In attesa di aggiornamento","Waiting for an update"},
  {"Trascina per spostare · dati ogni ","Drag to move · refresh every "},{" s · tasto destro per impostazioni"," s · right-click for settings"},
  {"Esporta diagnostica","Export diagnostics"},{"Installa aggiornamento…","Install update…"},{"Ultime 24 ore","Last 24 hours"},{"Ultimi 7 giorni","Last 7 days"},{"Esporta CSV","Export CSV"},
  {"Nessuno storico disponibile. I campioni saranno raccolti durante l'utilizzo.","No history yet. Samples will be collected while the app is running."},
  {"Disponibilità residua (%) · aumenti = rinnovi della quota","Remaining quota (%) · increases indicate quota resets"},{"Lo storico contiene quote, non token o costi.","History contains quotas, not tokens or costs."}
 };
 public static bool IsEnglish {get{return SettingsStore.Current.Language=="en";}}
 public static string T(string value){return T(value,IsEnglish);}
 public static string T(string value,bool english){string result;return english&&English.TryGetValue(value,out result)?result:value;}
 public static void Apply(DependencyObject root,bool english) {
  var text=root as TextBlock;if(text!=null&&(text.Tag is string||English.ContainsKey(text.Text))){if(!(text.Tag is string))text.Tag=text.Text;text.Text=T((string)text.Tag,english);}
  var combo=root as ComboBox;if(combo!=null&&combo.ItemsSource is string[]){if(!(combo.Tag is string[]))combo.Tag=combo.ItemsSource;int selected=combo.SelectedIndex;combo.ItemsSource=((string[])combo.Tag).Select(x=>T(x,english)).ToArray();combo.SelectedIndex=selected;}
  var content=root as ContentControl;if(content!=null&&content.Content is string){if(!(content.Tag is string))content.Tag=content.Content;content.Content=T((string)content.Tag,english);}
  foreach(var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())Apply(child,english);
 }
}
public class HistorySample {public string At;public double? Short,Weekly;}
static class QuotaHistory {
 public static string FilePath {get{return System.IO.Path.Combine(SettingsStore.DirectoryPath,"history.jsonl");}}
 public static void Record(Snapshot snapshot) {
  if(!snapshot.Online)return;
  using(var mutex=new Mutex(false,"Local\\CodexDashboardHistory")){bool owned=false;try{try{owned=mutex.WaitOne(100);}catch(AbandonedMutexException){owned=true;}if(!owned)return;
   Directory.CreateDirectory(SettingsStore.DirectoryPath);var rows=Load();var at=DateTime.UtcNow;
   if(rows.Count>0&& (at-DateTime.Parse(rows[rows.Count-1].At,null,System.Globalization.DateTimeStyles.RoundtripKind)).TotalSeconds<60)return;
   var shortQ=new[]{snapshot.Primary,snapshot.Secondary}.FirstOrDefault(x=>x!=null&&x.Minutes==300);var weekQ=new[]{snapshot.Primary,snapshot.Secondary}.FirstOrDefault(x=>x!=null&&x.Minutes==10080);
   rows.Add(new HistorySample{At=at.ToString("o"),Short=shortQ==null?(double?)null:shortQ.Remaining,Weekly=weekQ==null?(double?)null:weekQ.Remaining});
   var recent=rows.Where(x=>DateTime.Parse(x.At,null,System.Globalization.DateTimeStyles.RoundtripKind)>at.AddDays(-30)).Skip(Math.Max(0,rows.Count-10000)).Select(Json.Stringify);
   var temporary=FilePath+".tmp";File.WriteAllLines(temporary,recent);if(File.Exists(FilePath))File.Replace(temporary,FilePath,null);else File.Move(temporary,FilePath);
  }catch{}finally{if(owned)mutex.ReleaseMutex();}}
 }
 public static List<HistorySample> Load(){var rows=new List<HistorySample>();try{foreach(var line in File.ReadAllLines(FilePath))try{var row=new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<HistorySample>(line);DateTime at;if(row!=null&&DateTime.TryParse(row.At,out at))rows.Add(row);}catch{}}catch{}return rows;}
 public static HistorySample[] SelectPeriod(IEnumerable<HistorySample> rows,int days,DateTimeOffset now){var start=now.AddDays(-days);return rows.Where(x=>{var at=DateTimeOffset.Parse(x.At,System.Globalization.CultureInfo.InvariantCulture);return at>=start&&at<=now;}).OrderBy(x=>DateTimeOffset.Parse(x.At,System.Globalization.CultureInfo.InvariantCulture)).ToArray();}
 public static Window Show(Window owner) {
  var w=Info.Window(L.T("Storico quote"),720);if(owner!=null)w.Owner=owner;var root=(StackPanel)((Border)w.Content).Child;
  var options=new StackPanel{Orientation=Orientation.Horizontal};root.Children.Add(options);var daily=Info.Button(L.T("Ultime 24 ore"));var weekly=Info.Button(L.T("Ultimi 7 giorni"));var export=Info.Button(L.T("Esporta CSV"));daily.Name="HistoryDaily";weekly.Name="HistoryWeekly";foreach(var b in new[]{daily,weekly,export})options.Children.Add(b);
  var coverage=new TextBlock{Name="HistoryCoverage",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,8),FontSize=12};root.Children.Add(coverage);
  var charts=new StackPanel();root.Children.Add(charts);
  root.Children.Add(new TextBlock{Text=L.T("Le linee mostrano la quota ancora disponibile: 100% = tutta, 0% = esaurita.\nUna discesa indica consumo; una salita può indicare un rinnovo o un reset.\nPassa il mouse sui punti per leggere ora e percentuale."),FontSize=12,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,12,0,8)});
  Action<int> draw=days=>{charts.Children.Clear();daily.Background=Info.Color(days==1?Theme.yellow:Theme.hover);daily.Foreground=Info.Color(days==1?Theme.background:Theme.yellow);weekly.Background=Info.Color(days==7?Theme.yellow:Theme.hover);weekly.Foreground=Info.Color(days==7?Theme.background:Theme.yellow);var start=DateTime.UtcNow.AddDays(-days);var rows=SelectPeriod(Load(),days,DateTimeOffset.UtcNow);coverage.Text=L.T("Periodo selezionato: ")+L.T(days==1?"Ultime 24 ore":"Ultimi 7 giorni")+"\n"+(rows.Length==0?L.T("Nessun dato nel periodo selezionato. Lo storico si raccoglie mentre il programma è attivo."):L.T("Dati raccolti: ")+LocalTime(rows[0].At)+" → "+LocalTime(rows[rows.Length-1].At)+" · "+rows.Length+L.T(" rilevazioni\nIl grafico usa il periodo con dati disponibili, non riempie le ore mancanti."));
   DateTime first=rows.Length>0?DateTime.Parse(rows[0].At,null,System.Globalization.DateTimeStyles.RoundtripKind):start;DateTime last=rows.Length>1?DateTime.Parse(rows[rows.Length-1].At,null,System.Globalization.DateTimeStyles.RoundtripKind):first.AddMinutes(1);if(last<=first)last=first.AddMinutes(1);
   foreach(bool isWeek in new[]{false,true}){var accent=isWeek?Theme.cyan:Theme.yellow;var valid=rows.Where(x=>(isWeek?x.Weekly:x.Short).HasValue).ToArray();var panel=new StackPanel{Margin=new Thickness(0,8,0,0)};var latest=valid.LastOrDefault();var value=latest==null?(double?)null:isWeek?latest.Weekly:latest.Short;panel.Children.Add(new TextBlock{Text=L.T(isWeek?"SETTIMANA":"5 ORE")+"  ·  "+(value.HasValue?value.Value.ToString("0.#")+L.T("% disponibile · ultimo dato"):L.T("Dato non disponibile")),Foreground=Info.Color(accent),FontSize=18,FontWeight=FontWeights.Bold});
    var chart=new Canvas{Width=660,Height=180,Background=Info.Color(Theme.background)};panel.Children.Add(chart);charts.Children.Add(panel);
    for(int percent=0;percent<=100;percent+=25){double y=142-percent*1.2;chart.Children.Add(new Line{X1=48,Y1=y,X2=650,Y2=y,Stroke=Info.Color(Theme.track),StrokeThickness=1});var label=new TextBlock{Text=percent+"%",FontSize=11,Foreground=Info.Color(Theme.cyan)};Canvas.SetTop(label,y-8);chart.Children.Add(label);}
    Polyline line=null;foreach(var row in rows){var remaining=isWeek?row.Weekly:row.Short;if(!remaining.HasValue){line=null;continue;}if(line==null){line=new Polyline{Stroke=Info.Color(accent),StrokeThickness=2};chart.Children.Add(line);}var at=DateTime.Parse(row.At,null,System.Globalization.DateTimeStyles.RoundtripKind);double x=48+(at-first).TotalSeconds/(last-first).TotalSeconds*602,y=142-Math.Max(0,Math.Min(100,remaining.Value))*1.2;line.Points.Add(new Point(x,y));var dot=new Ellipse{Width=5,Height=5,Fill=Info.Color(accent),ToolTip=LocalTime(row.At)+" · "+remaining.Value.ToString("0.#")+L.T("% disponibile"),Cursor=System.Windows.Input.Cursors.Hand};Canvas.SetLeft(dot,x-2.5);Canvas.SetTop(dot,y-2.5);chart.Children.Add(dot);}
    if(valid.Length==0){var empty=new TextBlock{Text=L.T("Nessun dato per questa quota nel periodo selezionato."),Foreground=Info.Color(accent)};Canvas.SetLeft(empty,60);Canvas.SetTop(empty,65);chart.Children.Add(empty);}
    if(rows.Length>0)for(int i=0;i<(rows.Length==1?1:3);i++){var at=first.AddSeconds((last-first).TotalSeconds*i/2);var label=new TextBlock{Text=TimeZoneInfo.ConvertTimeFromUtc(at.ToUniversalTime(),TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).ToString("dd/MM HH:mm"),FontSize=11,Foreground=Info.Color(Theme.cyan)};Canvas.SetLeft(label,i==0?48:i==1?305:575);Canvas.SetTop(label,152);chart.Children.Add(label);}
   }
  };
  daily.Click+=(s,e)=>draw(1);weekly.Click+=(s,e)=>draw(7);export.Click+=(s,e)=>{var dialog=new Microsoft.Win32.SaveFileDialog{Filter="CSV|*.csv",FileName="CodexDashboard-history.csv"};if(dialog.ShowDialog(w)==true)File.WriteAllLines(dialog.FileName,new[]{"timestamp_utc,short_remaining_percent,weekly_remaining_percent"}.Concat(Load().Select(x=>x.At+","+(x.Short.HasValue?x.Short.Value.ToString(System.Globalization.CultureInfo.InvariantCulture):"")+","+(x.Weekly.HasValue?x.Weekly.Value.ToString(System.Globalization.CultureInfo.InvariantCulture):""))));};draw(1);w.MaxHeight=Math.Max(300,SystemParameters.WorkArea.Height-24);((Border)w.Content).Child=new ScrollViewer{Content=root,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};w.Show();return w;
 }
 static string LocalTime(string at){return TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(at,null,System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime(),TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).ToString("dd/MM HH:mm");}

}
static class Info {
 public static Brush Color(string color){return (Brush)new BrushConverter().ConvertFromString(color);}
 public static Button Button(string text){return new Button{Content=text,Background=Color(Theme.hover),Foreground=Color(Theme.yellow),BorderBrush=Color(Theme.yellow),Padding=new Thickness(10,7,10,7),Margin=new Thickness(0,0,8,0)};}
 public static Window Window(string title,int width) {var w=new Window{Title="Codex Dashboard · "+title,Width=width,SizeToContent=SizeToContent.Height,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=Color(Theme.background),Foreground=Color(Theme.cyan),FontFamily=new FontFamily(Theme.font),FontSize=14,ResizeMode=ResizeMode.NoResize};var root=new StackPanel{Margin=new Thickness(20)};root.Children.Add(new TextBlock{Text="// "+title.ToUpperInvariant(),Foreground=Color(Theme.yellow),FontSize=23,Margin=new Thickness(0,0,0,15)});w.Content=new Border{BorderBrush=Color(Theme.yellow),BorderThickness=new Thickness(1),Child=root};return w;}
 public static void Show(Window owner,Snapshot snapshot) {
  var w=Window(L.T("Informazioni e diagnostica"),560);if(owner!=null)w.Owner=owner;var root=(StackPanel)((Border)w.Content).Child;
  root.Children.Add(new TextBlock{Text="Codex Dashboard "+AppVersion.Value,FontSize=22,Foreground=Color(Theme.yellow)});
  root.Children.Add(new TextBlock{Text=(snapshot.Online?(L.IsEnglish?"Connected":"Connesso"):(L.IsEnglish?"Offline":"Non connesso"))+" · "+snapshot.Plan+"\n"+(snapshot.Updated.HasValue?L.T("Aggiornato alle ")+snapshot.Updated.Value.ToLocalTime().ToString("dd/MM HH:mm:ss"):L.T("Dato non disponibile")),Margin=new Thickness(0,12,0,12),TextWrapping=TextWrapping.Wrap});
  if(snapshot.Error!=null)root.Children.Add(new TextBlock{Text=L.T(snapshot.Error),Foreground=Color(Theme.alert),TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,0,0,12)});
  var export=Button(L.T("Esporta diagnostica"));root.Children.Add(export);export.Click+=(s,e)=>{var dialog=new Microsoft.Win32.SaveFileDialog{Filter="JSON|*.json",FileName="CodexDashboard-diagnostics.json"};if(dialog.ShowDialog(w)==true)File.WriteAllText(dialog.FileName,Json.Stringify(new {version=AppVersion.Value,os=Environment.OSVersion.VersionString,settings=SettingsStore.Current,quotas=snapshot,historySamples=QuotaHistory.Load().Count}));};
  var update=Button(L.T("Installa aggiornamento…"));root.Children.Add(update);update.Click+=(s,e)=>{var dialog=new Microsoft.Win32.OpenFileDialog{Filter="Codex Dashboard setup|CodexDashboard-Setup-*.exe"};if(dialog.ShowDialog(w)==true){var name=System.Diagnostics.FileVersionInfo.GetVersionInfo(dialog.FileName);if(name.ProductName!="Codex Dashboard Setup"){MessageBox.Show(L.IsEnglish?"Select a Codex Dashboard installer.":"Seleziona un installatore Codex Dashboard.");return;}System.Diagnostics.Process.Start(dialog.FileName);}};
  root.Children.Add(new TextBlock{Text=L.IsEnglish?"Updates use the complete installer to update app and Stream Deck together. Automatic online updates require a published release channel.":"Gli aggiornamenti usano l'installatore completo per aggiornare app e Stream Deck insieme. Gli aggiornamenti automatici online richiedono un canale di distribuzione pubblicato.",Margin=new Thickness(0,12,0,0),TextWrapping=TextWrapping.Wrap,FontSize=12});w.Show();
 }
}
}
