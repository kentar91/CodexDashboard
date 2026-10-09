param([switch]$Package,[switch]$Installer,[string]$StreamDeckCli = 'streamdeck')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$plugin = Join-Path $root 'build\streamdeck\com.codexdashboard.monitor.sdPlugin'
New-Item -ItemType Directory -Force -Path (Join-Path $root 'dist\app'),(Join-Path $plugin 'images') | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $root 'build\generated') | Out-Null
[IO.File]::WriteAllText((Join-Path $plugin 'manifest.json'),[IO.File]::ReadAllText((Join-Path $root 'src\streamdeck\manifest.json')),(New-Object Text.UTF8Encoding($false)))
Copy-Item -Path (Join-Path $root 'src\streamdeck\*.html') -Destination $plugin -Force
Copy-Item -Path (Join-Path $root 'src\streamdeck\en.json') -Destination $plugin -Force
$theme = Get-Content -LiteralPath (Join-Path $root 'assets\theme.json') -Raw | ConvertFrom-Json
$xaml = Get-Content -LiteralPath (Join-Path $root 'src\Widget.xaml') -Raw
$constants = @()
foreach ($key in 'yellow','cyan','background','track','muted','alert','hover','font') {
 $value = [string]$theme.$key
 if ($key -ne 'font' -and $value -notmatch '^#[0-9a-fA-F]{6}$') { throw "Colore non valido: $key" }
 if ($value -match '["\\\r\n]') { throw "Valore tema non valido: $key" }
 $xaml = $xaml.Replace('@'+$key,$value)
 $constants += 'public const string '+$key+' = "'+$value+'";'
}
[IO.File]::WriteAllText((Join-Path $root 'build\generated\Widget.generated.xaml'),$xaml,[Text.Encoding]::UTF8)
[IO.File]::WriteAllText((Join-Path $root 'build\generated\Theme.generated.cs'),('namespace CodexDashboard { static class Theme { '+($constants -join ' ')+' } }'),[Text.Encoding]::UTF8)
$refs = @('System.dll','System.Core.dll','System.Web.Extensions.dll','System.Drawing.dll','System.Windows.Forms.dll','WPF\PresentationCore.dll','WPF\PresentationFramework.dll','WPF\WindowsBase.dll','System.Xaml.dll') | ForEach-Object { '/reference:' + (Join-Path $framework $_) }
$branding = Join-Path $root 'assets\branding'
Copy-Item -LiteralPath (Join-Path $branding 'codex-dashboard-icon-72.png') -Destination (Join-Path $plugin 'images\icon.png') -Force
Copy-Item -LiteralPath (Join-Path $branding 'codex-dashboard-icon-144.png') -Destination (Join-Path $plugin 'images\icon@2x.png') -Force
$iconPath = Join-Path $root 'dist\app\CodexDashboard.ico'
Copy-Item -LiteralPath (Join-Path $branding 'CodexDashboard.ico') -Destination $iconPath -Force
& (Join-Path $framework 'csc.exe') /nologo /target:winexe /platform:x64 /optimize+ ('/out:'+(Join-Path $root 'dist\app\CodexDashboard.exe')) ('/win32icon:'+$iconPath) ('/resource:'+$iconPath+',Dashboard.ico') ('/resource:'+(Join-Path $branding 'codex-dashboard-icon-256.png')+',DashboardLogo.png') ('/resource:'+(Join-Path $root 'build\generated\Widget.generated.xaml')+',Widget.xaml') $refs (Join-Path $root 'src\CodexDashboard.cs') (Join-Path $root 'src\Settings.cs') (Join-Path $root 'src\Features.cs') (Join-Path $root 'src\StartupTask.cs') (Join-Path $root 'build\generated\Theme.generated.cs')
if ($LASTEXITCODE -ne 0) { throw 'Compilazione non riuscita.' }
Copy-Item -LiteralPath (Join-Path $root 'dist\app\CodexDashboard.exe') -Destination (Join-Path $plugin 'CodexDashboard.exe') -Force
Write-Output 'Creati programma e plugin con tema Night City HUD.'

foreach ($name in 'Avvia-CodexDashboard.vbs','Impostazioni-CodexDashboard.vbs','LEGGIMI.txt','README.en.txt') {
 Copy-Item -LiteralPath (Join-Path $root ('scripts\launchers\'+$name)) -Destination (Join-Path $root ('dist\app\'+$name)) -Force
}
Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination (Join-Path $root 'dist\app\LICENSE') -Force
New-Item -ItemType Directory -Path (Join-Path $root 'dist\branding'),(Join-Path $root 'dist\streamdeck') -Force | Out-Null
Compress-Archive -Path (Join-Path $branding '*') -DestinationPath (Join-Path $root 'dist\branding\CodexDashboard-logo-kit.zip') -Force
if ($Package -or $Installer) {
 & $StreamDeckCli pack $plugin --output (Join-Path $root 'dist\streamdeck') --force
 if ($LASTEXITCODE -ne 0) { throw 'Pacchetto Stream Deck non riuscito.' }
}
if ($Installer) {
 New-Item -ItemType Directory -Path (Join-Path $root 'dist\installer') -Force | Out-Null
 $setup = Join-Path $root 'dist\installer\CodexDashboard-Setup-2.0.7.exe'
 & (Join-Path $framework 'csc.exe') /nologo /target:winexe /platform:x64 /optimize+ ('/out:'+$setup) ('/win32icon:'+$iconPath) ('/resource:'+$iconPath+',App.ico') ('/resource:'+(Join-Path $root 'dist\app\CodexDashboard.exe')+',App.exe') ('/resource:'+(Join-Path $root 'dist\streamdeck\com.codexdashboard.monitor.streamDeckPlugin')+',Plugin.streamDeckPlugin') $refs (Join-Path $root 'src\Installer.cs') (Join-Path $root 'src\StartupTask.cs') (Join-Path $root 'build\generated\Theme.generated.cs')
 if ($LASTEXITCODE -ne 0) { throw 'Compilazione installatore non riuscita.' }
 Write-Output ('Creato installatore bilingue: '+$setup)
}
