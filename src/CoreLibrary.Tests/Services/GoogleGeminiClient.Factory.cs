using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Tests.Services;

internal static class GoogleGeminiClientFactory
{
    internal static GoogleGeminiClient CreateInstance()
    {
        // read API configuration
        var config = new ConfigurationBuilder().AddUserSecrets<GenerativeFillTests>().Build();
        var geminiApiKey = config["GeminiApiKey"];

        // create client instance
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<GoogleGeminiClient>();
        var cacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "CoreLibrary.Tests.GoogleGeminiClient");
        Directory.CreateDirectory(cacheFolder);
        var geminiClient = new GoogleGeminiClient(logger, geminiApiKey!, cacheFolder);

        return geminiClient;
    }
}

