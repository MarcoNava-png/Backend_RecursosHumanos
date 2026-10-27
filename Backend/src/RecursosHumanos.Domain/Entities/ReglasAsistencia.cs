using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class ReglasAsistencia
{
    public int IdReglaAsistencia { get; set; }

    public string NombreRegla { get; set; } = null!;

    public int MinutosTolerancia { get; set; }

    public int MinutosLimiteFalta { get; set; }

    public int MaximoRetardosPermitidos { get; set; }

    public bool AplicaSancionFalta { get; set; }

    public int IdPeriodoAcumulacion { get; set; }

    public int IdModoChecada { get; set; }

    public DateOnly FechaInicioVigencia { get; set; }

    public DateOnly? FechaFinVigencia { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ModosChecada IdModoChecadaNavigation { get; set; } = null!;

    public virtual PeriodosAcumulacion IdPeriodoAcumulacionNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<HistorialHorarios> HistorialHorarios { get; set; } = new List<HistorialHorarios>();
}
