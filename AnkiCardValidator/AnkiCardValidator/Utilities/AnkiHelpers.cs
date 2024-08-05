using AnkiCardValidator.ViewModels;
using Microsoft.Data.Sqlite;

namespace AnkiCardValidator.Utilities;

/// <summary>
/// Allows to read relevant data from Anki's SQLite database and returns them as .NET objects.
/// </summary>
public static class AnkiHelpers
{
    /// <summary>
    /// Returns Anki notes which have at least one card in specified Anki deck.
    /// </summary>
    /// <param name="ankiDatabasePath">A filesystem path to a SQL database of Anki (typically named `collection.anki2`)</param>
    /// <param name="deckName">Name of the deck serving as a filter for which flashcards to retrieve.</param>
    public static List<AnkiNote> GetAllNotesFromSpecificDeck(string ankiDatabasePath, string deckName)
    {
        using var connection = new SqliteConnection($"Data Source={ankiDatabasePath};");
        connection.Open();
        connection.CreateCollation("unicase", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

        var query = $@"
                SELECT DISTINCT notes.id, notes.flds, notes.tags, notetypes.name
                FROM cards
                JOIN notes ON cards.nid = notes.id
                JOIN notetypes ON notes.mid = notetypes.id
                WHERE cards.did = (SELECT id FROM decks WHERE name COLLATE NOCASE = '{deckName}')
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

            var fieldsRaw = reader.GetString(1);

            if (templateName != "BothDirections" && templateName != "OneDirection")
                continue;

            var ankiNote = new AnkiNote(noteId, templateName, tags, fieldsRaw);
            flashcards.Add(ankiNote);
        }

        return flashcards;
    }


    public static int AddTagToNotes(string ankiDatabasePath, List<CardViewModel> cardsToTag, string tagToAdd)
    {
        // tradeoff for simplicity: it's enough that one card has no penalty to add the tag to the note
        // (even though the reverse of the card might have some penalty)
        using var connection = new SqliteConnection($"Data Source={ankiDatabasePath};");
        connection.Open();

        var numTaggedCards = 0;

        foreach (var cardVm in cardsToTag)
        {
            var note = cardVm.Note;

            if (note.Tags.Contains($" {tagToAdd} ")) continue; // already has the tag

            var tagsAfterAdding = AnkiTagHelpers.AddTagToAnkiTagsString(tagToAdd, note.Tags);

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

            cardVm.Note.Tags = tagsAfterAdding;
        }

        return numTaggedCards;
    }

    /// <summary>
    /// Carefully updates the "Comments" field (named "Examples" in Anki) in Anki database.
    /// I'm not implementing a generic method for updating fields yet because I want to test it on a small use case first.
    /// Anki stores values of fields in a single string, separated by ASCII 0x1f (Unit Separator) character, which requires
    /// being careful and escaping such character.
    /// </summary>
    public static void UpdateFields(string ankiDatabasePath, List<AnkiNote> notesToUpdate)
    {
        using var connection = new SqliteConnection($"Data Source={ankiDatabasePath};");
        connection.Open();
        connection.CreateCollation("unicase", (x, y) => string.Compare(x, y, StringComparison.OrdinalIgnoreCase));

        foreach (var noteToUpdate in notesToUpdate)
        {
            // Update the field in the Anki database
            var query = $@"
                UPDATE notes
                SET flds = '{noteToUpdate.FieldsRawCurrent}'
                WHERE id = {noteToUpdate.Id};";
            // Execute the query
            using var command = new SqliteCommand(query, connection);
            var numRowsAffected = command.ExecuteNonQuery();
            if (numRowsAffected != 1)
            {
                throw new InvalidOperationException($"Expected to update exactly one row, but updated {numRowsAffected} rows.");
            }
        }

    }

}
