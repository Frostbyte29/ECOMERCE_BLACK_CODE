# Inicia solamente la instancia de demostración preparada en esta computadora.
# En otra computadora, configura MySQL 8.4 siguiendo el README.
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot
$configPath = Join-Path $projectRoot '.local/database.json'
if (-not (Test-Path -LiteralPath $configPath)) {
    throw 'No hay configuración local. Usa tu servidor MySQL 8.4 y configura los secretos según docs/INICIO.md.'
}
$config = Get-Content -Raw -LiteralPath $configPath | ConvertFrom-Json
$server = Join-Path $config.Bin 'mysqld.exe'
if (-not (Test-Path -LiteralPath $server)) { throw 'No se encontró el servidor MySQL. Revisa .local/database.json.' }
if (Get-NetTCPConnection -LocalPort $config.Port -State Listen -ErrorAction SilentlyContinue) {
    Write-Output "El puerto $($config.Port) ya está ocupado; no se inició otra instancia."
    exit 0
}
$argsList = @('--no-defaults', ('--basedir="' + (Split-Path $config.Bin) + '"'),
    ('--datadir="' + $config.Data + '"'), ('--port=' + $config.Port),
    '--bind-address=127.0.0.1', '--mysqlx=0', '--console')
$logRoot = Join-Path $projectRoot '.local'
$process = Start-Process -FilePath $server -ArgumentList $argsList -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $logRoot 'mysql-out.log') -RedirectStandardError (Join-Path $logRoot 'mysql-error.log')
Write-Output "MySQL iniciado (PID $($process.Id)). Puerto local $($config.Port). Revisa los registros si la conexión falla."
