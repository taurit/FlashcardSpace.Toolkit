using Microsoft.Extensions.Configuration;

namespace AnkiCardValidator;
public class Settings
{
    //public const string OpenAiModelId = "gpt-3.5-turbo";
    public const string OpenAiModelId = "gpt-4o";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string AnkiDatabaseFilePathDev = "d:\\Projekty\\AnkiStoryGenerator\\LocalDevData\\collection.anki2";
    public const string AnkiDatabaseFilePath = "c:\\Users\\windo\\AppData\\Roaming\\Anki2\\Usuario 1\\collection.anki2";

    public const string AnkiMediaFolderPath = "c:\\Users\\windo\\AppData\\Roaming\\Anki2\\Usuario 1\\collection.media\\";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string EvaluateCardQualityPromptPath = "d:\\Projekty\\AnkiCardValidator\\AnkiCardValidator\\AnkiCardValidator\\Prompts\\EvaluateCardQuality.sbn";
    public const string EvaluateCardQualityBatchPromptPath = "d:\\Projekty\\AnkiCardValidator\\AnkiCardValidator\\AnkiCardValidator\\Prompts\\EvaluateCardQualityBatch.sbn";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string GptResponseCacheDirectory = "s:\\Caches\\AnkiCardValidatorGptResponseCache\\";

    // hardcoded for simplicity in the proof-of-concept phase
    public const string FrequencyDictionarySpanish = "d:\\Projekty\\AnkiCardValidator\\LocalDevData\\es_full.txt";
    public const string FrequencyDictionaryPolish = "d:\\Projekty\\AnkiCardValidator\\LocalDevData\\pl_full.txt";

    public readonly string OpenAiDeveloperKey;
    public readonly string OpenAiOrganization;

    public Settings()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Settings>();
        var configuration = builder.Build();
        OpenAiDeveloperKey = configuration["OpenAiDeveloperKey"] ??
                             throw new InvalidOperationException("OpenAiDeveloperKey is missing in User Secrets configuration");
        OpenAiOrganization = configuration["OpenAiOrganization"] ??
                             throw new InvalidOperationException("OpenAiOrganization is missing in User Secrets configuration");
    }


}
