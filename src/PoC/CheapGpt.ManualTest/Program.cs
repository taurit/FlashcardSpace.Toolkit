using Microsoft.Extensions.Configuration;

namespace CheapGpt.ManualTest;

internal class Program
{
    static async Task Main(string[] args)
    {
        // Read API Key from User Secrets (Visual Studio: right-click on project -> Manage User Secrets)
        var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var apiKey = configuration["OpenAiDeveloperKey"] ?? throw new ArgumentException("OpenAPI Key is missing in User Secrets configuration");

        // Prepare prompts
        var prompt1 = "What is the capital of Poland?";
        var prompt2 = "Who won the ski jumping season 1999?";

        // Send prompts
        var client = new OpenAiBatchClient(apiKey, "gpt-4o", 1000, "s:\\Caches\\CheapGpt\\");
        client.AddPrompt(prompt1);
        client.AddPrompt(prompt2);
        await client.StartJob();

        // Await answer
        var answer1 = await client.GetAnswer(prompt1);
        Console.WriteLine(answer1);

        var answer2 = await client.GetAnswer(prompt2);
        Console.WriteLine(answer2);


    }

}
