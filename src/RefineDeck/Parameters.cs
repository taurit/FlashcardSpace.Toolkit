using CoreLibrary.Utilities;
using Microsoft.Extensions.Configuration;

namespace RefineDeck;
internal static class Parameters
{
    // List: https://ai.google.dev/gemini-api/docs/models/gemini

    // Most intelligent but expensive
    private const string MostRecentGeminiProModelId = "gemini-1.5-pro-002";
    // Less intelligent, but cheap and great for dev and testing
    private const string MostRecentGeminiFlashModelId = "gemini-1.5-flash-8b";

    public const string GeminiModelId = MostRecentGeminiFlashModelId;

    /// <summary>
    /// Load the secrets from:
    /// - User Secrets in Visual Studio, or
    /// - the secrets.json file
    /// </summary>
    internal static SecretParameters LoadSecrets()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<MainWindow>()
            .AddJsonFile("secrets.json", optional: true)
            .Build();

        // Bind the configuration values to the strongly typed class
        var secrets = new SecretParameters();
        configuration.Bind(secrets);
        return secrets;
    }
}
