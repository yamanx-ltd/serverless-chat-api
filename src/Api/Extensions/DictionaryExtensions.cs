namespace Api.Extensions;

public static class DictionaryExtensions
{
    public static string ToJson(this Dictionary<string, string?> dictionary)
    {
        return System.Text.Json.JsonSerializer.Serialize(dictionary);
    }
}