using System.Text.Json;

namespace Aspire.Hosting.AWS.LambdaEmulator;

public static class Utils
{
    public static string TryPrettyPrintJson(string data)
    {
        try
        {
            var doc = JsonDocument.Parse(data);
            var prettyPrintJson = System.Text.Json.JsonSerializer.Serialize(doc, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            return prettyPrintJson;
        }
        catch (Exception)
        {
            return data;
        }
    }
}
