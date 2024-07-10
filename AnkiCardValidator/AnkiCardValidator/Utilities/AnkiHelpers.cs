using AnkiCardValidator.ViewModels;
using PropertyChanged;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AnkiCardValidator.Utilities;

/// <remarks>
/// When renaming properties, remember to rename in Scriban template(s), too!
/// </remarks>
[AddINotifyPropertyChangedInterface]
public record AnkiNote([property: JsonIgnore] long Id, string FrontSide, string BackSide, string Tags)
{
    [JsonIgnore]
    public string Tags { get; set; } = Tags;
}

/// <summary>
/// Allows to read relevant data from Anki's SQLite database and returns them as .NET objects.
/// </summary>
public static class AnkiHelpers
{
    /// <summary>
    /// Returns Anki notes which have at least one card in specified Anki deck.
    /// </summary>
    /// <param name="databaseFilePath">A filesystem path to a SQL database of Anki (typically named `collection.anki2`)</param>
    /// <param name="deckName">Name of the deck serving as a filter for which flashcards to retrieve.</param>
    /// <param name="numCardsToFetchLimit">A number of cards to fetch. This is mostly to limit data for development purposes. The subset of cards is not that relevant in testing. `null` means no limit.</param>
    public static List<AnkiNote> GetAllNotesFromSpecificDeck(string databaseFilePath, string deckName, int? numCardsToFetchLimit = null)
    {
        using var connection = new SQLiteConnection($"Data Source={databaseFilePath};Version=3;");
        connection.Open();

        string limitString = numCardsToFetchLimit is null ? "" : $"LIMIT {numCardsToFetchLimit}";

        var query = $@"
                SELECT DISTINCT
                    notes.id, notes.flds, notes.tags
                FROM
                    cards
                JOIN
                    notes
                ON
                    cards.nid = notes.id
                WHERE
                    cards.did = (SELECT id FROM decks WHERE name COLLATE NOCASE = '{deckName}')
                    
                {limitString}
            ";

        // AND notes.tags LIKE '%hiszpanski-fajowe-znalezione-fiszki-z-audio%'

        using var command = new SQLiteCommand(query, connection);
        using var reader = command.ExecuteReader();

        var flashcards = new List<AnkiNote>();
        while (reader.Read())
        {
            var noteId = reader.GetInt64(0);
            var fields = reader.GetString(1).Split('\x1f');
            var tags = reader.GetString(2);

            // quick and hackish way to recognize fields in the card and what they mean (only in PoC)

            // My typical deck, including Spanish
            var wordInLearnedLanguage = fields[2];
            var wordInUserNativeLanguage = fields[0];

            var ankiNote = new AnkiNote(noteId, wordInLearnedLanguage, wordInUserNativeLanguage, tags);
            flashcards.Add(ankiNote);
        }

        return flashcards;
    }


    /// <summary>
    /// Parses tags from a string residing in `notes.tags`. Tags are separated by a space. Also, there is a leading space at the beginning, and a trailing space at the end of the string (most likely to simplify SQL queries, so they can use LIKE `%tag%` syntax).
    /// </summary>
    public static HashSet<string> ParseTags(string tagsString)
    {
        return tagsString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }

    public static void AddTagToNotes(string ankiDatabasePath, List<FlashcardViewModel> notesWithNoPenalty, string tagToAdd)
    {
        foreach (var noteVm in notesWithNoPenalty)
        {
            var note = noteVm.Note;

            if (note.Tags.Contains($" {tagToAdd} ")) continue; // already has the tag

            // my convention for marking cards as fit for learning feed
            if (note.Tags.Contains($" qa ")) continue;

            // a hack to support my convention of renaming tags in Anki and adding suffixes for subsequent batches 
            if (note.Tags.Contains($" opportunity")) continue; // hack: legacy convention of tagging with opportunity[NUMBER_OF_BATCH]

            var tagsAfterAdding = AddTagToAnkiTagsString(tagToAdd, note.Tags);

            // update tags string for the current note in the Anki database
            using var connection = new SQLiteConnection($"Data Source={ankiDatabasePath};Version=3;");
            connection.Open();

            var query = $@"
                UPDATE notes
                SET tags = '{tagsAfterAdding}'
                WHERE id = {note.Id};";

            using var command = new SQLiteCommand(query, connection);
            var numRowsAffected = command.ExecuteNonQuery();

            if (numRowsAffected != 1)
            {
                throw new InvalidOperationException($"Expected to update exactly one row, but updated {numRowsAffected} rows.");
            }

            noteVm.Note.Tags = tagsAfterAdding;
        }

        Debug.WriteLine("Finished adding tags");
    }

    /// <summary>
    /// Adds a tag to a string of tags. The tag is added only if it's not already there.
    /// Anki's tags are separated by a space. Also, there is a leading space at the beginning, and a trailing space at the end of the string (most likely to simplify SQL queries, so they can use LIKE `%tag%` syntax).
    /// Tags itself must not contain spaces and special characters, this method should throw exception if they do.
    /// </summary>
    /// <param name="tagToAdd">A tag to validate and add</param>
    /// <param name="tagsString">Existing tags</param>
    /// <returns></returns>
    public static string AddTagToAnkiTagsString(string tagToAdd, string tagsString)
    {
        if (tagToAdd.Contains(' ') || tagToAdd.Contains('%'))
        {
            throw new ArgumentException("Tag must not contain spaces or special characters.");
        }

        if (tagsString.Contains($" {tagToAdd} ")) return tagsString; // already has the tag

        var newTags = ParseTags(tagsString).Append(tagToAdd).Distinct();
        var newTagsString = $" {string.Join(' ', newTags)} ";
        return newTagsString;
    }

}



