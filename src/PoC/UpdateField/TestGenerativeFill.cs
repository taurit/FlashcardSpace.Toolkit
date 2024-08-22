using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.GenerativeFill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UpdateField;

class Country(string countryName) : ObjectWithId
{
    public string CountryName { get; init; } = countryName;

    [FillWithAI]
    [FillWithAIRule("Fill with the first historically known capital of the country, not the current one!")]
    public string? FirstCapital { get; set; } = null;

    [FillWithAI]
    [FillWithAIRule("Fill the value with name of the current president of the country")]
    [FillWithAIRule("Use CAPITAL LETTERS for this value")]
    public string? President { get; set; } = null;
}

internal class TestGenerativeFill
{
    public async Task DoTestGenerativeFill()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<TestGenerativeFill>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];

        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();
        var chatGptClient = new ChatGptClient(logger, openAiOrganizationId!, openAiDeveloperKey!, "s:\\Caches\\temp\\");
        var generativeFill = new GenerativeFill(chatGptClient);

        var countries = await generativeFill.FillMissingProperties("gpt-4o-mini", "gpt-mini",
                new Country[] { new("USA"), new("Poland"), new("France") }
            );


        foreach (var job in countries)
        {
            Console.WriteLine($"{job.CountryName}: first capital was {job.FirstCapital}, the current president is {job.President}.");
        }
    }
}
