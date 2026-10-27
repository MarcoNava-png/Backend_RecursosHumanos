namespace RecursosHumanos.Application.Common.Exceptions;

public static partial class Errores
{
    public static class Creditos
    {
        public static NoEncontradoException NoEncontrado(string tipo, int id) =>
            new("CRE-001", $"No existe el {tipo} {id}.");

        public static ReglaNegocioException TipoDescuentoNoAplica() =>
            new("CRE-002", "Ese tipo de descuento no aplica para este crédito.");

        public static ReglaNegocioException ConPagos() =>
            new("CRE-003", "Ya tiene descuentos aplicados en nómina; no se puede cancelar.");

        public static ReglaNegocioException ConceptoNoValido() =>
            new("CRE-004", "El concepto no es válido para este registro.");

        public static ReglaNegocioException FechaFinInvalida() =>
            new("CRE-005", "La fecha de fin no puede ser anterior a la de inicio.");
    }
}
