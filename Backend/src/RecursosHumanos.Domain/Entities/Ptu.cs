using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Ptu
{
    public int IdPtu { get; set; }

    public int IdEmpresa { get; set; }

    public int Ejercicio { get; set; }

    public decimal RentaGravable { get; set; }

    public decimal? MontoRepartir { get; set; }

    public decimal? TopeSalarioConfianza { get; set; }

    public int? IdPeriodoNomina { get; set; }

    public string Estatus { get; set; } = null!;

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empresas IdEmpresaNavigation { get; set; } = null!;

    public virtual PeriodosNomina? IdPeriodoNominaNavigation { get; set; }

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<PtuEmpleados> PtuEmpleados { get; set; } = new List<PtuEmpleados>();
}
