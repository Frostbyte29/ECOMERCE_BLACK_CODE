using System.ComponentModel.DataAnnotations;

namespace BlackCode.Web.ViewModels;

// Atributo de validación de servidor para campos bool que deben ser true (ej: aceptar términos).
// Al no implementar IClientModelValidator no genera atributos data-val en el HTML,
// lo que evita que jQuery Validate intente procesar un rango booleano sin adaptador y bloquee el submit.
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class MustBeTrueAttribute : ValidationAttribute
{
    public MustBeTrueAttribute()
        : base("Este campo es obligatorio.") { }

    public override bool IsValid(object? value) => value is true;
}
