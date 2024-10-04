namespace CoreLibrary.Models;
public record OpenAiCredentials(
    string? AzureOpenAiEndpoint, string? AzureOpenAiKey,
    string? OpenAiOrganizationId, string? OpenAiDeveloperKey
    )
{
    public OpenAiBackend BackendType
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(AzureOpenAiEndpoint) && !string.IsNullOrWhiteSpace(AzureOpenAiKey))
                return OpenAiBackend.Azure;

            if (!string.IsNullOrWhiteSpace(OpenAiOrganizationId) && !string.IsNullOrWhiteSpace(OpenAiDeveloperKey))
                return OpenAiBackend.OpenAi;

            return OpenAiBackend.None;
        }
    }
}
