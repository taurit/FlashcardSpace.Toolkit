using AnkiCardValidator.ViewModels;
using Microsoft.Data.Sqlite;
using PropertyChanged;
using System.Diagnostics;

namespace AnkiCardValidator.Utilities;

[AddINotifyPropertyChangedInterface]
[DebuggerDisplay("{FrontText} -> {BackText}")]
public record AnkiNote(
    long Id,
    string NoteTemplateName,
    string Tags,
    string FrontText,
    string BackText,
    string FrontAudio,
    string BackAudio,
    string Image,
    string Comments
    )
{
    public string Tags { get; set; } = Tags;

    /// <summary>
    /// Tagged for removal by the duplicate detection flow.
    /// </summary>
    public bool IsScheduledForRemoval => Tags.Contains(" remove ");
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
        using var connection = new SqliteConnection($"Data Source={databaseFilePath};");
        connection.Open();
        connection.CreateCollation("unicase", (x, y) => String.Compare(x, y, StringComparison.OrdinalIgnoreCase));

        // Register the custom collation

        string limitString = numCardsToFetchLimit is null ? "" : $"LIMIT {numCardsToFetchLimit}";

        var query = $@"
                SELECT DISTINCT notes.id, notes.flds, notes.tags, notetypes.name
                FROM cards
                JOIN notes ON cards.nid = notes.id
                JOIN notetypes ON notes.mid = notetypes.id
                WHERE cards.did = (SELECT id FROM decks WHERE name COLLATE NOCASE = '{deckName}')
                {limitString}
            ";

        // AND notes.tags LIKE '%hiszpanski-fajowe-znalezione-fiszki-z-audio%'

        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        var flashcards = new List<AnkiNote>();
        while (reader.Read())
        {
            var noteId = reader.GetInt64(0);

            var tags = reader.GetString(2);
            var templateName = reader.GetString(3);

            // My typical deck, including Spanish
            var fields = reader.GetString(1).Split('\x1f');

            // works for BothDirections and OneDirection in my collection:
            var frontText = fields[0];
            var frontAudio = fields[2];
            var backText = fields[2];
            var backAudio = fields[3];
            var image = fields[4];
            var comments = fields[5];

            var ankiNote = new AnkiNote(noteId, templateName, tags, frontText, backText, frontAudio, backAudio, image, comments);
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

    public static int AddTagToNotes(string ankiDatabasePath, List<CardViewModel> cardsWithNoPenalty, string tagToAdd)
    {
        // tradeoff for simplicity: it's enough that one card has no penalty to add the tag to the note
        // (even though the reverse of the card might have some penalty)
        using var connection = new SqliteConnection($"Data Source={ankiDatabasePath};");
        connection.Open();

        int numTaggedCards = 0;

        foreach (var noteVm in cardsWithNoPenalty)
        {
            var note = noteVm.Note;

            if (note.Tags.Contains($" {tagToAdd} ")) continue; // already has the tag

            // my convention for marking cards as fit for learning feed
            if (note.Tags.Contains($" qa ")) continue;

            // a hack to support my convention of renaming tags in Anki and adding suffixes for subsequent batches 
            if (note.Tags.Contains($" opportunity")) continue; // hack: legacy convention of tagging with opportunity[NUMBER_OF_BATCH]

            var tagsAfterAdding = AddTagToAnkiTagsString(tagToAdd, note.Tags);

            // update tags string for the current note in the Anki database
            var query = $@"
                UPDATE notes
                SET tags = '{tagsAfterAdding}'
                WHERE id = {note.Id};";

            using var command = new SqliteCommand(query, connection);
            var numRowsAffected = command.ExecuteNonQuery();

            if (numRowsAffected != 1)
            {
                throw new InvalidOperationException($"Expected to update exactly one row, but updated {numRowsAffected} rows.");
            }
            numTaggedCards++;

            noteVm.Note.Tags = tagsAfterAdding;
        }

        return numTaggedCards;
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
