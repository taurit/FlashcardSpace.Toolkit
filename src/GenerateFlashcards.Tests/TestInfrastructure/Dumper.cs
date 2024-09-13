using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GenerateFlashcards.Tests.TestInfrastructure;

/// <summary>
/// Source: https://stackoverflow.com/a/2699811/889779
/// </summary>
public static class Dumper
{
    private static string ToPrettyString(this object value)
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };
        return JsonConvert.SerializeObject(value, settings);
    }

    public static T Dump<T>(this T value) where T : class
    {
        Console.WriteLine(value.ToPrettyString());
        return value;
    }
}
