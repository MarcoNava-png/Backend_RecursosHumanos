using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class Empresas
{
    public int IdEmpresa { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string NombreComercial { get; set; } = null!;

    public string Rfc { get; set; } = null!;

    public int IdRegimenFiscal { get; set; }

    public string CodigoPostalExpedicion { get; set; } = null!;

    public string? NumeroCertificadoCsd { get; set; }

    public string? RutaCertificadoCsd { get; set; }

    public DateOnly? VigenciaCsd { get; set; }

    public string? ProveedorPac { get; set; }

    public string? SerieRecibos { get; set; }

    public int FolioSiguiente { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public string? DominioCorreo { get; set; }

    public string? OuDirectorio { get; set; }

    public virtual RegimenesFiscales IdRegimenFiscalNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<Campus> Campus { get; set; } = new List<Campus>();

    public virtual ICollection<Empleados> Empleados { get; set; } = new List<Empleados>();

    public virtual ICollection<EsquemasPrestaciones> EsquemasPrestaciones { get; set; } = new List<EsquemasPrestaciones>();

    public virtual ICollection<PeriodosNomina> PeriodosNomina { get; set; } = new List<PeriodosNomina>();

    public virtual ICollection<Ptu> Ptu { get; set; } = new List<Ptu>();

    public virtual ICollection<RegistrosPatronales> RegistrosPatronales { get; set; } = new List<RegistrosPatronales>();
}
