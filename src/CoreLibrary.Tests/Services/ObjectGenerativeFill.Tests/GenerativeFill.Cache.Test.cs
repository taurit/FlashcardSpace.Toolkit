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
        output.Should().NotBeNull();
        output.Should().HaveCount(3);
        output.Should().ContainSingle(w => w.WordInItalian == "grazie" && w.WordInEnglish != null);
        output.Should().ContainSingle(w => w.WordInItalian == "ciao" && w.WordInEnglish != null);
        output.Should().ContainSingle(w => w.WordInItalian == "buongiorno" && w.WordInEnglish != null);

        var filesInCacheFolder = Directory.GetFiles(_generativeFill.GenerativeFillCacheFolder, "*.json", SearchOption.TopDirectoryOnly);
        filesInCacheFolder.Should().HaveCountGreaterThanOrEqualTo(3);
        filesInCacheFolder.Should().Contain(f => f.Contains("grazie"));
        filesInCacheFolder.Should().Contain(f => f.Contains("ciao"));
        filesInCacheFolder.Should().Contain(f => f.Contains("buongiorno"));
    }

    [TestMethod]
    public async Task When_GenerativeFillIsUsed_CacheFilesAreUtilized()
    {
        // Arrange
        var input = new List<ItalianWord>() {
            new("arrivederci"),
            new("buonasera"),
        };
        // if cache files existed from last test runs, remove them
        var cacheFilesFromPreviousRuns = Directory.EnumerateFiles(_generativeFill.GenerativeFillCacheFolder, "*arrivederci*.json");
        foreach (var file in cacheFilesFromPreviousRuns)
        {
            File.Delete(file);
        }
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);
        var cacheFilePath = Directory.EnumerateFiles(_generativeFill.GenerativeFillCacheFolder, "*arrivederci*.json").Single();

        // manipulate the cache to be able to prove the file was used
        var cacheContent = await File.ReadAllTextAsync(cacheFilePath);
        var cacheItem = JsonConvert.DeserializeObject<ItalianWord>(cacheContent);
        cacheItem!.WordInEnglish = "VALUE IN CACHE FILE";
        await File.WriteAllTextAsync(cacheFilePath, JsonConvert.SerializeObject(cacheItem));

        // Act
        Stopwatch s = Stopwatch.StartNew();
        var output2 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);
        s.Stop();

        // Assert
        output2.Should().NotBeNull();
        output2.Should().HaveCount(2);
        output2.Single(w => w.WordInItalian == "arrivederci").WordInEnglish.Should().Be("VALUE IN CACHE FILE");

        s.ElapsedMilliseconds.Should().BeLessThan(100, because: "data should be read from cache and be quick");
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
