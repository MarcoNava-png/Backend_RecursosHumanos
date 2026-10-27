namespace RecursosHumanos.Application.Common.Interfaces;

public interface IReloj
{
    DateTime AhoraUtc { get; }

    DateOnly Hoy { get; }

    DateTime InicioDelDiaUtc(DateOnly fecha);
}
