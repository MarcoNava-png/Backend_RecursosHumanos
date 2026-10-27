using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Constantes;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Infrastructure.Persistence;

public class AuditoriaInterceptor(IUsuarioActual usuarioActual, IReloj reloj) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        LlenarColumnas(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        LlenarColumnas(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void LlenarColumnas(DbContext? contexto)
    {
        if (contexto is null) return;
        var idUsuario = usuarioActual.IdUsuario ?? UsuarioSistema.Id;
        var ahora = reloj.AhoraUtc;

        foreach (var entrada in contexto.ChangeTracker.Entries())
        {
            if (entrada.Metadata.FindProperty(nameof(Empleados.IdUsuarioCreacion)) is null) continue;

            if (entrada.State == EntityState.Added)
            {
                entrada.Property(nameof(Empleados.IdUsuarioCreacion)).CurrentValue = idUsuario;
                entrada.Property(nameof(Empleados.FechaCreacion)).CurrentValue = ahora;
                entrada.Property(nameof(Empleados.Activo)).CurrentValue = true;
            }
            else if (entrada.State == EntityState.Modified)
            {
                entrada.Property(nameof(Empleados.IdUsuarioModificacion)).CurrentValue = idUsuario;
                entrada.Property(nameof(Empleados.FechaModificacion)).CurrentValue = ahora;
            }
        }
    }
}
