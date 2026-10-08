$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$plugin = Join-Path $root 'streamdeck\com.codexdashboard.monitor.sdPlugin'
New-Item -ItemType Directory -Force -Path (Join-Path $root 'outputs'),(Join-Path $plugin 'images') | Out-Null
$refs = @('System.dll','System.Core.dll','System.Web.Extensions.dll','System.Drawing.dll','System.Windows.Forms.dll','WPF\PresentationCore.dll','WPF\PresentationFramework.dll','WPF\WindowsBase.dll','System.Xaml.dll') | ForEach-Object { '/reference:' + (Join-Path $framework $_) }
& (Join-Path $framework 'csc.exe') /nologo /target:winexe /platform:x64 /optimize+ ('/out:'+(Join-Path $root 'outputs\CodexDashboard.exe')) ('/resource:'+(Join-Path $root 'src\Widget.xaml')+',Widget.xaml') $refs (Join-Path $root 'src\CodexDashboard.cs')
if ($LASTEXITCODE -ne 0) { throw 'Compilazione non riuscita.' }
Copy-Item -LiteralPath (Join-Path $root 'outputs\CodexDashboard.exe') -Destination (Join-Path $plugin 'CodexDashboard.exe') -Force
Add-Type -AssemblyName System.Drawing
foreach ($size in 72,144) {
 $bitmap = New-Object Drawing.Bitmap $size,$size
 $graphics = [Drawing.Graphics]::FromImage($bitmap)
 $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#0B1017'))
 $yellow = New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#FCEE0A'))
 $cyan = New-Object Drawing.SolidBrush ([Drawing.ColorTranslator]::FromHtml('#00E6F6'))
 $font = New-Object Drawing.Font 'Consolas',([single]($size/5)),([Drawing.FontStyle]::Bold)
 $graphics.FillRectangle($yellow,0,0,$size,([single]($size/20)))
 $graphics.DrawString('CD',$font,$yellow,([single]($size/4)),([single]($size/3)))
 $graphics.FillRectangle($cyan,([single]($size/8)),([single]($size*0.78)),([single]($size*0.75)),([single]($size/24)))
 $name = if($size -eq 72){'icon.png'}else{'icon@2x.png'}
 $bitmap.Save((Join-Path $plugin ('images\'+$name)),[Drawing.Imaging.ImageFormat]::Png)
 $font.Dispose();$yellow.Dispose();$cyan.Dispose();$graphics.Dispose();$bitmap.Dispose()
}
Write-Output 'Creati programma Windows e plugin Stream Deck.'
