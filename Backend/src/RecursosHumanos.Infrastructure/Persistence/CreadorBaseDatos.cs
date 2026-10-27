using Npgsql;

namespace RecursosHumanos.Infrastructure.Persistence;

public static class CreadorBaseDatos
{
    public static async Task<bool> CrearSiNoExisteAsync(string conexion, string rutaScript, CancellationToken ct = default)
    {
        var nombre = new NpgsqlConnectionStringBuilder(conexion).Database
            ?? throw new InvalidOperationException("La cadena de conexión no indica la base de datos.");
        var servidor = new NpgsqlConnectionStringBuilder(conexion) { Database = "postgres", Pooling = false }.ConnectionString;

        await using (var cn = new NpgsqlConnection(servidor))
        {
            await cn.OpenAsync(ct);
            await using var existe = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @nombre", cn);
            existe.Parameters.AddWithValue("nombre", nombre);
            if (await existe.ExecuteScalarAsync(ct) is not null) return false;
            await using var crear = new NpgsqlCommand($"CREATE DATABASE {Identificador(nombre)} ENCODING 'UTF8' TEMPLATE template0", cn);
            await crear.ExecuteNonQueryAsync(ct);
        }

        try
        {
            await using var bd = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(conexion) { Pooling = false }.ConnectionString);
            await bd.OpenAsync(ct);
            await using var script = new NpgsqlCommand(await File.ReadAllTextAsync(rutaScript, ct), bd) { CommandTimeout = 0 };
            await script.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            await using var cn = new NpgsqlConnection(servidor);
            await cn.OpenAsync(CancellationToken.None);
            await using var borrar = new NpgsqlCommand($"DROP DATABASE IF EXISTS {Identificador(nombre)} WITH (FORCE)", cn);
            await borrar.ExecuteNonQueryAsync(CancellationToken.None);
            throw;
        }
        return true;
    }

    private static string Identificador(string nombre) => $"\"{nombre.Replace("\"", "\"\"")}\"";
}
