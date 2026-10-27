using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class MotivosBaja
{
    public int IdMotivoBaja { get; set; }

    public string DescripcionMotivo { get; set; } = null!;

    public bool AplicaFiniquito { get; set; }

    public int IdReglaPrimaAntiguedad { get; set; }

    public bool AplicaIndemnizacion90Dias { get; set; }

    public bool Aplica20DiasPorAnio { get; set; }

    public int? IdCausaBajaImss { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual CausasBajaImss? IdCausaBajaImssNavigation { get; set; }

    public virtual ReglasPrimaAntiguedad IdReglaPrimaAntiguedadNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<BajasLaborales> BajasLaborales { get; set; } = new List<BajasLaborales>();
}
