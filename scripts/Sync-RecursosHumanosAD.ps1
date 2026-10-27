#Requires -Version 5.1
#Requires -Modules ActiveDirectory
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)] [string] $OuBase,
    [string] $CarpetaLogs = (Join-Path $PSScriptRoot 'logs'),
    [string] $CarpetaContrasenas = 'D:\TI\RecursosHumanos\Contrasenas',
    [string] $Psql = 'C:\Program Files\PostgreSQL\18\bin\psql.exe'
)

$ErrorActionPreference = 'Stop'
$simular = [bool]$WhatIfPreference
$ejecucion = Get-Date -Format 'yyyyMMdd_HHmmss'
$utf8ConBom = New-Object System.Text.UTF8Encoding $true
$utf8SinBom = New-Object System.Text.UTF8Encoding $false
$env:PGCLIENTENCODING = 'UTF8'

[void][IO.Directory]::CreateDirectory($CarpetaLogs)
$archivoLog = Join-Path $CarpetaLogs "sync_$ejecucion.log"
$archivoContrasenas = Join-Path $CarpetaContrasenas "contrasenas_$ejecucion.csv"

$atributos = 'sAMAccountName', 'userPrincipalName', 'mail', 'givenName', 'sn', 'displayName', 'title',
             'department', 'company', 'physicalDeliveryOfficeName', 'streetAddress', 'l', 'st',
             'postalCode', 'employeeID', 'telephoneNumber', 'mobile'

function Write-Log([string] $Texto) {
    $linea = '{0:yyyy-MM-dd HH:mm:ss} {1}' -f (Get-Date), $Texto
    [IO.File]::AppendAllText($archivoLog, $linea + [Environment]::NewLine, $utf8ConBom)
    Write-Host $linea
}

function ConvertTo-SqlTexto($Valor) {
    if ([string]::IsNullOrEmpty($Valor)) { 'NULL' } else { "'" + ([string]$Valor).Replace("'", "''") + "'" }
}

function ConvertTo-LdapValor([string] $Valor) {
    $Valor -replace '\\', '\5c' -replace '\*', '\2a' -replace '\(', '\28' -replace '\)', '\29'
}

function Invoke-Psql([string] $Sql) {
    $ErrorActionPreference = 'Continue'
    $entrada = [IO.Path]::GetTempFileName()
    $salida = [IO.Path]::GetTempFileName()
    try {
        [IO.File]::WriteAllText($entrada, $Sql, $utf8SinBom)
        $errores = & $Psql -X -q -A -t -v ON_ERROR_STOP=1 -f $entrada -o $salida 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) { throw "psql: $errores" }
        [IO.File]::ReadAllText($salida, $utf8SinBom)
    }
    finally {
        [IO.File]::Delete($entrada)
        [IO.File]::Delete($salida)
    }
}

function Register-Sincronizacion($IdEmpleado, [string] $Accion, [bool] $Exitoso, [string] $Mensaje, $Cuenta) {
    $guid = if ($Cuenta) { "'$($Cuenta.ObjectGUID)'::uuid" } else { 'NULL' }
    Invoke-Psql ("SELECT recursos_humanos.fn_registrar_sincronizacion({0}, {1}, {2}, {3}, {4}, {5});" -f
        [int]$IdEmpleado, (ConvertTo-SqlTexto $Accion), $Exitoso.ToString().ToLower(),
        (ConvertTo-SqlTexto $Mensaje), $guid, (ConvertTo-SqlTexto $Cuenta.DistinguishedName)) | Out-Null
}

function Test-DentroDeUsag([string] $Dn) {
    $Dn -and ($Dn -eq $OuBase -or $Dn.EndsWith(",$OuBase", [StringComparison]::OrdinalIgnoreCase))
}

