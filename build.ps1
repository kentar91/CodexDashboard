param([switch]$Package,[switch]$Installer,[string]$StreamDeckCli='streamdeck')
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'scripts\build.ps1') @PSBoundParameters
