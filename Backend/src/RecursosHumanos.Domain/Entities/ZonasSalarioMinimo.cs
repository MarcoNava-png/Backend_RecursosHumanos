using System;
using System.Collections.Generic;

namespace RecursosHumanos.Domain.Entities;

public partial class ZonasSalarioMinimo
{
    public int IdZonaSalarioMinimo { get; set; }

    public string Clave { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int IdTipoIndicador { get; set; }

    public int IdUsuarioCreacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int? IdUsuarioModificacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }

    public virtual TiposIndicador IdTipoIndicadorNavigation { get; set; } = null!;

    public virtual Usuarios IdUsuarioCreacionNavigation { get; set; } = null!;

    public virtual Usuarios? IdUsuarioModificacionNavigation { get; set; }

    public virtual ICollection<RegistrosPatronales> RegistrosPatronales { get; set; } = new List<RegistrosPatronales>();
}
