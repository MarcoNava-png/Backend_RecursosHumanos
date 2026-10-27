using RecursosHumanos.Application.Common.Interfaces;

namespace RecursosHumanos.Infrastructure;

public class Reloj : IReloj
{
    public static readonly TimeZoneInfo ZonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    public DateTime AhoraUtc => DateTime.UtcNow;

    public DateOnly Hoy => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaHoraria));

    public DateTime InicioDelDiaUtc(DateOnly fecha) => TimeZoneInfo.ConvertTimeToUtc(fecha.ToDateTime(TimeOnly.MinValue), ZonaHoraria);
}
