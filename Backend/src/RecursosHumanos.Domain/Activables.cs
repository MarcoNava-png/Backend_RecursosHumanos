namespace RecursosHumanos.Domain.Entities;

public interface IActivable
{
    bool Activo { get; set; }
}

public partial class Empresas : IActivable;
public partial class Campus : IActivable;
public partial class RegistrosPatronales : IActivable;
public partial class EsquemasPrestaciones : IActivable;
public partial class Departamentos : IActivable;
public partial class Puestos : IActivable;
public partial class Perfiles : IActivable;
public partial class DiasFestivos : IActivable;
public partial class Checadores : IActivable;
public partial class ReglasAsistencia : IActivable;
public partial class Materias : IActivable;
public partial class PeriodosEscolares : IActivable;
