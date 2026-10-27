using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class AuditoriaLogs
{
    public long IdLog { get; set; }

    public int IdUsuario { get; set; }

    public string TablaAfectada { get; set; } = null!;

    public string IdRegistro { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public string? JsonDatosPrevios { get; set; }

    public string? JsonDatosNuevos { get; set; }

    public DateTime FechaAccion { get; set; }

    public string? IpOrigen { get; set; }

    public virtual Usuarios IdUsuarioNavigation { get; set; } = null!;
}
