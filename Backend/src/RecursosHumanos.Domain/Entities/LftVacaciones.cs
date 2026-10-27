using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class LftVacaciones
{
    public int IdLftVacacion { get; set; }

    public int AnioAntiguedad { get; set; }

    public int DiasDeLey { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<SaldosVacaciones> SaldosVacaciones { get; set; } = new List<SaldosVacaciones>();
}
