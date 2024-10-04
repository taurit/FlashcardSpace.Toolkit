using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

internal static class GenerativeFillTestFactory
{
    internal static readonly string GenerativeFillCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "CoreLibrary.Tests.GenerativeFill");
    private static readonly string ChatGptClientCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "CoreLibrary.Tests.ChatGptClient");

    internal static GenerativeFill CreateInstance()
    {
        // read API configuration
        var config = new ConfigurationBuilder().AddUserSecrets<GenerativeFillTests>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];
        var azureOpenAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var azureOpenAiKey = config["AZURE_OPENAI_KEY"];
        var openAiCredentials = new OpenAiCredentials(azureOpenAiEndpoint, azureOpenAiKey, openAiOrganizationId, openAiDeveloperKey);

        // create ChatGPT client instance
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();

        Directory.CreateDirectory(ChatGptClientCacheFolder);
        var chatGptClient = new ChatGptClient(logger, openAiCredentials, ChatGptClientCacheFolder);

        // create instance of system under test
        var generativeFill = new GenerativeFill(chatGptClient, GenerativeFillCacheFolder);
        return generativeFill;
    }
}

