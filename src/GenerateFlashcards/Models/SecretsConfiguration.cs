using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Models;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Property names match conventions typical for Environment Variables, which is one of the sources of this data")]
public class SecretsConfiguration
{
    public string? OPENAI_ORGANIZATION_ID { get; set; }
    public string? OPENAI_DEVELOPER_KEY { get; set; }

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(OPENAI_ORGANIZATION_ID))
        {
            throw new InvalidOperationException($"The `OPENAI_ORGANIZATION_ID` secret is missing in configuration. Read how to set it: {Parameters.UrlToDocumentationAboutDefiningUserSecrets}");
        }

        if (string.IsNullOrWhiteSpace(OPENAI_DEVELOPER_KEY))
        {
            throw new InvalidOperationException($"The `OPENAI_DEVELOPER_KEY` secret is missing in configuration. Read how to set it: {Parameters.UrlToDocumentationAboutDefiningUserSecrets}");
        }
    }
}
