using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class PeriodosNomina
{
    public int IdPeriodoNomina { get; set; }

    public int IdEmpresa { get; set; }

    public int IdTipoNomina { get; set; }

    public int IdPeriodicidadPago { get; set; }

    public int Ejercicio { get; set; }

    public int NumeroPeriodo { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public DateOnly FechaPago { get; set; }

    public decimal DiasPago { get; set; }

    public string EstatusCierre { get; set; } = null!;

    public int? IdAutorizadorNomina { get; set; }

    public DateTime? FechaAutorizacion { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados? IdAutorizadorNominaNavigation { get; set; }

    public virtual Empresas IdEmpresaNavigation { get; set; } = null!;

    public virtual PeriodicidadesPago IdPeriodicidadPagoNavigation { get; set; } = null!;

    public virtual TiposNomina IdTipoNominaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<IncidenciasNomina> IncidenciasNomina { get; set; } = new List<IncidenciasNomina>();

    public virtual ICollection<Nominas> Nominas { get; set; } = new List<Nominas>();

    public virtual ICollection<Ptu> Ptu { get; set; } = new List<Ptu>();
}
