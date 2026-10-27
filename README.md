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

## Frontend

- Documentación: `http://SERVIDOR:5000/scalar`. Por VPN debe estar abierto el puerto 5000.
- Login: `POST /api/auth/login` con `{ "nombreUsuario": "...", "contrasena": "..." }`. Regresa el token y la sesión con rol y permisos.
- El token va en cada petición en el encabezado `Authorization: Bearer <token>` y dura 8 horas.
- Los errores vienen como ProblemDetails, con `title` (mensaje) y `codigo` (por ejemplo `EMP-003`).
- Login con Microsoft: `GET /api/auth/entra/configuracion` da los datos para MSAL y el token de Microsoft se manda a `POST /api/auth/entra`.
- Si el frontend corre en otra dirección o puerto, hay que agregarla en `Cors:Origenes`.

La Api va por http porque solo se usa dentro de la VPN. Para publicarla en internet necesita HTTPS.
