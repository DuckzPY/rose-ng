using System.Text.Json;

namespace RoseNG.Core
{
    public static class JsonFormat
    {
        /// <summary>
        /// Pretty-prints a raw JSON string for display in an output box. Falls
        /// back to the original text if it isn't valid JSON (an error message).
        /// </summary>
        public static string Pretty(string rawJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawJson);
                return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return rawJson;
            }
        }
    }
}
