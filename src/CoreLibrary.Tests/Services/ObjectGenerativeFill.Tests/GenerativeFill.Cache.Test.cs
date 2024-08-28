using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

[TestClass, TestCategory("SkipInGitHubActions")]
//[Ignore("Skipped to avoid unnecessary costs. Uncomment when modifying the service or changing the AI model.")]
public class GenerativeFillCacheTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task When_GenerativeFillIsUsed_SecondCallIsReadFromCacheAndNotAPI()
    {
        // Arrange
        var input = new ItalianWord("grazie");

        // Act
        List<TimeSpan> attemptTimes = new List<TimeSpan>();

        for (int numAttempt = 0; numAttempt < 3; numAttempt++)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);
            sw.Stop();
            attemptTimes.Add(sw.Elapsed);

            Console.WriteLine("Attempt {0} took {1} ms", numAttempt, sw.ElapsedMilliseconds);
        }

        // Assert
        // observations: online call is ~900 ms
        // cache call is ~5 ms
        attemptTimes[1].TotalMilliseconds.Should().BeLessThan(100, because: "data should be read from cache and be quick");
    }

    [TestMethod]
    public async Task When_GenerativeFillIsUsed_IndividualFilesForAllReceivedObjectsAreCreated()
    {
        // Arrange
        var input = new List<ItalianWord>() {
            new("grazie"),
            new("ciao"),
            new("buongiorno"),
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert

    }

}

internal class ItalianWord(string wordInItalian) : ObjectWithId
{
    [JsonConstructor] public ItalianWord() : this(null!) { }

    public string WordInItalian { get; init; } = wordInItalian;

    [FillWithAI]
    [FillWithAIRule($"Translate {nameof(WordInItalian)} to English")]
    public string? WordInEnglish { get; set; } = null;
}
