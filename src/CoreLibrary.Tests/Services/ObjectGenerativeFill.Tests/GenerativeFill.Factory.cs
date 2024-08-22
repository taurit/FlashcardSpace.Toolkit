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
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();
        var cacheRootFolder = Path.Combine(Path.GetTempPath(), "ObjectGenerativeFill", "CoreLibraryIntegrationTests");
        Directory.CreateDirectory(cacheRootFolder);

        var chatGptClient = new ChatGptClient(logger, openAiOrganizationId!, openAiDeveloperKey!, cacheRootFolder);

        // Create instance of system under test
        var generativeFill = new GenerativeFill(chatGptClient);
        return generativeFill;
    }
}

