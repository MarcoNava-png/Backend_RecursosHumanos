\if :{?clave_sync}
\else
    \echo 'Falta la contraseña: agrega -v clave_sync=... al comando psql'
    \quit
\endif

BEGIN;

SELECT format('CREATE ROLE recursoshumanos_sync LOGIN PASSWORD %L', :'clave_sync')
 WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'recursoshumanos_sync') \gexec
SELECT format('ALTER ROLE recursoshumanos_sync PASSWORD %L', :'clave_sync') \gexec

GRANT CONNECT ON DATABASE :"DBNAME" TO recursoshumanos_sync;
GRANT USAGE ON SCHEMA recursos_humanos TO recursoshumanos_sync;

GRANT SELECT ON recursos_humanos.vw_bi_directorio_empleados TO recursoshumanos_sync;

ALTER FUNCTION recursos_humanos.fn_registrar_sincronizacion(INT, TEXT, BOOLEAN, TEXT, UUID, TEXT)
    SECURITY DEFINER SET search_path = recursos_humanos, pg_temp;

REVOKE EXECUTE ON FUNCTION recursos_humanos.fn_registrar_sincronizacion(INT, TEXT, BOOLEAN, TEXT, UUID, TEXT) FROM PUBLIC;
GRANT  EXECUTE ON FUNCTION recursos_humanos.fn_registrar_sincronizacion(INT, TEXT, BOOLEAN, TEXT, UUID, TEXT) TO recursoshumanos_sync;

COMMIT;
