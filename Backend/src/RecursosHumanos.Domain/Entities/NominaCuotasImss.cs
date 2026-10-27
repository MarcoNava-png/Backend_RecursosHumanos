using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class NominaCuotasImss
{
    public int IdNominaCuota { get; set; }

    public int IdNomina { get; set; }

    public string ClaveRamo { get; set; } = null!;

    public decimal DiasCotizados { get; set; }

    public decimal Base { get; set; }

    public decimal CuotaPatron { get; set; }

    public decimal CuotaObrero { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Nominas IdNominaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }
}
