namespace RecursosHumanos.Application.Common.Interfaces;

public interface IAlmacenArchivos
{
    int MaximoMb { get; }

    Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct);
    Stream Abrir(string ruta);
}
