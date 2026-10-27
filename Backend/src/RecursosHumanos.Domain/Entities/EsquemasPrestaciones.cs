using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class EsquemasPrestaciones
{
    public int IdEsquemaPrestaciones { get; set; }

    public int IdEmpresa { get; set; }

    public string NombreEsquema { get; set; } = null!;

    public int DiasAguinaldo { get; set; }

    public decimal PorcentajePrimaVacacional { get; set; }

    public decimal PorcentajePrimaDominical { get; set; }

    public bool DescontarFaltasAguinaldo { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual Empresas IdEmpresaNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<EmpleadosNomina> EmpleadosNomina { get; set; } = new List<EmpleadosNomina>();
}
