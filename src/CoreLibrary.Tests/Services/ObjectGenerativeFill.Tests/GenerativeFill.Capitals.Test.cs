using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using Newtonsoft.Json;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

/// <summary>
/// These tests use the real OpenAI API, therefore are skipped from running in the pipeline to not generate costs.
/// </summary>
[TestClass, TestCategory("SkipInGitHubActions")]
//[Ignore("Skipped to avoid unnecessary costs. Uncomment when modifying the service or changing the AI model.")]
public class GenerativeFillTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task FillCountryFirstCapital()
    {
        // Arrange
        var input = new Country("Poland");

        input.CountryName.Should().Be("Poland");
        input.FirstCapital.Should().BeNull();
        input.CurrentCapital.Should().BeNull();

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        input.CountryName.Should().Be("Poland", because: "Input object should not be modified");
        input.FirstCapital.Should().BeNull(because: "Input object should not be modified");
        input.CurrentCapital.Should().BeNull(because: "Input object should not be modified");

        output.CountryName.Should().Be("Poland", because: "Fields without attributes are not supposed to be changed by service");
        output.FirstCapital.Should().Be("Gniezno", because: "Field with [FillWithAI] attribute should be correctly filled by the service");
        output.CurrentCapital.Should().Be("WARSAW", because: "Field with [FillWithAI] attribute should be correctly filled by the service");
    }
}

class Country(string countryName) : ObjectWithId
{
    [JsonConstructor] public Country() : this(null!) { }

    public string CountryName { get; init; } = countryName;

    [FillWithAI]
    [FillWithAIRule("The first historically known capital of the country, not the current one")]
    [FillWithAIRule("If capital goes by several names, use the local name of the city (not the international one)")]
    public string? FirstCapital { get; set; } = null;

    [FillWithAI]
    [FillWithAIRule("The name of the current capital of the country.")]
    [FillWithAIRule("If capital goes by several names, use the name most used in English-speaking countries.")]
    [FillWithAIRule("Use CAPITAL LETTERS for this value.")]
    public string? CurrentCapital { get; set; } = null;
}
