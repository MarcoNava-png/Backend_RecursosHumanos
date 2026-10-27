$ErrorActionPreference = 'Stop'
$src = Join-Path $PSScriptRoot 'src'
$entidades = Join-Path $src 'RecursosHumanos.Domain\Entities'
$contexto = Join-Path $src 'RecursosHumanos.Infrastructure\Persistence\RecursosHumanosDbContext.cs'
$utf8 = New-Object System.Text.UTF8Encoding $true

function Edit-Texto([string] $Ruta, [string] $Antes, [string] $Despues) {
    $texto = [IO.File]::ReadAllText($Ruta)
    if (-not $texto.Contains($Antes)) { throw "No se encontró en ${Ruta}: $Antes" }
    [IO.File]::WriteAllText($Ruta, $texto.Replace($Antes, $Despues), $utf8)
}

dotnet build (Join-Path $src 'RecursosHumanos.Api')
if ($LASTEXITCODE -ne 0) { throw 'La solución no compila; corrígela antes de regenerar' }

if (Test-Path $entidades) { Remove-Item $entidades -Recurse -Force }

dotnet ef dbcontext scaffold --no-build 'Name=ConnectionStrings:RecursosHumanos' Npgsql.EntityFrameworkCore.PostgreSQL `
    --project (Join-Path $src 'RecursosHumanos.Infrastructure') --startup-project (Join-Path $src 'RecursosHumanos.Api') `
    --schema recursos_humanos --schema nomina --context RecursosHumanosDbContext `
    --context-dir Persistence --context-namespace RecursosHumanos.Infrastructure.Persistence `
    --output-dir ../RecursosHumanos.Domain/Entities --namespace RecursosHumanos.Domain.Entities `
    --no-onconfiguring --no-pluralize --force
if ($LASTEXITCODE -ne 0) { throw 'Falló el scaffold' }

Get-ChildItem $entidades -Filter 'TbBi*.cs' | Rename-Item -NewName { $_.Name.Substring(4) }
foreach ($archivo in @(Get-ChildItem $entidades -Filter '*.cs') + (Get-Item $contexto)) {
    $texto = [IO.File]::ReadAllText($archivo.FullName)
    [IO.File]::WriteAllText($archivo.FullName, ($texto -creplace '\bTbBi(?=[A-Z])', ''), $utf8)
}

Edit-Texto (Join-Path $entidades 'Empleados.cs') `
    'public virtual HistorialSueldos? HistorialSueldos { get; set; }' `
    'public virtual ICollection<HistorialSueldos> HistorialSueldos { get; set; } = new List<HistorialSueldos>();'
Edit-Texto $contexto 'WithOne(p => p.HistorialSueldos)' 'WithMany(p => p.HistorialSueldos)'
Edit-Texto $contexto '.HasForeignKey<HistorialSueldos>(d => d.IdEmpleado)' '.HasForeignKey(d => d.IdEmpleado)'

dotnet build (Join-Path $PSScriptRoot 'RecursosHumanos.slnx')

