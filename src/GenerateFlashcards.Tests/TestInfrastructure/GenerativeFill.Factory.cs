using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GenerateFlashcards.Tests.Infrastructure;

internal class GenerativeFillTestFactory
{
    internal static GenerativeFill CreateInstance()
    {
        // read API configuration
        var config = new ConfigurationBuilder().AddUserSecrets<GenerativeFillTestFactory>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];

        // create ChatGPT client instance
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();
        var cacheRootFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "GenerateFlashcards.Tests.ChatGptClient");
        Directory.CreateDirectory(cacheRootFolder);
        var chatGptClient = new ChatGptClient(logger, openAiOrganizationId!, openAiDeveloperKey!, cacheRootFolder);

        // create instance of system under test
        var generativeFillCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "GenerateFlashcards.Tests.GenerativeFill");
        var generativeFill = new GenerativeFill(chatGptClient, generativeFillCacheFolder);
        return generativeFill;
    }
}

