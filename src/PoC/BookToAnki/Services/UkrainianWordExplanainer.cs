using BookToAnki.NotePropertiesDatabase;
using BookToAnki.Services.OpenAi;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BookToAnki.Services;

public record WordToExplain(string Word, string Context);

[Serializable]
public record UkrainianWordExplanation(string NominativeForm, string PolishTranslation, string EnglishTranslation, string ExplanationInPolish, string ExplanationInEnglish);

public class UkrainianWordExplainer
{
    private readonly OpenAiServiceWrapper _openAiService;
    private readonly NoteProperties _noteProperties;

    public UkrainianWordExplainer(OpenAiServiceWrapper openAiService, NoteProperties noteProperties)
    {
        _openAiService = openAiService;
        _noteProperties = noteProperties;
    }

    public async Task BatchPrepareExplanations(IReadOnlyList<WordToExplain> words, int limit, Action updateMoneyCounterInUi)
    {
        var systemPrompt = await File.ReadAllTextAsync("Resources/ExplainUkrainianWordPromptTemplate.System.txt");

        var alreadyExplainedWordsInContext = _noteProperties.GetAlreadyExplainedByGpt4().ToHashSet();
        var alreadyExplainedWords = new HashSet<string>(alreadyExplainedWordsInContext.Select(x => x.Word), StringComparer.InvariantCultureIgnoreCase);

        var wordsNotYetExplained = words.Where(x => !alreadyExplainedWords.Contains(x.Word)).ToList();
        var wordsNotYetExplainedToExplain = wordsNotYetExplained.Take(limit).ToList();

        int chunkSize = 9;
        var chunks = wordsNotYetExplainedToExplain.Chunk(chunkSize).ToList();
        foreach (var chunk in chunks)
        {
            var inputSerialized = JsonSerializer.Serialize(chunk, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            var userPrompt = $"Here's the input:\n" +
                             $"```json\n" +
                             $"{inputSerialized}\n" +
                             $"```";
            var response = await _openAiService.CreateChatCompletion(systemPrompt, userPrompt, OpenAI.ObjectModels.Models.Gpt_4o, false);

            if (response is null) throw new ArgumentException("ChatGPT provided no valid response");

            var json = response.GetJsonFromChatGptResponse();
            List<UkrainianWordExplanation>? explanations = null;
            try
            {
                explanations = JsonSerializer.Deserialize<List<UkrainianWordExplanation>>(json);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine("Deserialization of a response from OpenAPI API failed.");
                Debug.WriteLine($"Error message: {ex.Message}, {ex.StackTrace}");
                Debug.WriteLine($"Content that failed to deserialize: {json}");
                Debug.WriteLine("Falling back to a manual resolver");

                // fallback: sometimes Json contain some syntax error, and the quickest way to fix is to do it manually:
                var jsonFromFallbackResolver = await _openAiService.CreateChatCompletion(systemPrompt, userPrompt, "manual", true);
                explanations = JsonSerializer.Deserialize<List<UkrainianWordExplanation>>(jsonFromFallbackResolver);
            }

            if (explanations is null) throw new ArgumentException("Unexpected response - could not deserialize");
            if (explanations.Count != chunk.Length) throw new ArgumentException($"Unexpected response - expected {chunkSize} answers, but got {explanations.Count}");

            int index = 0;
            foreach (var word in chunk)
            {
                var explanation = explanations[index];
                var keyW = new PrefKey(word.Word, "*");
                var keyWC = new PrefKey(word.Word, word.Context);

                if (alreadyExplainedWordsInContext.Contains(keyW) || alreadyExplainedWordsInContext.Contains(keyWC))
                {
                    throw new InvalidOperationException("Unexpected: input was specifically filtered out to not pay for the same word twice! debug.");
                }

                _noteProperties.SetWordNominativeOriginalGpt4(keyW, explanation.NominativeForm);
                _noteProperties.SetWordNominativeOriginalGpt4(keyWC, explanation.NominativeForm);

                _noteProperties.SetWordNominativePlGpt4(keyW, explanation.PolishTranslation);
                _noteProperties.SetWordNominativePlGpt4(keyWC, explanation.PolishTranslation);

                _noteProperties.SetWordNominativeEnGpt4(keyW, explanation.EnglishTranslation);
                _noteProperties.SetWordNominativeEnGpt4(keyWC, explanation.EnglishTranslation);

                _noteProperties.SetWordExplanationPlGpt4(keyW, explanation.ExplanationInPolish);
                _noteProperties.SetWordExplanationPlGpt4(keyWC, explanation.ExplanationInPolish);

                _noteProperties.SetWordExplanationEnGpt4(keyW, explanation.ExplanationInEnglish);
                _noteProperties.SetWordExplanationEnGpt4(keyWC, explanation.ExplanationInEnglish);

                Debug.WriteLine($"Saved explanation for '{word}': '{explanation.PolishTranslation}, {explanation.ExplanationInPolish}'");

                index++;
            }
            updateMoneyCounterInUi();
        }
    }

    internal UkrainianWordExplanation TryGetExplanation(PrefKey key, PrefKey wordScopedKey)
    {
        var nominativeForm = _noteProperties.GetWordNominativeOriginal(key) ??
                             _noteProperties.GetWordNominativeOriginal(wordScopedKey) ??
                             _noteProperties.GetWordNominativeOriginalGpt4(key) ??
                             _noteProperties.GetWordNominativeOriginalGpt4(wordScopedKey) ??
                             "[no data]";

        var polishTranslation = _noteProperties.GetWordNominativePl(key) ??
                                _noteProperties.GetWordNominativePl(wordScopedKey) ??
                                _noteProperties.GetWordNominativePlGpt4(key) ??
                                _noteProperties.GetWordNominativePlGpt4(wordScopedKey) ??
                                "[no data]";

        var englishTranslation = _noteProperties.GetWordNominativeEn(key) ??
                                 _noteProperties.GetWordNominativeEn(wordScopedKey) ??
                                 _noteProperties.GetWordNominativeEnGpt4(key) ??
                                 _noteProperties.GetWordNominativeEnGpt4(wordScopedKey) ??
                                 "[no data]";

        var polishExplanation = _noteProperties.GetWordExplanationPl(key) ??
                                _noteProperties.GetWordExplanationPl(wordScopedKey) ??
                                _noteProperties.GetWordExplanationPlGpt4(key) ??
                                _noteProperties.GetWordExplanationPlGpt4(wordScopedKey) ??
                                "[no data]";

        var englishExplanation = _noteProperties.GetWordExplanationEn(key) ??
                                 _noteProperties.GetWordExplanationEn(wordScopedKey) ??
                                 _noteProperties.GetWordExplanationEnGpt4(key) ??
                                 _noteProperties.GetWordExplanationEnGpt4(wordScopedKey) ??
                                 "[no data]";

        return new UkrainianWordExplanation(nominativeForm, polishTranslation, englishTranslation, polishExplanation, englishExplanation);
    }
}
