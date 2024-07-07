using System.Data.SQLite;
using System.Text.Json.Serialization;

namespace AnkiCardValidator.Utilities;

/// <remarks>
/// When renaming properties, remember to rename in Scriban template(s), too!
/// </remarks>
public record AnkiNote([property: JsonIgnore] long Id, string FrontSide, string BackSide, [property: JsonIgnore] HashSet<string> Tags);

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

        var query = $@"
                SELECT DISTINCT
                    notes.id, notes.flds, notes.tags
                FROM
                    cards
                JOIN
                    notes
                ON
                    cards.nid = notes.id
                JOIN
                    revlog
                ON
                    cards.id = revlog.cid
                WHERE
                    cards.did = (SELECT id FROM decks WHERE name COLLATE NOCASE = '{deckName}')
                ORDER BY
                    revlog.id DESC
                LIMIT {numCardsToFetchLimit}
            ";

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

            var tagsList = ParseTags(tags);

            var ankiNote = new AnkiNote(noteId, wordInLearnedLanguage, wordInUserNativeLanguage, tagsList);
            flashcards.Add(ankiNote);
        }

        return flashcards;
    }

    /// <summary>
    /// Parses tags from a string residing in `notes.tags`. Tags are separated by a space. Also, there is a leading space at the beginning, and a trailing space at the end of the string (most likely to simplify SQL queries, so they can use LIKE `%tag%` syntax).
    /// </summary>
    /// <param name="tagsString"></param>
    /// <returns></returns>
    public static HashSet<string> ParseTags(string tagsString)
    {
        return tagsString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
    }
}



