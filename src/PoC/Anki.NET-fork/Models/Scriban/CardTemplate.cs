using System.Text.Json.Serialization;

//"tmpls": [
//{
//    "name": "Forward",
//    "qfmt": "{{English}}",
//    "did": null,
//    "bafmt": "",
//    "afmt": "{{FrontSide}}\n<hr id=answer>{{Ukrainian}}",
//    "ord": 0,
//    "bqfmt": ""
//},
//{
//    "name": "Back",
//    "qfmt": "{{Ukrainian}}",
//    "did": null,
//    "bafmt": "",
//    "afmt": "{{FrontSide}}\n<hr id=answer>{{English}}",
//    "ord": 1,
//    "bqfmt": ""
//}
//],

namespace Anki.NET.Models.Scriban;

[Serializable]
public class CardTemplate
{
    public CardTemplate(int ordinalStartingWith0, string name, string qfmt, string afmt)
    {
        Name = name;
        Qfmt = qfmt;
        Afmt = afmt;
        Ord = ordinalStartingWith0;
    }

    /// <summary>
    /// Template name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; }

    /// <summary>
    /// Question format string
    /// </summary>
    [JsonPropertyName("qfmt")]
    public string Qfmt { get; init; }

    /// <summary>
    /// answer template string
    /// </summary>
    [JsonPropertyName("afmt")]
    public string Afmt { get; init; }

    /// <summary>
    /// Deck override (null by default)
    /// </summary>
    [JsonPropertyName("did")]
    public object Did => null;

    /// <summary>
    /// Browser answer format: used for displaying answer in browser
    /// </summary>
    [JsonPropertyName("bafmt")]
    public string Bafmt => "";


    /// <summary>
    /// template number
    /// </summary>
    [JsonPropertyName("ord")]
    public int Ord { get; init; }

    [JsonPropertyName("bqfmt")]
    public string Bqfmt => "";
}
