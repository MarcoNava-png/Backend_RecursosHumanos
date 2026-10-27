using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class RegistrosPatronales
{
    public int IdRegistroPatronal { get; set; }

    public int IdEmpresa { get; set; }

    public string RegistroPatronal { get; set; } = null!;

    public int IdClaseRiesgo { get; set; }

    public int IdZonaSalarioMinimo { get; set; }

    public int IdEstado { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual ClasesRiesgo IdClaseRiesgoNavigation { get; set; } = null!;

    public virtual Empresas IdEmpresaNavigation { get; set; } = null!;

    public virtual Estados IdEstadoNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ZonasSalarioMinimo IdZonaSalarioMinimoNavigation { get; set; } = null!;

    public virtual ICollection<EmpleadosNomina> EmpleadosNomina { get; set; } = new List<EmpleadosNomina>();

    public virtual ICollection<PrimasRiesgoTrabajo> PrimasRiesgoTrabajo { get; set; } = new List<PrimasRiesgoTrabajo>();
}
