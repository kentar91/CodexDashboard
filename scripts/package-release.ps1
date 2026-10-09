param([string]$StreamDeckCli='streamdeck',[string]$Node='node')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$manifest=Get-Content (Join-Path $root 'src/streamdeck/manifest.json') -Raw | ConvertFrom-Json
$version=[string]$manifest.Version -replace '\.0$',''
& (Join-Path $root 'build.ps1') -Installer -StreamDeckCli $StreamDeckCli
& $StreamDeckCli validate (Join-Path $root 'build/streamdeck/com.codexdashboard.monitor.sdPlugin')
if($LASTEXITCODE -ne 0){throw 'Stream Deck validation failed.'}
& (Join-Path $root 'tests/release-smoke.ps1') -Node $Node
& (Join-Path $root 'tests/installer-components.ps1')
& (Join-Path $root 'tests/full-close-smoke.ps1')
$app=Join-Path $root 'dist/app'
New-Item -ItemType Directory -Force (Join-Path $app 'docs'),(Join-Path $app 'assets/branding') | Out-Null
foreach($name in 'README.md','README.en.md','LICENSE'){Copy-Item -LiteralPath (Join-Path $root $name) -Destination $app -Force}
Copy-Item -Path (Join-Path $root 'docs/*.md') -Destination (Join-Path $app 'docs') -Force
Copy-Item -LiteralPath (Join-Path $root 'assets/branding/codex-dashboard-logo-dark.png') -Destination (Join-Path $app 'assets/branding') -Force
$release=Join-Path $root 'dist/release'
New-Item -ItemType Directory -Force $release | Out-Null
Copy-Item -LiteralPath (Join-Path $root "dist/installer/CodexDashboard-Setup-$version.exe"),(Join-Path $root 'dist/streamdeck/com.codexdashboard.monitor.streamDeckPlugin'),(Join-Path $root 'dist/branding/CodexDashboard-logo-kit.zip') -Destination $release -Force
$portable="CodexDashboard-$version-windows-x64.zip"
Compress-Archive -Path (Join-Path $app '*') -DestinationPath (Join-Path $release $portable) -Force
$names=@("CodexDashboard-Setup-$version.exe",'com.codexdashboard.monitor.streamDeckPlugin','CodexDashboard-logo-kit.zip',$portable)
$lines=foreach($name in $names){(Get-FileHash -LiteralPath (Join-Path $release $name) -Algorithm SHA256).Hash.ToLowerInvariant()+'  '+$name}
[IO.File]::WriteAllText((Join-Path $release 'SHA256SUMS.txt'),(($lines -join "`n")+"`n"),[Text.UTF8Encoding]::new($false))
Write-Output "PASS: release $version ready in $release / rilascio pronto in $release"
