namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Horarios
    {
        public static NoEncontradoException NoEncontrado(int id) =>
            new("HOR-001", $"No existe el horario {id}.");

        public static ReglaNegocioException Traslape() =>
            new("HOR-002", "El horario se cruza con otro vigente del mismo día.");
    }
}
