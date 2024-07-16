using AnkiCardValidator.Models;

namespace AnkiCardValidator.Utilities;
public class FlashcardDirectionDetector(FrequencyDataProvider polishFrequencyDataProvider, FrequencyDataProvider spanishFrequencyDataProvider)
{
    public FlashcardDirection DetectDirectionOfACard(AnkiNote note)
    {
        var isQuestionLikelyInSpanish = StringHelpers.IsStringLikelyInSpanishLanguage(note.FrontSide);
        var isQuestionLikelyInPolish = StringHelpers.IsStringLikelyInPolishLanguage(note.FrontSide);
        var isAnswerLikelyInSpanish = StringHelpers.IsStringLikelyInSpanishLanguage(note.BackSide);
        var isAnswerLikelyInPolish = StringHelpers.IsStringLikelyInPolishLanguage(note.BackSide);

        if (isQuestionLikelyInPolish && isAnswerLikelyInSpanish)
        {
            // most certain case
            return FlashcardDirection.QuestionInPolish;
        }

        if (isQuestionLikelyInSpanish && isAnswerLikelyInPolish)
        {
            // most certain case
            return FlashcardDirection.QuestionInSpanish;
        }

        if (isQuestionLikelyInPolish)
            return FlashcardDirection.QuestionInPolish;

        if (isQuestionLikelyInSpanish)
            return FlashcardDirection.QuestionInSpanish;

        if (isAnswerLikelyInPolish)
            return FlashcardDirection.QuestionInSpanish;

        if (isAnswerLikelyInSpanish)
            return FlashcardDirection.QuestionInPolish;

        var questionPositionInPolishFrequencyDictionary = polishFrequencyDataProvider.GetPosition(note.FrontSide);
        var questionPositionInSpanishFrequencyDictionary = spanishFrequencyDataProvider.GetPosition(note.FrontSide);
        var answerPositionInPolishFrequencyDictionary = polishFrequencyDataProvider.GetPosition(note.BackSide);
        var answerPositionInSpanishFrequencyDictionary = spanishFrequencyDataProvider.GetPosition(note.BackSide);

        if (questionPositionInPolishFrequencyDictionary.HasValue && answerPositionInSpanishFrequencyDictionary.HasValue)
            return FlashcardDirection.QuestionInPolish;

        if (questionPositionInSpanishFrequencyDictionary.HasValue && answerPositionInPolishFrequencyDictionary.HasValue)
            return FlashcardDirection.QuestionInSpanish;

        if (questionPositionInPolishFrequencyDictionary.HasValue) return FlashcardDirection.QuestionInPolish;
        if (questionPositionInSpanishFrequencyDictionary.HasValue) return FlashcardDirection.QuestionInSpanish;
        if (answerPositionInPolishFrequencyDictionary.HasValue) return FlashcardDirection.QuestionInSpanish;
        if (answerPositionInSpanishFrequencyDictionary.HasValue) return FlashcardDirection.QuestionInPolish;

        // also use frequency dictionary heuristic if a term has more than 1 word...
        var wordsInQuestion = note.FrontSide.Split(' ');
        var wordsInAnswer = note.BackSide.Split(' ');
        var allWordsInQuestionArePresentInPolishDictionary = wordsInQuestion.All(
            x => polishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var allWordsInQuestionArePresentInSpanishDictionary = wordsInQuestion.All(
            x => spanishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var allWordsInAnswerArePresentInPolishDictionary = wordsInAnswer.All(
            x => polishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var allWordsInAnswerArePresentInSpanishDictionary = wordsInAnswer.All(
            x => spanishFrequencyDataProvider.GetPosition(x).HasValue
        );
        if (allWordsInQuestionArePresentInPolishDictionary && allWordsInAnswerArePresentInSpanishDictionary)
            return FlashcardDirection.QuestionInPolish;
        if (allWordsInQuestionArePresentInSpanishDictionary && allWordsInAnswerArePresentInPolishDictionary)
            return FlashcardDirection.QuestionInSpanish;
        if (allWordsInQuestionArePresentInPolishDictionary)
            return FlashcardDirection.QuestionInPolish;
        if (allWordsInQuestionArePresentInSpanishDictionary)
            return FlashcardDirection.QuestionInSpanish;
        if (allWordsInAnswerArePresentInSpanishDictionary)
            return FlashcardDirection.QuestionInPolish;
        if (allWordsInAnswerArePresentInPolishDictionary)
            return FlashcardDirection.QuestionInSpanish;

        throw new NotImplementedException($"Could not decide. Front={note.FrontSide}. Back={note.BackSide}");
    }
}
