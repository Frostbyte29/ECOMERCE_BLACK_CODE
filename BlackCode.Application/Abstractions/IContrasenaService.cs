namespace BlackCode.Application.Abstractions;

public enum ResultadoContrasena
{
    Fallido,
    Exitoso,
    ExitosoRehashRequerido   // la clave es correcta pero el hash usa un formato viejo: re-hashear y guardar
}

/// <summary>
/// SRP: hashear y verificar contraseñas es una responsabilidad aparte del login.
/// </summary>
public interface IContrasenaService
{
    string Hashear(string contrasena);
    ResultadoContrasena Verificar(string hashContrasena, string contrasenaProvista);
}
