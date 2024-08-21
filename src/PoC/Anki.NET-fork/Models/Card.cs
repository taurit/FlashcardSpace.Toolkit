namespace Anki.NET.Models;

internal class Card
{
    public Card(Note note, string deckId)
    {
        Id = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var modificationTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        Query = @"INSERT INTO cards VALUES(" + Id + ", " + note.NoteId + ", " + deckId + ", " + "0, " +
                modificationTimestampSeconds + ", -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '');";
    }

    private long Id { get; }
    internal string Query { get; }
}
