namespace BlackCode.Domain.Configuracion;

// Configuración editable por el admin, ej. "Shipping:FlatCost" = "15.00"
public class ConfiguracionTienda
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
}
