#Requires -RunAsAdministrator
param(
    [string] $Destino = 'D:\RecursosHumanos',
    [int] $Puerto = 5000,
    [string] $ServidorBd,
    [int] $PuertoBd = 5432,
    [string[]] $Origenes = @(),
    [switch] $Documentacion,
    [switch] $Reconfigurar
)

$ErrorActionPreference = 'Stop'
$servicio = 'RecursosHumanos'
$cuenta = "NT SERVICE\$servicio"
$api = Join-Path $Destino 'Api'
$archivos = Join-Path $Destino 'archivos'
$config = Join-Path $api 'appsettings.Production.json'

if (Get-Service $servicio -ErrorAction SilentlyContinue) { Stop-Service $servicio }

New-Item -ItemType Directory -Force $api, $archivos | Out-Null
robocopy (Join-Path $PSScriptRoot 'Api') $api /MIR /XF appsettings.Production.json /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'No se pudieron copiar los archivos.' }

if (-not (Get-Service $servicio -ErrorAction SilentlyContinue)) {
    New-Service -Name $servicio -BinaryPathName "`"$api\RecursosHumanos.Api.exe`"" -DisplayName 'RecursosHumanos Api' `
        -Description 'Api de RecursosHumanos (asistencia y nómina)' -StartupType Automatic | Out-Null
    sc.exe config $servicio obj= $cuenta | Out-Null
    sc.exe failure $servicio reset= 86400 actions= restart/60000 | Out-Null
}
if (-not (Get-NetFirewallRule -DisplayName "RecursosHumanos Api ($Puerto)" -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName "RecursosHumanos Api ($Puerto)" -Direction Inbound -Protocol TCP -LocalPort $Puerto -Action Allow | Out-Null
}

if (-not (Test-Path $config) -or $Reconfigurar) {
    if (-not $ServidorBd) { throw 'La primera vez indica -ServidorBd (IP o nombre del servidor de PostgreSQL).' }
    $segura = Read-Host 'Contraseña de recursoshumanos_app en PostgreSQL' -AsSecureString
    $claveBd = [Runtime.InteropServices.Marshal]::PtrToStringBSTR([Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura))
    $bytes = New-Object byte[] 48
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    @{
        Urls              = "http://0.0.0.0:$Puerto"
        ConnectionStrings = @{ RecursosHumanos = "Host=$ServidorBd;Port=$PuertoBd;Database=recursoshumanos;Username=recursoshumanos_app;Password=`"$claveBd`"" }
        Jwt               = @{ Clave = [Convert]::ToBase64String($bytes) }
        Archivos          = @{ Carpeta = $archivos }
        Cors              = @{ Origenes = @($Origenes) }
        Documentacion     = @{ Habilitada = [bool]$Documentacion }
    } | ConvertTo-Json -Depth 3 | Set-Content $config -Encoding UTF8
}

icacls $api /grant "${cuenta}:(OI)(CI)RX" | Out-Null
icacls $archivos /grant "${cuenta}:(OI)(CI)M" | Out-Null
icacls $config /inheritance:r /grant:r '*S-1-5-32-544:F' '*S-1-5-18:F' "${cuenta}:R" | Out-Null

Start-Service $servicio
Start-Sleep -Seconds 3
try {
    Invoke-RestMethod "http://localhost:$Puerto/api/auth/entra/configuracion" | Out-Null
    Write-Host "Listo: http://$($env:COMPUTERNAME):$Puerto (documentación en /scalar si se habilitó)"
}
catch {
    Write-Warning "El servicio no responde. Revisa el Visor de eventos > Registros de Windows > Aplicación (origen RecursosHumanos)."
}

