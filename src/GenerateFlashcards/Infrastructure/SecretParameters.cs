using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Infrastructure;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Property names match conventions typical for Environment Variables, which is one of the sources of this data")]
public class SecretParameters
{
    public string? OPENAI_ORGANIZATION_ID { get; set; }
    public string? OPENAI_DEVELOPER_KEY { get; set; }

    public bool EnsureOpenAIKeysArePresent(ILogger logger)
    {
        // write out values
        var openAiKeysPresent = true;
        if (string.IsNullOrWhiteSpace(OPENAI_ORGANIZATION_ID))
        {
            logger.LogWarning($"The `OPENAI_ORGANIZATION_ID` secret is missing in configuration. Application will use mocked Generative AI responses. Read how to configure: {Parameters.UrlToDocumentationAboutDefiningUserSecrets}\n");
            openAiKeysPresent = false;
        }

        if (string.IsNullOrWhiteSpace(OPENAI_DEVELOPER_KEY))
        {
            logger.LogWarning($"The `OPENAI_DEVELOPER_KEY` secret is missing in configuration. Application will use mocked Generative AI responses. Read how to configure: {Parameters.UrlToDocumentationAboutDefiningUserSecrets}");
            openAiKeysPresent = false;
        }

        return openAiKeysPresent;
    }
}
