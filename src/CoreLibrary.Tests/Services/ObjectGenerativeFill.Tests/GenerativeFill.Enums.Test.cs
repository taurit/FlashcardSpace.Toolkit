using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

[TestClass, TestCategory("SkipInGitHubActions")]
//[Ignore("Skipped to avoid unnecessary costs. Uncomment when modifying the service or changing the AI model.")]
public class GenerativeFillEnumsTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task GenerativeFill_ShouldSupportEnumType()
    {
        // Arrange
        var input = new List<TermInEnglish>() {
            new() { Term = "a cat" },
            new() { Term = "to be on cloud nine" },
            new() { Term = "to run" },
            new() { Term = "quickly" },
            new() { Term = "blue" },
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Should().HaveCount(5);
        output[0].TermType.Should().Be(TermType.Noun);
        output[1].TermType.Should().Be(TermType.Idiom);
        output[2].TermType.Should().Be(TermType.Verb);
        output[3].TermType.Should().Be(TermType.Other);
        output[4].TermType.Should().Be(TermType.Other);

    }
}

internal enum TermType
{
    Noun,
    Verb,
    Idiom,
    Other,
}

internal class TermInEnglish() : ObjectWithId
{
    public string? Term { get; init; }

    [FillWithAI]
    public TermType TermType { get; set; }
}
