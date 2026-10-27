namespace RecursosHumanos.Application.Common.Interfaces;

public interface IGeneradorToken
{
    (string Token, DateTime ExpiraUtc) Generar(int idUsuario, int? idEmpleado, string nombreUsuario, string rol);
}
