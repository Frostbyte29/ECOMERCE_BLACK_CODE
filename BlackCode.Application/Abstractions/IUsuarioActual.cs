namespace BlackCode.Application.Abstractions;

/// <summary>
/// Quién está haciendo la petición. ISP: solo lo que los servicios necesitan, nada más.
/// La implementación lee la cookie de sesión y vive en BlackCode.Web.
/// </summary>
public interface IUsuarioActual
{
    int? UsuarioId { get; }
    bool EstaAutenticado { get; }
    bool EsAdmin { get; }
}
