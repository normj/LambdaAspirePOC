using System.Text.Json;

namespace Aspire.Hosting.AWS.LambdaServiceEmulator;

public static class Utils
{
    public static string TryPrettyPrintJson(string data)
    {
        try
        {
            var doc = JsonDocument.Parse(data);
            var prettyPrintJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions()
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
