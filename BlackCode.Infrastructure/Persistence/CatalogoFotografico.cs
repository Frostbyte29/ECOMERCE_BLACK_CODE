namespace BlackCode.Infrastructure.Persistence;

public record ProductoFotografico(string Nombre, string Slug, decimal Precio, string Color, string CodigoHex, string Descripcion, bool Destacado);

// Datos de demostración editables: cada entrada representa un diseño, no una toma fotográfica.
public static class CatalogoFotografico
{
    public static IReadOnlyList<ProductoFotografico> Productos { get; } =
    [
        new("Anillo Carmesí", "anillo-carmesi", 69m, "Rojo y plateado", "#8B2635", "Frente ovalado rojo con marco labrado y relieves laterales de inspiración clásica.", true),
        new("Anillo Obsidiana", "anillo-obsidiana", 65m, "Negro y plateado", "#555555", "Frente ovalado negro, borde trenzado y laterales con detalles geométricos.", true),
        new("Anillo Hexagrama", "anillo-hexagrama", 59m, "Plateado", "#C0C0C0", "Sello circular con estrella de seis puntas en relieve sobre fondo oscuro.", false),
        new("Anillo Celta", "anillo-celta", 59m, "Plateado", "#C0C0C0", "Diseño de cruz entrelazada con relieves redondeados y contrastes oscuros.", false),
        new("Anillo Brújula", "anillo-brujula", 65m, "Plateado", "#C0C0C0", "Rosa de los vientos en el frente y motivos náuticos grabados en los laterales.", true),
        new("Anillo Osario", "anillo-osario", 55m, "Plateado", "#C0C0C0", "Banda ancha con una sucesión de pequeñas calaveras y acabado de efecto envejecido.", false),
        new("Anillo Escudo", "anillo-escudo", 55m, "Plateado", "#C0C0C0", "Sello en forma de escudo con cruz central y contornos marcados.", false),
        new("Anillo Aries", "anillo-aries", 75m, "Plateado", "#C0C0C0", "Cabeza de carnero con cuernos curvos en relieve. Una pieza de volumen escultórico.", false),
        new("Anillo Dragón", "anillo-dragon", 79m, "Plateado", "#C0C0C0", "Figura de dragón con textura de escamas, curvas abiertas y detalles en relieve.", false),
        new("Anillo Prisma", "anillo-prisma", 59m, "Plateado", "#C0C0C0", "Frente circular con un motivo de diamante geométrico y borde ornamentado.", false),
        new("Anillo Imperio", "anillo-imperio", 75m, "Negro y dorado", "#9E8548", "Frente ovalado negro con borde dorado y laterales de textura cuadriculada.", true),
        new("Anillo Medianoche", "anillo-medianoche", 49m, "Negro", "#111111", "Sello cuadrado negro de líneas limpias, superficie lisa y esquinas redondeadas.", false),
        new("Anillo Roseta", "anillo-roseta", 65m, "Plateado", "#C0C0C0", "Medallón circular de filigrana con grabados curvos en el cuerpo del anillo.", false),
        new("Anillo Templario", "anillo-templario", 55m, "Plateado", "#C0C0C0", "Cruz de brazos anchos sobre una cara circular de fondo oscuro.", false),
        new("Anillo Memento", "anillo-memento", 69m, "Plateado", "#C0C0C0", "Calavera alargada con relieves laterales y acabado de efecto envejecido.", false),
        new("Anillo Azabache", "anillo-azabache", 69m, "Negro y plateado", "#555555", "Frente ovalado negro facetado con marco ornamental y cuerpo labrado.", false),
        new("Anillo Cobra", "anillo-cobra", 75m, "Plateado", "#C0C0C0", "Serpiente enrollada con textura de escamas y cabeza elevada.", false),
        new("Anillo Náutico", "anillo-nautico", 59m, "Plateado", "#C0C0C0", "Ancla sobre un medallón ovalado, borde de cuerda y laterales texturizados.", false),
        new("Anillo Equilibrio", "anillo-equilibrio", 59m, "Negro y plateado", "#555555", "Símbolo yin yang en el frente y motivos curvos grabados en los laterales.", false)
    ];
}
