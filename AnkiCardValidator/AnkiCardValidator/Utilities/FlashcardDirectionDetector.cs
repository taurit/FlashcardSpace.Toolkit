using AnkiCardValidator.Models;

namespace AnkiCardValidator.Utilities;
public class FlashcardDirectionDetector(NormalFormProvider normalFormProvider, FrequencyDataProvider polishFrequencyDataProvider, FrequencyDataProvider spanishFrequencyDataProvider)
{
    public FlashcardDirection DetectDirectionOfACard(AnkiNote note)
    {
        var detectedDirectionOfACard = TryDetermineDirectionBasedOnAlphabet(note) ??
                                       TryDetermineDirectionBasedOnFrequencyDictionaryPresence(note) ??
                                       TryDetermineDirectionBasedOnFrequencyDictionaryPresenceOfWords(note)
                                       ;

        if (detectedDirectionOfACard.HasValue) return detectedDirectionOfACard.Value;

        throw new NotImplementedException($"Could not decide. Front={note.FrontSide}. Back={note.BackSide}");
    }

    private static FlashcardDirection? TryDetermineDirectionBasedOnAlphabet(AnkiNote note)
    {
        var isQuestionLikelyInSpanish = StringHelpers.IsStringLikelyInSpanishLanguage(note.FrontSide);
        var isQuestionLikelyInPolish = StringHelpers.IsStringLikelyInPolishLanguage(note.FrontSide);
        var isAnswerLikelyInSpanish = StringHelpers.IsStringLikelyInSpanishLanguage(note.BackSide);
        var isAnswerLikelyInPolish = StringHelpers.IsStringLikelyInPolishLanguage(note.BackSide);

        if (isQuestionLikelyInPolish && isAnswerLikelyInSpanish) return FlashcardDirection.QuestionInPolish;
        if (isQuestionLikelyInSpanish && isAnswerLikelyInPolish) return FlashcardDirection.QuestionInSpanish;
        if (isQuestionLikelyInPolish) return FlashcardDirection.QuestionInPolish;
        if (isQuestionLikelyInSpanish) return FlashcardDirection.QuestionInSpanish;
        if (isAnswerLikelyInPolish) return FlashcardDirection.QuestionInSpanish;
        if (isAnswerLikelyInSpanish) return FlashcardDirection.QuestionInPolish;

        return null;
    }

    private FlashcardDirection? TryDetermineDirectionBasedOnFrequencyDictionaryPresence(AnkiNote note)
    {
        var questionPositionInPolishFrequencyDictionary = polishFrequencyDataProvider.GetPosition(note.FrontSide);
        var questionPositionInSpanishFrequencyDictionary = spanishFrequencyDataProvider.GetPosition(note.FrontSide);
        var answerPositionInPolishFrequencyDictionary = polishFrequencyDataProvider.GetPosition(note.BackSide);
        var answerPositionInSpanishFrequencyDictionary = spanishFrequencyDataProvider.GetPosition(note.BackSide);

        if (questionPositionInPolishFrequencyDictionary.HasValue &&
            !questionPositionInSpanishFrequencyDictionary.HasValue &&
            answerPositionInSpanishFrequencyDictionary.HasValue &&
            !answerPositionInPolishFrequencyDictionary.HasValue)
            return FlashcardDirection.QuestionInPolish;

        if (!questionPositionInPolishFrequencyDictionary.HasValue &&
            questionPositionInSpanishFrequencyDictionary.HasValue &&
            !answerPositionInSpanishFrequencyDictionary.HasValue &&
            answerPositionInPolishFrequencyDictionary.HasValue)
            return FlashcardDirection.QuestionInSpanish;

        return null;
    }

    private FlashcardDirection? TryDetermineDirectionBasedOnFrequencyDictionaryPresenceOfWords(AnkiNote note)
    {
        var wordsInQuestion = normalFormProvider
            .GetNormalizedFormOfLearnedTermWithCache(note.FrontSide)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var wordsInAnswer = normalFormProvider
            .GetNormalizedFormOfLearnedTermWithCache(note.BackSide)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

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

        var questionSeemsInPolish = questionAveragePositionInPolishDictionary < questionAveragePositionInSpanishDictionary;
        var questionSeemsInSpanish = questionAveragePositionInPolishDictionary > questionAveragePositionInSpanishDictionary;
        var answerSeemsInPolish = answerAveragePositionInPolishDictionary < answerAveragePositionInSpanishDictionary;
        var answerSeemsInSpanish = answerAveragePositionInPolishDictionary > answerAveragePositionInSpanishDictionary;

        if (questionSeemsInPolish && answerSeemsInSpanish) return FlashcardDirection.QuestionInPolish;
        if (questionSeemsInSpanish && answerSeemsInPolish) return FlashcardDirection.QuestionInSpanish;
        if (questionSeemsInPolish) return FlashcardDirection.QuestionInPolish;
        if (questionSeemsInSpanish) return FlashcardDirection.QuestionInSpanish;
        if (answerSeemsInPolish) return FlashcardDirection.QuestionInSpanish;
        if (answerSeemsInSpanish) return FlashcardDirection.QuestionInPolish;

        return null;
    }

}
