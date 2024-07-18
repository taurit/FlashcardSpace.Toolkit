using AnkiCardValidator.ViewModels;

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
    public List<CardViewModel> DetectDuplicatesInQuestion(CardViewModel flashcard, IEnumerable<CardViewModel> allCards)
    {
        return allCards
            .Where(otherCard =>
                otherCard != flashcard &&
                otherCard.CardDirectionFlag == flashcard.CardDirectionFlag &&
                _duplicateDetectionEqualityComparer.Equals(otherCard.Question, flashcard.Question)
            )
            .ToList();
    }

}
