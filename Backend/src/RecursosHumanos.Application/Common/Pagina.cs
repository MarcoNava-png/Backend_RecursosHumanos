namespace RecursosHumanos.Application.Common;

public record Pagina<T>(List<T> Elementos, int Total, int NumeroPagina, int Tamano);
