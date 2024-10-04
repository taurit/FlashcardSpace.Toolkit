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

    public bool WarnIfGenerativeAIKeysAreNotPresent(ILogger logger)
    {
        // write out values
        var genAiKeysPresent = true;

        if (string.IsNullOrWhiteSpace(OPENAI_ORGANIZATION_ID))
        {
            logger.LogWarning($"The `OPENAI_ORGANIZATION_ID` secret is missing in configuration. Application will use mocked Generative AI responses. Read how to configure: {DocumentationUrlAboutUserSecrets}\n");
            genAiKeysPresent = false;
        }

        if (string.IsNullOrWhiteSpace(OPENAI_DEVELOPER_KEY))
        {
            logger.LogWarning($"The `OPENAI_DEVELOPER_KEY` secret is missing in configuration. Application will use mocked Generative AI responses. Read how to configure: {DocumentationUrlAboutUserSecrets}");
            genAiKeysPresent = false;
        }

        if (string.IsNullOrWhiteSpace(AZURE_TEXT_TO_SPEECH_KEY))
        {
            logger.LogWarning($"The `AZURE_TEXT_TO_SPEECH_KEY` secret is missing in configuration. Application will not generate audio files. Read how to configure: {DocumentationUrlAboutUserSecrets}");
            genAiKeysPresent = false;
        }

        if (string.IsNullOrWhiteSpace(AZURE_TEXT_TO_SPEECH_REGION))
        {
            logger.LogWarning($"The `AZURE_TEXT_TO_SPEECH_REGION` secret is missing in configuration. Application will not generate audio files. Read how to configure: {DocumentationUrlAboutUserSecrets}");
            genAiKeysPresent = false;
        }

        return genAiKeysPresent;
    }
}