function Find-Cuenta($Empleado) {
    if ($Empleado.object_guid) {
        try { return Get-ADUser -Identity ([guid]$Empleado.object_guid) -Properties adminCount }
        catch [Microsoft.ActiveDirectory.Management.ADIdentityNotFoundException] { }
    }
    foreach ($filtro in "(sAMAccountName=$(ConvertTo-LdapValor $Empleado.sAMAccountName))",
                        "(userPrincipalName=$(ConvertTo-LdapValor $Empleado.userPrincipalName))") {
        $cuenta = Get-ADUser -LDAPFilter $filtro -Properties adminCount
        if ($cuenta) { return $cuenta }
    }
}

function Assert-CuentaModificable($Cuenta) {
    if ($Cuenta.adminCount -eq 1 -or -not (Test-DentroDeUsag $Cuenta.DistinguishedName)) {
        throw "La cuenta $($Cuenta.DistinguishedName) es protegida o está fuera de $OuBase; no se modifica."
    }
}

function Get-Atributos($Empleado) {
    $reemplazar = @{}
    $limpiar = @()
    foreach ($a in $atributos) {
        if ([string]::IsNullOrWhiteSpace($Empleado.$a)) { $limpiar += $a } else { $reemplazar[$a] = [string]$Empleado.$a }
    }
    $pendiente = $null
    if (-not $Empleado.manager) {
        $limpiar += 'manager'
    }
    else {
        $jefe = Get-ADUser -LDAPFilter "(sAMAccountName=$(ConvertTo-LdapValor $Empleado.manager))"
        if ($jefe) { $reemplazar.manager = $jefe.DistinguishedName }
        else { $pendiente = "El jefe $($Empleado.manager) todavía no existe en AD; se reintenta en la siguiente ejecución." }
    }
    [pscustomobject]@{ Reemplazar = $reemplazar; Limpiar = $limpiar; Pendiente = $pendiente }
}

function New-ContrasenaTemporal {
    $caracteres = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!#$%*+-=?@'
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    $bytes = New-Object byte[] 16
    do {
        $rng.GetBytes($bytes)
        $contrasena = -join ($bytes | ForEach-Object { $caracteres[$_ % $caracteres.Length] })
    } until ($contrasena -cmatch '[A-Z]' -and $contrasena -cmatch '[a-z]' -and $contrasena -match '\d' -and $contrasena -match '[^A-Za-z0-9]')
    $contrasena
}

function New-Cuenta($Empleado, $Datos) {
    $otros = $Datos.Reemplazar.Clone()
    $otros.Remove('sAMAccountName')
    $otros.Remove('userPrincipalName')
    $upn = $Empleado.userPrincipalName
    if ($Empleado.cuenta_existente_m365) {
        $upn = $Empleado.mail
        $otros.mail = $Empleado.mail
        $otros.proxyAddresses = "SMTP:$($Empleado.mail)"
    }
    $contrasena = New-ContrasenaTemporal
    $cuenta = New-ADUser -Name $Empleado.displayName -SamAccountName $Empleado.sAMAccountName -UserPrincipalName $upn `
        -Path $Empleado.ou_destino -OtherAttributes $otros -Enabled $true -ChangePasswordAtLogon $true `
        -AccountPassword (ConvertTo-SecureString $contrasena -AsPlainText -Force) -PassThru
    [pscustomobject]@{
        fecha               = Get-Date -Format 'yyyy-MM-dd HH:mm'
        id_empleado         = $Empleado.id_empleado
        nombre              = $Empleado.displayName
        usuario             = $upn
        contrasena_temporal = $contrasena
    } | Export-Csv -Path $archivoContrasenas -Append -NoTypeInformation -Encoding UTF8
    $cuenta
}

if (-not $simular -and -not (Test-Path $CarpetaContrasenas)) {
    throw "No existe la carpeta de contraseñas $CarpetaContrasenas. Créala con permisos solo para TI."
}

