param(
    [string] $Servidor = 'localhost',
    [int] $Puerto = 5432,
    [string] $Usuario = 'postgres',
    [string] $Base = 'recursoshumanos',
    [string] $Salida = (Join-Path $PSScriptRoot 'recursoshumanos.sql')
)

$ErrorActionPreference = 'Stop'
$pgDump = (Get-Command pg_dump -ErrorAction SilentlyContinue).Source
if (-not $pgDump) {
    $pgDump = Get-ChildItem 'C:\Program Files\PostgreSQL\*\bin\pg_dump.exe' | Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $pgDump) { throw 'No se encontró pg_dump. Instala PostgreSQL o agrégalo al PATH.' }

$temporal = [IO.Path]::GetTempFileName()
& $pgDump -h $Servidor -p $Puerto -U $Usuario -d $Base -f $temporal --encoding=UTF8 --no-owner --no-privileges --no-comments `
    --inserts --rows-per-insert=500 --exclude-table-data=recursos_humanos.tb_bi_auditoria_logs
if ($LASTEXITCODE -ne 0) { throw 'Falló pg_dump.' }

$lineas = [IO.File]::ReadAllLines($temporal, [Text.Encoding]::UTF8) | Where-Object { $_ -notmatch '^(--|\\restrict|\\unrestrict)' }
$texto = ($lineas -join "`n") -replace "`n{3,}", "`n`n"
$texto = $texto.Trim() + @'


DO $$
BEGIN
    EXECUTE format('ALTER DATABASE %I SET timezone TO %L', current_database(), 'America/Mexico_City');
    EXECUTE format('REVOKE CONNECT ON DATABASE %I FROM PUBLIC', current_database());
END
$$;

'@
[IO.File]::WriteAllText($Salida, $texto.Replace("`r`n", "`n"), (New-Object Text.UTF8Encoding $false))
Remove-Item $temporal
Write-Host "Listo: $Salida"
