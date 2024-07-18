using AnkiCardValidator.ViewModels;
using System.Collections.ObjectModel;

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
    public void DetectDuplicatesInQuestion(ObservableCollection<CardViewModel> allCards)
    {
        foreach (var card in allCards)
        {
            card.DuplicatesOfQuestion.Clear();
        }

        for (int i = 0; i < allCards.Count; i++)
        {
            var card = allCards[i];
            for (int j = i + 1; j < allCards.Count; j++)
            {
                var otherCard = allCards[j];
                if (card.IsQuestionInPolish == otherCard.IsQuestionInPolish && _duplicateDetectionEqualityComparer.Equals(card.Question, otherCard.Question))
                {
                    card.DuplicatesOfQuestion.Add(otherCard);
                    otherCard.DuplicatesOfQuestion.Add(card);
                }
            }
        }
    }

}
