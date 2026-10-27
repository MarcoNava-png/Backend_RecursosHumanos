using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RecursosHumanos.Application.Common.Interfaces;

namespace RecursosHumanos.Infrastructure.Persistence;

public class UsuarioConexionInterceptor(IUsuarioActual usuario) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData) =>
        Comando(connection).ExecuteNonQuery();

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default) =>
        await Comando(connection).ExecuteNonQueryAsync(cancellationToken);

    private DbCommand Comando(DbConnection connection)
    {
        var comando = connection.CreateCommand();
        comando.CommandText = "SELECT set_config('app.id_usuario', @usuario, false), set_config('app.ip_origen', @ip, false)";
        Parametro(comando, "usuario", usuario.IdUsuario?.ToString() ?? "");
        Parametro(comando, "ip", usuario.Ip ?? "");
        return comando;
    }

    private static void Parametro(DbCommand comando, string nombre, string valor)
    {
        var parametro = comando.CreateParameter();
        parametro.ParameterName = nombre;
        parametro.Value = valor;
        comando.Parameters.Add(parametro);
    }
}
