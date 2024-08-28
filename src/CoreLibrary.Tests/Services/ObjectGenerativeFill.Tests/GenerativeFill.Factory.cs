using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

internal static class GenerativeFillTestFactory
{
    internal static GenerativeFill CreateInstance()
    {
        // read API configuration
        var config = new ConfigurationBuilder().AddUserSecrets<GenerativeFillTests>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];

        // create ChatGPT client instance
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();
        var chatGptClientCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "CoreLibrary.Tests.ChatGptClient");
        Directory.CreateDirectory(chatGptClientCacheFolder);
        var chatGptClient = new ChatGptClient(logger, openAiOrganizationId!, openAiDeveloperKey!, chatGptClientCacheFolder);

        // create instance of system under test
        var generativeFillCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "CoreLibrary.Tests.GenerativeFill");
        var generativeFill = new GenerativeFill(chatGptClient, generativeFillCacheFolder);
        return generativeFill;
    }
}