Write-Log ("Inicio de sincronización" + $(if ($simular) { ' (SIMULACIÓN -WhatIf: no se cambia AD ni la base)' }))
$empleados = Invoke-Psql "SELECT coalesce(json_agg(v), '[]') FROM recursos_humanos.vw_bi_directorio_empleados v WHERE requiere_sincronizacion;" |
    ConvertFrom-Json
$correctos = 0
$fallidos = 0

foreach ($e in $empleados) {
    $cuenta = $null
    $acciones = @()
    $accion = 'actualizar'
    $quien = "[$($e.id_empleado)] $($e.sAMAccountName)"
    try {
        if (-not (Test-DentroDeUsag $e.ou_destino)) { throw "ou_destino '$($e.ou_destino)' no está dentro de $OuBase." }
        $datos = Get-Atributos $e
        if ($simular -and $datos.Pendiente) { Write-Log "$quien [WhatIf] aviso: $($datos.Pendiente)" }
        $cuenta = Find-Cuenta $e

        if (-not $cuenta) {
            if (-not $e.habilitada) { Write-Log "$quien no existe en AD y está inactivo en RecursosHumanos; se omite."; continue }
            $accion = if ($e.cuenta_existente_m365) { 'empatar' } else { 'crear' }
            if ($simular) { Write-Log "$quien [WhatIf] $accion cuenta en $($e.ou_destino)"; continue }
            $cuenta = New-Cuenta $e $datos
            $acciones += $accion
        }
        else {
            Assert-CuentaModificable $cuenta
            $accion = 'actualizar'
            if ($simular) { Write-Log "$quien [WhatIf] actualizar atributos; limpiar: $($datos.Limpiar -join ', ')" }
            else {
                $cambios = @{}
                if ($datos.Reemplazar.Count) { $cambios.Replace = $datos.Reemplazar }
                if ($datos.Limpiar.Count) { $cambios.Clear = $datos.Limpiar }
                Set-ADUser -Identity $cuenta.ObjectGUID @cambios
            }
            $acciones += $accion

            if ($e.habilitada -ne $cuenta.Enabled) {
                $accion = if ($e.habilitada) { 'habilitar' } else { 'deshabilitar' }
                if ($simular) { Write-Log "$quien [WhatIf] $accion" }
                elseif ($e.habilitada) { Enable-ADAccount -Identity $cuenta.ObjectGUID }
                else { Disable-ADAccount -Identity $cuenta.ObjectGUID }
                $acciones += $accion
            }

            if (($cuenta.DistinguishedName -replace '^(?:[^,\\]|\\.)+,', '') -ne $e.ou_destino) {
                $accion = 'mover'
                if ($simular) { Write-Log "$quien [WhatIf] mover de $($cuenta.DistinguishedName) a $($e.ou_destino)" }
                else { Move-ADObject -Identity $cuenta.ObjectGUID -TargetPath $e.ou_destino }
                $acciones += $accion
            }
            if ($simular) { continue }
            $cuenta = Get-ADUser -Identity $cuenta.ObjectGUID
        }

        if ($datos.Pendiente) { throw $datos.Pendiente }
        foreach ($a in $acciones) { Register-Sincronizacion $e.id_empleado $a $true $null $cuenta }
        Write-Log "$quien OK: $($acciones -join ', ')"
        $correctos++
    }
    catch {
        $fallidos++
        $mensaje = $_.Exception.Message
        if ($acciones) { $mensaje = "Aplicado: $($acciones -join ', '). Error: $mensaje" }
        Write-Log "$quien ERROR ($accion): $mensaje"
        if (-not $simular) {
            try { Register-Sincronizacion $e.id_empleado $accion $false $mensaje $cuenta }
            catch { Write-Log "$quien no se pudo registrar el error en la base: $($_.Exception.Message)" }
        }
    }
}

Write-Log "Fin: $(@($empleados).Count) por sincronizar, $correctos correctos, $fallidos con error."
exit [int]($fallidos -gt 0)

