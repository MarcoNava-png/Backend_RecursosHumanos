using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class HistorialHorarios
{
    public int IdHistorialHorario { get; set; }

    public int IdEmpleado { get; set; }

    public int? IdMateria { get; set; }

    public int? IdPeriodo { get; set; }

    public int IdReglaAsistencia { get; set; }

    public short DiaSemana { get; set; }

    public TimeOnly HoraEntrada { get; set; }

    public TimeOnly HoraSalida { get; set; }

    public TimeOnly? HoraComidaInicio { get; set; }

    public TimeOnly? HoraComidaFin { get; set; }

    public DateOnly FechaInicioVigencia { get; set; }

    public DateOnly? FechaFinVigencia { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empleados IdEmpleadoNavigation { get; set; } = null!;

    public virtual Materias? IdMateriaNavigation { get; set; }

    public virtual PeriodosEscolares? IdPeriodoNavigation { get; set; }

    public virtual ReglasAsistencia IdReglaAsistenciaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<AsistenciaDiaria> AsistenciaDiaria { get; set; } = new List<AsistenciaDiaria>();

    public virtual ICollection<Suplencias> Suplencias { get; set; } = new List<Suplencias>();
}
