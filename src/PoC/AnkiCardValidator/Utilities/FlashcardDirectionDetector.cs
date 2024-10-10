using AnkiCardValidator.Models;
using AnkiCardValidator.ViewModels;
using CoreLibrary.Services;
using CoreLibrary.Utilities;

namespace AnkiCardValidator.Utilities;
public class FlashcardDirectionDetector(StringSanitizer stringSanitizer, FrequencyDataProvider polishFrequencyDataProvider, FrequencyDataProvider spanishFrequencyDataProvider)
{
    public FlashcardDirection DetectDirectionOfACard(AnkiNote note)
    {
        var detectedDirectionOfACard = TryDetermineDirectionBasedOnAlphabetAndCommonWords(note) ??
                                       TryDetermineDirectionBasedOnFrequencyDictionaryPresence(note) ??
                                       TryDetermineDirectionBasedOnFrequencyDictionaryPresenceOfWords(note)
                                       ;

        if (detectedDirectionOfACard.HasValue) return detectedDirectionOfACard.Value;

        throw new NotImplementedException($"Could not decide. Front={note.FrontText}. Back={note.BackText}");
    }

    private static FlashcardDirection? TryDetermineDirectionBasedOnAlphabetAndCommonWords(AnkiNote note)
    {
        var isQuestionLikelyInPolish = note.FrontText.IsStringLikelyInPolishLanguage();
        var isAnswerLikelyInSpanish = note.BackText.IsStringLikelyInSpanishLanguage();
        if (isQuestionLikelyInPolish && isAnswerLikelyInSpanish) return FlashcardDirection.FrontTextInPolish;

        var isQuestionLikelyInSpanish = note.FrontText.IsStringLikelyInSpanishLanguage();
        var isAnswerLikelyInPolish = note.BackText.IsStringLikelyInPolishLanguage();

        if (isQuestionLikelyInSpanish && isAnswerLikelyInPolish) return FlashcardDirection.FrontTextInSpanish;
        if (isQuestionLikelyInPolish) return FlashcardDirection.FrontTextInPolish;
        if (isQuestionLikelyInSpanish) return FlashcardDirection.FrontTextInSpanish;
        if (isAnswerLikelyInPolish) return FlashcardDirection.FrontTextInSpanish;
        if (isAnswerLikelyInSpanish) return FlashcardDirection.FrontTextInPolish;

        return null;
    }

    private FlashcardDirection? TryDetermineDirectionBasedOnFrequencyDictionaryPresence(AnkiNote note)
    {
        var questionPositionInPolishFrequencyDictionary = polishFrequencyDataProvider.GetPosition(note.FrontText);
        var questionPositionInSpanishFrequencyDictionary = spanishFrequencyDataProvider.GetPosition(note.FrontText);
        var answerPositionInPolishFrequencyDictionary = polishFrequencyDataProvider.GetPosition(note.BackText);
        var answerPositionInSpanishFrequencyDictionary = spanishFrequencyDataProvider.GetPosition(note.BackText);

        if (questionPositionInPolishFrequencyDictionary.HasValue &&
            !questionPositionInSpanishFrequencyDictionary.HasValue &&
            answerPositionInSpanishFrequencyDictionary.HasValue &&
            !answerPositionInPolishFrequencyDictionary.HasValue)
            return FlashcardDirection.FrontTextInPolish;

        if (!questionPositionInPolishFrequencyDictionary.HasValue &&
            questionPositionInSpanishFrequencyDictionary.HasValue &&
            !answerPositionInSpanishFrequencyDictionary.HasValue &&
            answerPositionInPolishFrequencyDictionary.HasValue)
            return FlashcardDirection.FrontTextInSpanish;

        return null;
    }

    private FlashcardDirection? TryDetermineDirectionBasedOnFrequencyDictionaryPresenceOfWords(AnkiNote note)
    {
        var wordsInQuestion = stringSanitizer
            .GetNormalizedFormOfLearnedTermWithCache(note.FrontText)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var wordsInAnswer = stringSanitizer
            .GetNormalizedFormOfLearnedTermWithCache(note.BackText)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var numQuestionWordsPresentInSpanishDictionary = wordsInQuestion.Count(
            x => spanishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var numQuestionWordsPresentInPolishDictionary = wordsInQuestion.Count(
            x => polishFrequencyDataProvider.GetPosition(x).HasValue
        );

        if (numQuestionWordsPresentInPolishDictionary != numQuestionWordsPresentInSpanishDictionary)
            return numQuestionWordsPresentInPolishDictionary > numQuestionWordsPresentInSpanishDictionary
                ? FlashcardDirection.FrontTextInPolish
                : FlashcardDirection.FrontTextInSpanish;
        var numAnswerWordsPresentInSpanishDictionary = wordsInAnswer.Count(
            x => spanishFrequencyDataProvider.GetPosition(x).HasValue
        );
        var numAnswerWordsPresentInPolishDictionary = wordsInAnswer.Count(
            x => polishFrequencyDataProvider.GetPosition(x).HasValue
        );
        if (numAnswerWordsPresentInPolishDictionary != numAnswerWordsPresentInSpanishDictionary)
            return numAnswerWordsPresentInPolishDictionary > numAnswerWordsPresentInSpanishDictionary
                ? FlashcardDirection.FrontTextInSpanish
                : FlashcardDirection.FrontTextInPolish;

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

        if (questionSeemsInPolish && answerSeemsInSpanish) return FlashcardDirection.FrontTextInPolish;
        if (questionSeemsInSpanish && answerSeemsInPolish) return FlashcardDirection.FrontTextInSpanish;
        if (questionSeemsInPolish) return FlashcardDirection.FrontTextInPolish;
        if (questionSeemsInSpanish) return FlashcardDirection.FrontTextInSpanish;
        if (answerSeemsInPolish) return FlashcardDirection.FrontTextInSpanish;
        if (answerSeemsInSpanish) return FlashcardDirection.FrontTextInPolish;

        return null;
    }

}
