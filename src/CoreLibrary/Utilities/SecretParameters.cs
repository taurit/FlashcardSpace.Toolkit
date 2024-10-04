using CoreLibrary.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace CoreLibrary.Utilities;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Property names match conventions typical for Environment Variables, which is one of the sources of this data")]
public class SecretParameters
{
    const string DocumentationUrlAboutUserSecrets = "https://github.com/taurit/FlashcardSpace.Toolkit/blob/main/docs/Secrets.md";

    public string? AZURE_OPENAI_ENDPOINT { get; set; }
    public string? AZURE_OPENAI_KEY { get; set; }

    public string? OPENAI_ORGANIZATION_ID { get; set; }
    public string? OPENAI_DEVELOPER_KEY { get; set; }

    public string? AZURE_TEXT_TO_SPEECH_KEY { get; set; }
    public string? AZURE_TEXT_TO_SPEECH_REGION { get; set; }

    public OpenAiCredentials OpenAiCredentials => new(AZURE_OPENAI_ENDPOINT!, AZURE_OPENAI_KEY!, OPENAI_ORGANIZATION_ID!, OPENAI_DEVELOPER_KEY!);

    public string GeminiApiKey { get; set; }

    public bool WarnIfGenerativeAIKeysAreNotPresent(ILogger logger)
    {
        var genAiKeysPresent = true;

        if (OpenAiCredentials.BackendType == OpenAiBackend.None)
        {
            logger.LogWarning($"The Open AI credentials are not found in the configuration. Read how to configure: {DocumentationUrlAboutUserSecrets}");

            genAiKeysPresent = false;
        }

        if (string.IsNullOrWhiteSpace(AZURE_TEXT_TO_SPEECH_KEY))
        {
            logger.LogWarning($"The `AZURE_TEXT_TO_SPEECH_KEY` secret is missing in configuration. Read how to configure: {DocumentationUrlAboutUserSecrets}");

            genAiKeysPresent = false;
        }

        if (string.IsNullOrWhiteSpace(AZURE_TEXT_TO_SPEECH_REGION))
        {
            logger.LogWarning($"The `AZURE_TEXT_TO_SPEECH_REGION` secret is missing in configuration. Read how to configure: {DocumentationUrlAboutUserSecrets}");

            genAiKeysPresent = false;
        }

        return genAiKeysPresent;
    }
}
