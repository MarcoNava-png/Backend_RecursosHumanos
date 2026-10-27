# RecursosHumanos

Api en .NET 10 con base de datos PostgreSQL 18.

## Trabajar en tu equipo

1. Instala PostgreSQL 18 y el SDK de .NET 10.
2. Copia `Backend/src/RecursosHumanos.Api/appsettings.Development.ejemplo.json` como `appsettings.Development.json` en la misma carpeta y pon tus datos en `ConnectionStrings` y una clave de al menos 32 caracteres en `Jwt:Clave`.
3. Abre `Backend/RecursosHumanos.slnx` y ejecuta la Api.

Si la base de datos no existe, la Api la crea al arrancar con `BaseDatos/recursoshumanos.sql` (estructura, catálogos y datos de prueba). La documentación queda en `http://localhost:5112/scalar`.

Cuando cambies la estructura de la base, vuelve a generar el script y el modelo:

```powershell
.\BaseDatos\generar-script.ps1
.\Backend\scaffold.ps1
```

## Servidor de base de datos

1. Instala PostgreSQL 18.
2. En `postgresql.conf`: `listen_addresses = '*'`
3. En `pg_hba.conf`, permite la conexión solo desde la IP del servidor de la Api:
   ```
   host  recursoshumanos  recursoshumanos_app   192.168.1.30/32  scram-sha-256
   ```
4. Reinicia PostgreSQL y abre el puerto solo para esa IP:
   ```powershell
   New-NetFirewallRule -DisplayName 'PostgreSQL RecursosHumanos' -Direction Inbound -Protocol TCP -LocalPort 5432 -RemoteAddress 192.168.1.30 -Action Allow
   ```
5. Crea la base y los usuarios:
   ```powershell
   createdb -U postgres -E UTF8 -T template0 recursoshumanos
   psql -U postgres -d recursoshumanos -f recursoshumanos.sql
   psql -U postgres -d recursoshumanos -v clave_app='...' -f usuario_app.sql
   psql -U postgres -d recursoshumanos -v clave_sync='...' -f usuario_sincronizacion.sql
   ```
   `usuario_app.sql` crea el usuario con el que se conecta la Api. Si después se agregan tablas, se vuelve a correr.

## Servidor de la Api (Windows Server)

En tu equipo genera el paquete:

```powershell
.\Backend\publicar.ps1
```

Copia la carpeta `Backend\publicado` al servidor y en PowerShell como administrador:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\instalar-servidor.ps1 -ServidorBd 192.168.1.20 -Origenes 'http://localhost:4200' -Documentacion
```

| Parámetro | Uso |
|---|---|
| `-ServidorBd` | IP o nombre del servidor de PostgreSQL |
| `-Origenes` | Direcciones desde donde corre el frontend |
| `-Documentacion` | Habilita Scalar y Swagger |
| `-Destino` | Carpeta de instalación (por defecto `D:\RecursosHumanos`) |
| `-Puerto` | Puerto de la Api (por defecto 5000) |

Queda instalado el servicio de Windows "RecursosHumanos", que arranca con el servidor, y se abre el puerto en el firewall. La primera vez pide la contraseña de `recursoshumanos_app` y genera `Api\appsettings.Production.json`.

Para actualizar se genera el paquete otra vez, se copia y se corre `.\instalar-servidor.ps1` sin parámetros; la configuración se conserva.

Para cambiar la configuración se edita `D:\RecursosHumanos\Api\appsettings.Production.json` y se corre `Restart-Service RecursosHumanos`. Los errores quedan en el Visor de eventos, en Aplicación, con origen RecursosHumanos.

## Frontend

- Documentación: `http://SERVIDOR:5000/scalar`. Por VPN debe estar abierto el puerto 5000.
- Login: `POST /api/auth/login` con `{ "nombreUsuario": "...", "contrasena": "..." }`. Regresa el token y la sesión con rol y permisos.
- El token va en cada petición en el encabezado `Authorization: Bearer <token>` y dura 8 horas.
- Los errores vienen como ProblemDetails, con `title` (mensaje) y `codigo` (por ejemplo `EMP-003`).
- Login con Microsoft: `GET /api/auth/entra/configuracion` da los datos para MSAL y el token de Microsoft se manda a `POST /api/auth/entra`.
- Si el frontend corre en otra dirección o puerto, hay que agregarla en `Cors:Origenes`.

La Api va por http porque solo se usa dentro de la VPN. Para publicarla en internet necesita HTTPS.
