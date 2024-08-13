using Microsoft.Extensions.Configuration;

namespace AnkiCardValidator;
public class Settings
{
    //public const string OpenAiModelId = "gpt-4o-mini"; // cheap and simplified model for dev purposes

    // https://openai.com/api/pricing/
    public const string OpenAiModelId = "gpt-4o-2024-08-06"; // newest snapshot of the model - cheaper than the last one
    public const string OpenAiModelGenerationId = "gpt-4o"; // this is for use in cache filenames, so responses from gpt-4o and gpt-4o-yyyy-mm-dd are considered the same model

    // hardcoded for simplicity in the proof-of-concept phase
    public const string AnkiDatabaseFilePathDev = "d:\\Projekty\\AnkiStoryGenerator\\LocalDevData\\collection.anki2";
    public const string AnkiDatabaseFilePath = "c:\\Users\\windo\\AppData\\Roaming\\Anki2\\Usuario 1\\collection.anki2";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string EvaluateQualitySpanishToPolishPromptPath = "D:\\Projekty\\AnkiCardValidator\\AnkiCardValidator\\AnkiCardValidator\\Prompts\\EvaluateCardQualityBatchSpanishToPolish.sbn";
    public const string EvaluateQualityPolishToSpanishPromptPath = "D:\\Projekty\\AnkiCardValidator\\AnkiCardValidator\\AnkiCardValidator\\Prompts\\EvaluateCardQualityBatchPolishToSpanish.sbn";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string GptResponseCacheDirectory = "s:\\Caches\\AnkiCardValidatorGptResponseCache\\";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string FrequencyDictionarySpanish = "d:\\Projekty\\AnkiCardValidator\\LocalDevData\\es_full.txt";
    public const string FrequencyDictionaryPolish = "d:\\Projekty\\AnkiCardValidator\\LocalDevData\\pl_full.txt";

    public readonly string OpenAiDeveloperKey;
    public readonly string OpenAiOrganization;
    public readonly string GeminiApiKey;

    public Settings()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Settings>();
        var configuration = builder.Build();
        OpenAiDeveloperKey = configuration["OpenAiDeveloperKey"] ??
                             throw new InvalidOperationException("OpenAiDeveloperKey is missing in User Secrets configuration");
        OpenAiOrganization = configuration["OpenAiOrganization"] ??
                             throw new InvalidOperationException("OpenAiOrganization is missing in User Secrets configuration");
        GeminiApiKey = configuration["GeminiApiKey"] ??
                       throw new InvalidOperationException("GeminiApiKey is missing in User Secrets configuration");
    }


    public const string QualityIssuesIgnoreListFilePath = "s:\\Caches\\AnkiCardValidatorIgnoreList.txt";
}
