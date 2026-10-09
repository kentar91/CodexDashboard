using System;
using System.IO;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;

namespace CodexDashboard {
static class StartupTask {
 public const string Name="CodexDashboard-CodexStartup";
 static int Run(string arguments){using(var process=new Process{StartInfo=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"schtasks.exe"),arguments){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true}}){process.Start();string output=process.StandardOutput.ReadToEnd();string error=process.StandardError.ReadToEnd();process.WaitForExit();if(process.ExitCode!=0&&arguments.StartsWith("/Create"))throw new IOException("Registrazione avvio Codex non riuscita: "+output+error);return process.ExitCode;}}
 public static bool Exists(){return Run("/Query /TN \""+Name+"\"")==0;}
 public static void Configure(bool enabled,string executable,string directory){
  if(!enabled){if(Exists()&&Run("/Delete /TN \""+Name+"\" /F")!=0)throw new IOException("Rimozione avvio Codex non riuscita.");return;}
  Directory.CreateDirectory(directory);string file=Path.Combine(directory,"startup-task.xml");string user=WindowsIdentity.GetCurrent().User.Value;
  var triggers=new System.Text.StringBuilder();var first=DateTime.Now.AddSeconds(10);for(int i=0;i<6;i++)triggers.Append("<TimeTrigger><Repetition><Interval>PT1M</Interval><StopAtDurationEnd>false</StopAtDurationEnd></Repetition><StartBoundary>"+first.AddSeconds(i*10).ToString("yyyy-MM-ddTHH:mm:ss")+"</StartBoundary><Enabled>true</Enabled></TimeTrigger>");
  string xml="<?xml version=\"1.0\" encoding=\"UTF-16\"?><Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\"><Triggers>"+triggers+"</Triggers><Principals><Principal id=\"User\"><UserId>"+user+"</UserId><LogonType>InteractiveToken</LogonType><RunLevel>LeastPrivilege</RunLevel></Principal></Principals><Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy><DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries><StopIfGoingOnBatteries>false</StopIfGoingOnBatteries><StartWhenAvailable>true</StartWhenAvailable><ExecutionTimeLimit>PT0S</ExecutionTimeLimit></Settings><Actions Context=\"User\"><Exec><Command>"+SecurityElement.Escape(executable)+"</Command><Arguments>--launch-with-codex</Arguments></Exec></Actions></Task>";
  try{File.WriteAllText(file,xml,System.Text.Encoding.Unicode);Run("/Create /TN \""+Name+"\" /XML \""+file+"\" /F");}finally{if(File.Exists(file))File.Delete(file);}
 }
}
}
