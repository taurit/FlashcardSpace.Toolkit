using AnkiCardValidator.Models;

namespace AnkiCardValidator.Utilities;
public class FlashcardDirectionDetector(NormalFormProvider normalFormProvider, FrequencyDataProvider polishFrequencyDataProvider, FrequencyDataProvider spanishFrequencyDataProvider)
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
        var wordsInQuestion = normalFormProvider.GetNormalizedFormOfLearnedTermWithCache(note.FrontSide).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var wordsInAnswer = normalFormProvider.GetNormalizedFormOfLearnedTermWithCache(note.BackSide).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var numQuestionWordsPresentInSpanishDictionary = wordsInQuestion.Count(
            x => spanishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var numQuestionWordsPresentInPolishDictionary = wordsInQuestion.Count(
            x => polishFrequencyDataProvider.GetPosition(x).HasValue
        );

        if (numQuestionWordsPresentInPolishDictionary != numQuestionWordsPresentInSpanishDictionary)
            return numQuestionWordsPresentInPolishDictionary > numQuestionWordsPresentInSpanishDictionary
                ? FlashcardDirection.QuestionInPolish
                : FlashcardDirection.QuestionInSpanish;
        var numAnswerWordsPresentInSpanishDictionary = wordsInAnswer.Count(
            x => spanishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var numAnswerWordsPresentInPolishDictionary = wordsInAnswer.Count(
            x => polishFrequencyDataProvider.GetPosition(x).HasValue
        );
        if (numAnswerWordsPresentInPolishDictionary != numAnswerWordsPresentInSpanishDictionary)
            return numAnswerWordsPresentInPolishDictionary > numAnswerWordsPresentInSpanishDictionary
                ? FlashcardDirection.QuestionInSpanish
                : FlashcardDirection.QuestionInPolish;

        // words present in both frequency dictionaries... compare their positions
        var questionAveragePositionInPolishDictionary = wordsInQuestion
            .Select(polishFrequencyDataProvider.GetPosition)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .DefaultIfEmpty(int.MaxValue)
            .Average();
        var answerAveragePositionInPolishDictionary = wordsInAnswer
            .Select(polishFrequencyDataProvider.GetPosition)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .DefaultIfEmpty(int.MaxValue)
            .Average();
        var questionAveragePositionInSpanishDictionary = wordsInQuestion
            .Select(spanishFrequencyDataProvider.GetPosition)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .DefaultIfEmpty(int.MaxValue)
            .Average();
        var answerAveragePositionInSpanishDictionary = wordsInAnswer
            .Select(spanishFrequencyDataProvider.GetPosition)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .DefaultIfEmpty(int.MaxValue)
            .Average();

        if (questionAveragePositionInPolishDictionary < answerAveragePositionInPolishDictionary)
        {
            if (questionAveragePositionInPolishDictionary < questionAveragePositionInSpanishDictionary)
                return FlashcardDirection.QuestionInPolish;
            else return FlashcardDirection.QuestionInSpanish;
        }
        else
        {
            if (answerAveragePositionInPolishDictionary < answerAveragePositionInSpanishDictionary)
                return FlashcardDirection.QuestionInSpanish;
            else return FlashcardDirection.QuestionInPolish;
        }

        throw new NotImplementedException($"Could not decide. Front={note.FrontSide}. Back={note.BackSide}");
    }
}
