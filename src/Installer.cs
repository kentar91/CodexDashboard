using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using Microsoft.Win32;

[assembly:AssemblyTitle("Codex Dashboard Setup")]
[assembly:AssemblyProduct("Codex Dashboard Setup")]
[assembly:AssemblyVersion("2.0.1.0")]
[assembly:AssemblyFileVersion("2.0.1.0")]
namespace CodexDashboard {
static class Setup {
 const string UninstallKey="Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\CodexDashboard";
 static string InstallPath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","CodexDashboard");}}
 static string DataPath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CodexDashboard");}}
 static string ShortcutPath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"Codex Dashboard");}}
 static bool English;
 static string T(string it,string en){return English?en:it;}
 static Color ColorOf(string value){return ColorTranslator.FromHtml(value);}
 [STAThread]static int Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length==2&&args[0]=="--extract-test"){Directory.CreateDirectory(args[1]);Extract("App.exe",Path.Combine(args[1],"CodexDashboard.exe"));Extract("Plugin.streamDeckPlugin",Path.Combine(args[1],"com.codexdashboard.monitor.streamDeckPlugin"));Extract("App.ico",Path.Combine(args[1],"CodexDashboard.ico"));return 0;}
  bool uninstall=args.Contains("--uninstall");English=System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName=="en";
  try{var settings=new JavaScriptSerializer().Deserialize<System.Collections.Generic.Dictionary<string,object>>(File.ReadAllText(Path.Combine(DataPath,"settings.json")));if(settings.ContainsKey("Language"))English=(string)settings["Language"]=="en";}catch{}
  if(args.Contains("--preview-en"))English=true;if(args.Contains("--preview-it"))English=false;
  var w=new Form{Text="Codex Dashboard Setup",Width=590,Height=470,StartPosition=FormStartPosition.CenterScreen,BackColor=ColorOf(Theme.background),ForeColor=ColorOf(Theme.cyan),Font=new Font(Theme.font,11),FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false};
  using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("App.ico"))w.Icon=new Icon(stream);
  var header=new Label{Text="// CODEX DASHBOARD 2.0",Left=24,Top=26,Width=530,Height=40,ForeColor=ColorOf(Theme.yellow),Font=new Font(Theme.font,23,FontStyle.Bold)};
  var language=new Panel{Left=325,Top=82,Width=230,Height=30};
  var italian=new RadioButton{Text="Italiano",Left=0,Top=0,Width=115,Height=28,Checked=!English,ForeColor=ColorOf(Theme.yellow)};
  var english=new RadioButton{Text="English",Left=115,Top=0,Width=115,Height=28,Checked=English,ForeColor=ColorOf(Theme.yellow)};
  language.Controls.AddRange(new Control[]{italian,english});
  var languageLabel=new Label{Left=24,Top=86,Width=300,Height=28};var description=new Label{Left=24,Top=128,Width=525,Height=110};
  var desktop=new CheckBox{Left=24,Top=248,Width=525,Height=25,Checked=true};var deck=new CheckBox{Left=24,Top=280,Width=525,Height=25,Checked=true,Visible=!uninstall};
  var progress=new Label{Left=24,Top=313,Width=525,Height=40,ForeColor=ColorOf(Theme.yellow)};
  var install=new Button{Left=355,Top=372,Width=195,Height=40,BackColor=ColorOf(Theme.hover),ForeColor=ColorOf(Theme.yellow),FlatStyle=FlatStyle.Flat};
  Action translate=()=>{English=english.Checked;languageLabel.Text=T("Lingua / Language","Language / Lingua");description.Text=uninstall?T("Rimuove il programma e i collegamenti.\nImpostazioni e storico personali vengono conservati.","Removes the app and shortcuts.\nPersonal settings and history are preserved."):T("Installa o aggiorna app e pacchetto Stream Deck per il tuo utente.\nLa lingua scelta sarà usata anche dal programma.\nDestinazione: ","Installs or updates the app and Stream Deck package for your user.\nThe app will use the selected language.\nDestination: ")+InstallPath;desktop.Text=uninstall?T("Conferma rimozione del programma","Confirm app removal"):T("Crea collegamento sul desktop","Create a desktop shortcut");deck.Text=T("Apri il pacchetto in Stream Deck al termine","Open the package in Stream Deck when finished");install.Text=uninstall?T("Disinstalla","Uninstall"):T("Installa / Aggiorna","Install / Update");};italian.CheckedChanged+=(s,e)=>{if(italian.Checked)translate();};english.CheckedChanged+=(s,e)=>{if(english.Checked)translate();};translate();
  bool completed=false;install.Click+=(s,e)=>{if(completed){if(!uninstall)Process.Start(Path.Combine(InstallPath,"CodexDashboard.exe"));w.Close();return;}if(uninstall&&!desktop.Checked)return;install.Enabled=false;language.Enabled=false;try{if(uninstall){Uninstall();progress.Text=T("Rimozione completata.","Removal completed.");install.Text=T("Chiudi","Close");}else{Install(English?"en":"it",desktop.Checked);progress.Text=T("Installazione completata.","Installation completed.");install.Text=T("Apri Codex Dashboard","Open Codex Dashboard");if(deck.Checked){var package=Path.Combine(InstallPath,"com.codexdashboard.monitor.streamDeckPlugin");try{Process.Start(new ProcessStartInfo(package){UseShellExecute=true});}catch{MessageBox.Show(w,T("Apri manualmente il pacchetto Stream Deck dalla cartella del programma.","Open the Stream Deck package manually from the app folder."));}}}completed=true;install.Enabled=true;}catch(Exception ex){progress.Text=T("Operazione non riuscita: ","Operation failed: ")+ex.Message;install.Enabled=true;language.Enabled=true;}};
  w.Controls.AddRange(new Control[]{header,language,languageLabel,description,desktop,deck,progress,install});
  if(args.Any(x=>x.StartsWith("--preview-")))w.Shown+=(s,e)=>{using(var bitmap=new Bitmap(w.Width,w.Height)){w.DrawToBitmap(bitmap,new Rectangle(0,0,w.Width,w.Height));var directory=Environment.GetEnvironmentVariable("CODEXDASHBOARD_ARTIFACT_DIR");if(string.IsNullOrEmpty(directory))throw new IOException("Set CODEXDASHBOARD_ARTIFACT_DIR for previews");Directory.CreateDirectory(directory);bitmap.Save(Path.Combine(directory,"installer-"+(English?"en":"it")+".png"),System.Drawing.Imaging.ImageFormat.Png);}w.Close();};
  Application.Run(w);return 0;
 }
 static void Extract(string name,string target){using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream(name))using(var output=File.Create(target)){if(stream==null)throw new IOException("Missing package: "+name);stream.CopyTo(output);}}
 static void CloseWidget(){try{using(var e=EventWaitHandle.OpenExisting("Local\\CodexDashboardWidgetClose"))e.Set();Thread.Sleep(650);}catch{} }
 static void Install(string language,bool desktop){
  CloseWidget();Directory.CreateDirectory(InstallPath);var names=new[]{"CodexDashboard.exe","CodexDashboard.ico","com.codexdashboard.monitor.streamDeckPlugin"};var resources=new[]{"App.exe","App.ico","Plugin.streamDeckPlugin"};
  string stage=Path.Combine(InstallPath,"stage-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(stage);
  var backed=new System.Collections.Generic.List<string>();var written=new System.Collections.Generic.List<string>();
  try{for(int i=0;i<names.Length;i++)Extract(resources[i],Path.Combine(stage,names[i]));
   for(int i=0;i<names.Length;i++){string target=Path.Combine(InstallPath,names[i]);if(File.Exists(target)){File.Move(target,Path.Combine(stage,names[i]+".backup"));backed.Add(names[i]);}File.Copy(Path.Combine(stage,names[i]),target);written.Add(names[i]);}
  }catch{foreach(var name in written)File.Delete(Path.Combine(InstallPath,name));foreach(var name in backed)File.Move(Path.Combine(stage,name+".backup"),Path.Combine(InstallPath,name));throw;}
  finally{foreach(var file in Directory.GetFiles(stage))File.Delete(file);Directory.Delete(stage);}
  var self=Assembly.GetExecutingAssembly().Location;var uninstall=Path.Combine(InstallPath,"Uninstall.exe");if(!string.Equals(self,uninstall,StringComparison.OrdinalIgnoreCase))File.Copy(self,uninstall,true);
  Directory.CreateDirectory(DataPath);var fileSettings=Path.Combine(DataPath,"settings.json");var json=new JavaScriptSerializer();var settings=new System.Collections.Generic.Dictionary<string,object>();if(File.Exists(fileSettings))settings=json.Deserialize<System.Collections.Generic.Dictionary<string,object>>(File.ReadAllText(fileSettings));settings["Language"]=language;var temporary=fileSettings+".setup.tmp";File.WriteAllText(temporary,json.Serialize(settings));if(File.Exists(fileSettings))File.Replace(temporary,fileSettings,null);else File.Move(temporary,fileSettings);
  Directory.CreateDirectory(ShortcutPath);Shortcut(Path.Combine(ShortcutPath,"Codex Dashboard.lnk"),Path.Combine(InstallPath,"CodexDashboard.exe"),"");Shortcut(Path.Combine(ShortcutPath,"Impostazioni - Settings.lnk"),Path.Combine(InstallPath,"CodexDashboard.exe"),"--settings");if(desktop)Shortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Codex Dashboard.lnk"),Path.Combine(InstallPath,"CodexDashboard.exe"),"");
  using(var key=Registry.CurrentUser.CreateSubKey(UninstallKey)){key.SetValue("DisplayName","Codex Dashboard");key.SetValue("DisplayVersion","2.0.1");key.SetValue("Publisher","CodexDashboard");key.SetValue("InstallLocation",InstallPath);key.SetValue("DisplayIcon",Path.Combine(InstallPath,"CodexDashboard.ico"));key.SetValue("UninstallString","\""+uninstall+"\" --uninstall");key.SetValue("NoModify",1);key.SetValue("NoRepair",1);}
  using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true)){if(key!=null&&key.GetValue("CodexDashboard")!=null){var old=Convert.ToString(key.GetValue("CodexDashboard"));key.SetValue("CodexDashboard","\""+Path.Combine(InstallPath,"CodexDashboard.exe")+"\""+(old.Contains("--watch-codex")?" --watch-codex":""));}}
 }
 static void Shortcut(string name,string target,string args){object shell=null,shortcut=null;try{shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));shortcut=shell.GetType().InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{name});var type=shortcut.GetType();type.InvokeMember("TargetPath",BindingFlags.SetProperty,null,shortcut,new object[]{target});type.InvokeMember("Arguments",BindingFlags.SetProperty,null,shortcut,new object[]{args});type.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,shortcut,new object[]{InstallPath});type.InvokeMember("Save",BindingFlags.InvokeMethod,null,shortcut,null);}finally{if(shortcut!=null)System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);if(shell!=null)System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);}}
 static string Literal(string text){return "'"+text.Replace("'","''")+"'";}
 static void Uninstall(){
  CloseWidget();using(var key=Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",true)){if(key!=null)key.DeleteValue("CodexDashboard",false);}Registry.CurrentUser.DeleteSubKeyTree(UninstallKey,false);
  foreach(var name in new[]{"Codex Dashboard.lnk","Impostazioni - Settings.lnk"}){var file=Path.Combine(ShortcutPath,name);if(File.Exists(file))File.Delete(file);}if(Directory.Exists(ShortcutPath)&&Directory.GetFileSystemEntries(ShortcutPath).Length==0)Directory.Delete(ShortcutPath);var desktop=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"Codex Dashboard.lnk");if(File.Exists(desktop))File.Delete(desktop);
  // Exact known files only; preserve data and any other files the user placed here.
  var known=new[]{"CodexDashboard.exe","CodexDashboard.ico","com.codexdashboard.monitor.streamDeckPlugin","Uninstall.exe"}.Select(x=>Path.GetFullPath(Path.Combine(InstallPath,x))).ToArray();foreach(var path in known)if(!string.Equals(Path.GetDirectoryName(path),Path.GetFullPath(InstallPath),StringComparison.OrdinalIgnoreCase))throw new IOException("Invalid uninstall path");
  string script="Wait-Process -Id "+Process.GetCurrentProcess().Id+" -ErrorAction SilentlyContinue; "+string.Join("; ",known.Select(x=>"Remove-Item -LiteralPath "+Literal(x)+" -Force -ErrorAction SilentlyContinue"))+"; if ((Test-Path -LiteralPath "+Literal(InstallPath)+") -and @(Get-ChildItem -LiteralPath "+Literal(InstallPath)+" -Force).Count -eq 0) { Remove-Item -LiteralPath "+Literal(InstallPath)+" -Force }";
  Process.Start(new ProcessStartInfo("powershell.exe","-NoProfile -WindowStyle Hidden -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(script))){CreateNoWindow=true,UseShellExecute=false});
 }
}
}
