namespace RecursosHumanos.Domain.Constantes;

public static class Permiso
{
    public const string EmpleadosVer = "empleados.ver";
    public const string EmpleadosEditar = "empleados.editar";
    public const string AsistenciaVer = "asistencia.ver";
    public const string AsistenciaRegistrarChecadas = "asistencia.registrar_checadas";
    public const string AsistenciaProcesar = "asistencia.procesar";
    public const string HorariosVer = "horarios.ver";
    public const string HorariosEditar = "horarios.editar";
    public const string SolicitudesVer = "solicitudes.ver";
    public const string SolicitudesCrearOtros = "solicitudes.crear_otros";
    public const string SolicitudesAprobar = "solicitudes.aprobar";
    public const string VacacionesVer = "vacaciones.ver";
    public const string VacacionesGenerarSaldos = "vacaciones.generar_saldos";
    public const string IncapacidadesVer = "incapacidades.ver";
    public const string IncapacidadesEditar = "incapacidades.editar";
    public const string SuplenciasVer = "suplencias.ver";
    public const string SuplenciasAsignar = "suplencias.asignar";
    public const string SuplenciasAprobar = "suplencias.aprobar";
    public const string NominaVer = "nomina.ver";
    public const string NominaCalcular = "nomina.calcular";
    public const string NominaAutorizar = "nomina.autorizar";
    public const string NominaCerrar = "nomina.cerrar";
    public const string NominaDatosVer = "nomina.datos_ver";
    public const string NominaDatosEditar = "nomina.datos_editar";
    public const string BajasVer = "bajas.ver";
    public const string BajasEditar = "bajas.editar";
    public const string BajasPagar = "bajas.pagar";
    public const string PtuVer = "ptu.ver";
    public const string PtuEditar = "ptu.editar";
    public const string CreditosVer = "creditos.ver";
    public const string CreditosEditar = "creditos.editar";
    public const string CatalogosEditar = "catalogos.editar";
    public const string AuditoriaVer = "auditoria.ver";
    public const string RolesEditar = "roles.editar";
    public const string UsuariosVer = "usuarios.ver";
    public const string UsuariosEditar = "usuarios.editar";
}

public static class ClaveRol
{
    public const string Empleado = "empleado";
}

public static class Alcance
{
    public const string Todos = "todos";
    public const string Equipo = "equipo";
    public const string Propio = "propio";
}
