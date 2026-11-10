namespace BlackCode.Application.Features.Administracion;

// OCP: los formatos se registran como estrategias; el servicio de carga no cambia.
public interface IFormatoImagenProducto
{
    IReadOnlyList<string> Extensiones { get; }
    string ExtensionAlmacenamiento { get; }
    bool EsValida(byte[] datos);
}

public sealed class FormatoImagenJpeg : IFormatoImagenProducto
{
    public IReadOnlyList<string> Extensiones => [".jpg", ".jpeg"];
    public string ExtensionAlmacenamiento => ".jpg";
    public bool EsValida(byte[] datos) => datos.Length >= 20 && datos[0] == 0xff && datos[1] == 0xd8
        && datos[2] == 0xff && datos[^2] == 0xff && datos[^1] == 0xd9;
}

public sealed class FormatoImagenPng : IFormatoImagenProducto
{
    public IReadOnlyList<string> Extensiones => [".png"];
    public string ExtensionAlmacenamiento => ".png";
    public bool EsValida(byte[] datos) => datos.Length >= 45
        && datos.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
        && datos.AsSpan(12, 4).SequenceEqual("IHDR"u8)
        && datos.AsSpan(datos.Length - 8, 4).SequenceEqual("IEND"u8);
}

public sealed class FormatoImagenWebp : IFormatoImagenProducto
{
    public IReadOnlyList<string> Extensiones => [".webp"];
    public string ExtensionAlmacenamiento => ".webp";
    public bool EsValida(byte[] datos) => datos.Length >= 20 && datos.AsSpan(0, 4).SequenceEqual("RIFF"u8)
        && datos.AsSpan(8, 4).SequenceEqual("WEBP"u8)
        && System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(datos.AsSpan(4, 4)) == datos.Length - 8;
}
