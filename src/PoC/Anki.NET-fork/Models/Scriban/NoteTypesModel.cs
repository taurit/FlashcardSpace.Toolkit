namespace Anki.NET.Models.Scriban;

internal record NoteTypesModel(string DeckId, string ModificationTimeSeconds, string ModelName, string ModelId, string Css,
    string FieldListJson, string CardTemplatesJsonArray);
