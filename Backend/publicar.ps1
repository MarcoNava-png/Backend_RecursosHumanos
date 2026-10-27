param([string] $Salida = (Join-Path $PSScriptRoot 'publicado'))

$ErrorActionPreference = 'Stop'
if (Test-Path $Salida) { Remove-Item $Salida -Recurse -Force }

dotnet publish (Join-Path $PSScriptRoot 'src\RecursosHumanos.Api') -c Release -r win-x64 --self-contained -o (Join-Path $Salida 'Api')
if ($LASTEXITCODE -ne 0) { throw 'Falló la publicación.' }

Copy-Item (Join-Path $PSScriptRoot 'instalar-servidor.ps1') $Salida
Copy-Item (Join-Path $PSScriptRoot '..\BaseDatos') (Join-Path $Salida 'BaseDatos') -Recurse
Copy-Item (Join-Path $PSScriptRoot '..\README.md') $Salida
Write-Host "Listo: $Salida. Cópiala al servidor y corre instalar-servidor.ps1 como administrador."

