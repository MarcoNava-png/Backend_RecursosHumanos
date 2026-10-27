\if :{?clave_app}
\else
    \echo 'Falta la contraseña: agrega -v clave_app=... al comando psql'
    \quit
\endif

BEGIN;

SELECT format('CREATE ROLE recursoshumanos_app LOGIN PASSWORD %L', :'clave_app')
 WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'recursoshumanos_app') \gexec
SELECT format('ALTER ROLE recursoshumanos_app PASSWORD %L', :'clave_app') \gexec

GRANT CONNECT, TEMPORARY ON DATABASE :"DBNAME" TO recursoshumanos_app;
GRANT USAGE ON SCHEMA recursos_humanos, nomina TO recursoshumanos_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA recursos_humanos, nomina TO recursoshumanos_app;
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA recursos_humanos, nomina TO recursoshumanos_app;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA recursos_humanos, nomina TO recursoshumanos_app;
REVOKE UPDATE, DELETE ON recursos_humanos.tb_bi_auditoria_logs FROM recursoshumanos_app;
REVOKE EXECUTE ON FUNCTION recursos_humanos.fn_registrar_sincronizacion(INT, TEXT, BOOLEAN, TEXT, UUID, TEXT) FROM PUBLIC, recursoshumanos_app;

ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA recursos_humanos, nomina
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO recursoshumanos_app;
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA recursos_humanos, nomina
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO recursoshumanos_app;

COMMIT;
