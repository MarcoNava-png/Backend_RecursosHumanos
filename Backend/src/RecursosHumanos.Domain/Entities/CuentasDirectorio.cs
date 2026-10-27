using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class CuentasDirectorio
{
    public int IdCuentaDirectorio { get; set; }

    public int IdEmpleado { get; set; }

    public string SamAccountName { get; set; } = null!;

    public string UserPrincipalName { get; set; } = null!;

    public string CorreoInstitucional { get; set; } = null!;

    public Guid? ObjectGuid { get; set; }

    public Guid? EntraObjectId { get; set; }

    public string? DistinguishedName { get; set; }

    public bool CuentaExistenteM365 { get; set; }

    public string? HashSincronizado { get; set; }

    public DateTime? FechaUltimaSincronizacion { get; set; }

    public string? UltimoError { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
