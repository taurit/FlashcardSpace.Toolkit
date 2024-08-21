using Anki.NET.Helpers;
using Anki.NET.Models.Scriban;
using Scriban;

namespace Anki.NET.Models;

internal class Collection
{
    private const long Id = 1;

    public Collection(AnkiDeckModel ankiDeckModel, string css)
    {
        DeckId = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        var modificationTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        var crt = GetDayStart();


        var conf = BuildConfigurationOptionsJson(ankiDeckModel);
        var models = BuildNoteModelsJson(ankiDeckModel, css, modificationTimeSeconds);
        var decksJson = BuildDecksConfigJson(ankiDeckModel, DeckId);
        var decksConfigurationsJson = GeneralHelper.ReadResource("AnkiNet.AnkiData.dconf.scriban-txt");

        Query = @"INSERT INTO col VALUES(" + Id + ", " + crt + ", " + DeckId + ", " + DeckId + ", 11, 0, 0, 0, '"
                + conf + "', '" + models + "', '" + decksJson + "', '" + decksConfigurationsJson + "', "
                + "'{}'" + ");";
    }

    internal string Query { get; }
    internal string DeckId { get; }

    private static string BuildConfigurationOptionsJson(AnkiDeckModel ankiDeckModel)
    {
        var confTemplate = GeneralHelper.ReadResource("AnkiNet.AnkiData.conf.scriban-txt");
        var conf = Template.Parse(confTemplate).Render(new ConfScribanModel(ankiDeckModel.ModelId), member => member.Name);
        return conf;
    }

    private string BuildNoteModelsJson(AnkiDeckModel ankiDeckModel, string css, string modificationTimeSeconds)
    {
        var modelsFileContent = GeneralHelper.ReadResource("AnkiNet.AnkiData.models.scriban-txt");
        var fieldListJson = ankiDeckModel.FieldList.ToJson();
        var json = Template.Parse(modelsFileContent).Render(
            new NoteTypesModel(DeckId, modificationTimeSeconds, ankiDeckModel.ModelName, ankiDeckModel.ModelId, css, fieldListJson,
                ankiDeckModel.CardTemplatesJsonArray),
            member => member.Name);

        return json;
    }

    private string BuildDecksConfigJson(AnkiDeckModel ankiDeckModel, string modificationTimeMilliseconds)
    {
        var deckTemplate = GeneralHelper.ReadResource("AnkiNet.AnkiData.decks.scriban-txt");
        var decks = Template.Parse(deckTemplate).Render(
            new DecksScribanModel(ankiDeckModel.ModelName, DeckId, modificationTimeMilliseconds),
            member => member.Name);
        return decks;
    }

    private static long GetDayStart()
    {
        var dateOffset = DateTimeOffset.Now;
        var fourHoursSpan = new TimeSpan(4, 0, 0);
        dateOffset = dateOffset.Subtract(fourHoursSpan);
        dateOffset = new DateTimeOffset(dateOffset.Year, dateOffset.Month, dateOffset.Day, 0, 0, 0, dateOffset.Offset);
        dateOffset = dateOffset.Add(fourHoursSpan);
        return dateOffset.ToUnixTimeSeconds();
    }
}
