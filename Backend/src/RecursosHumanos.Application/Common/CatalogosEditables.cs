using Microsoft.EntityFrameworkCore;
using RecursosHumanos.Application.Common.Exceptions;
using RecursosHumanos.Application.Common.Interfaces;
using RecursosHumanos.Domain.Entities;

namespace RecursosHumanos.Application.Common;

public static class CatalogosEditables
{
    public static async Task CambiarActivoAsync<T>(this IRecursosHumanosDbContext db, DbSet<T> conjunto, int id, bool activo,
        string catalogo, Func<Task<bool>>? enUso, CancellationToken ct) where T : class, IActivable
    {
        var registro = await conjunto.FindAsync([id], ct) ?? throw Errores.Catalogos.NoEncontrado(catalogo, id);
        if (!activo && enUso is not null && await enUso()) throw Errores.Catalogos.EnUso(catalogo);
        registro.Activo = activo;
        await db.SaveChangesAsync(ct);
    }

    public static async Task<T> BuscarAsync<T>(this DbSet<T> conjunto, int id, string catalogo, CancellationToken ct) where T : class =>
        await conjunto.FindAsync([id], ct) ?? throw Errores.Catalogos.NoEncontrado(catalogo, id);
}
