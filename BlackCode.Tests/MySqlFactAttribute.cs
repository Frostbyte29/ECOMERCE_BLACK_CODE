namespace BlackCode.Tests;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MySqlFactAttribute : FactAttribute
{
    public MySqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BLACKCODE_TEST_CONNECTION_STRING")))
            Skip = "Requiere BLACKCODE_TEST_CONNECTION_STRING con una base de pruebas aislada.";
    }
}
