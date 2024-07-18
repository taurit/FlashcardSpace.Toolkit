namespace AnkiCardValidator.Utilities;

public class DuplicateDetector(NormalFormProvider normalFormProvider)
{
    private readonly DuplicateDetectionEqualityComparer _duplicateDetectionEqualityComparer = new(normalFormProvider);

    /// <summary>
    /// Heuristically detects possible duplicates in a list of notes.
    /// </summary>
    /// <returns>
    /// Notes with suspiciously similar content of the *front* side.
    /// </returns>
    public List<AnkiNote> DetectDuplicatesFront(AnkiNote flashcard, List<AnkiNote> allNotes)
    {
        return allNotes
            .Where(note => note != flashcard && _duplicateDetectionEqualityComparer.Equals(note.FrontText, flashcard.FrontText))
            .ToList();
    }

    public List<AnkiNote> DetectDuplicatesBack(AnkiNote flashcard, List<AnkiNote> allNotes)
    {
        return allNotes
            .Where(note => note != flashcard && _duplicateDetectionEqualityComparer.Equals(note.BackText, flashcard.BackText))
            .ToList();
    }
}
