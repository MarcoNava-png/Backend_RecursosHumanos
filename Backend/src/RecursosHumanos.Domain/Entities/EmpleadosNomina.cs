using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class EmpleadosNomina
{
    public int IdEmpleadoNomina { get; set; }

    public int IdEmpleado { get; set; }

    public int IdRegistroPatronal { get; set; }

    public int IdEsquemaPrestaciones { get; set; }

    public string? Nss { get; set; }

    public DateOnly? FechaAltaImss { get; set; }

    public int IdPeriodicidadPago { get; set; }

    public int IdTipoContrato { get; set; }

    public int IdTipoRegimen { get; set; }

    public int IdTipoJornada { get; set; }

    public int IdClaseRiesgoPuesto { get; set; }

    public bool Sindicalizado { get; set; }

    public bool ExcluidoPtu { get; set; }

    public int IdMetodoPago { get; set; }

    public int? IdBanco { get; set; }

    public string? Clabe { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Bancos? IdBancoNavigation { get; set; }

    public virtual ClasesRiesgo IdClaseRiesgoPuestoNavigation { get; set; } = null!;

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual EsquemasPrestaciones IdEsquemaPrestacionesNavigation { get; set; } = null!;

    public virtual MetodosPago IdMetodoPagoNavigation { get; set; } = null!;

    public virtual PeriodicidadesPago IdPeriodicidadPagoNavigation { get; set; } = null!;

    public virtual RegistrosPatronales IdRegistroPatronalNavigation { get; set; } = null!;

    public virtual TiposContrato IdTipoContratoNavigation { get; set; } = null!;

    public virtual TiposJornada IdTipoJornadaNavigation { get; set; } = null!;

    public virtual TiposRegimen IdTipoRegimenNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
