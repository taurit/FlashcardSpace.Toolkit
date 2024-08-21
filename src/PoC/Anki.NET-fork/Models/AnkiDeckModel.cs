using Anki.NET.Models.Scriban;
using System.Text.Json;

namespace Anki.NET.Models;

public class AnkiDeckModel
{
    public string ModelId { get; }
    public string ModelName { get; }
    public FieldList FieldList { get; }
    public string ShortUniquePrefixForMediaFiles { get; }
    public string CardTemplatesJsonArray { get; }

    public AnkiDeckModel(string modelName, FieldList fieldList, CardTemplate[] cardTemplates, string shortUniquePrefixForMediaFiles)
    {
        if (cardTemplates.Length == 0) throw new ArgumentException("You need to have at least 1 card template in the deck");

        ModelName = modelName;
        FieldList = fieldList;
        ShortUniquePrefixForMediaFiles = shortUniquePrefixForMediaFiles;
        ModelId = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        CardTemplatesJsonArray = JsonSerializer.Serialize(cardTemplates);
    }
}
