using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

/// <summary>
/// These tests use the real OpenAI API, therefore are skipped from running in the pipeline to not generate costs.
/// </summary>
[TestClass, TestCategory("SkipInGitHubActions")]
//[Ignore("Skipped to avoid unnecessary costs. Uncomment when modifying the service or changing the AI model.")]
public class GenerativeFillAmericanStatesTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task FillAmericanStateDetails()
    {
        // Arrange
        var input = new AmericanState("California");

        input.StateName.Should().Be("California");
        input.Capital.Should().BeNull();
        input.NeighboringStates.Should().BeNull();

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        input.StateName.Should().Be("California", because: "Input object should not be modified");
        input.Capital.Should().BeNull(because: "Input object should not be modified");
        input.NeighboringStates.Should().BeNull(because: "Input object should not be modified");

        output.StateName.Should().Be("California", because: "Fields without attributes are not supposed to be changed by service");
        output.Capital.Should().Be("<b>Sacramento</b>", because: "Field with [FillWithAI] attribute should be correctly filled by the service");
        output.NeighboringStates.Should().BeEquivalentTo(["Oregon", "Nevada", "Arizona"]);
    }

    [TestMethod]
    public async Task FillAmericanStateDetails_WorksWithArrays_AndReturnsResultInTheSameOrder_EvenWhenInputContainsDuplicates()
    {
        // Arrange
        var input = new List<AmericanState>() {
            new("California"),
            new("Texas"),
            new("Florida"),
            new("Texas"),
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);

        // Assert
        output.Should().HaveCount(4, because: "All input items should be processed");

        output[0].StateName.Should().Be("California");
        output[0].Capital.Should().Be("<b>Sacramento</b>");
        output[0].NeighboringStates.Should().BeEquivalentTo(["Oregon", "Nevada", "Arizona"]);

        output[1].StateName.Should().Be("Texas");
        output[1].Capital.Should().Be("<b>Austin</b>");
        output[1].NeighboringStates.Should().BeEquivalentTo(["New Mexico", "Oklahoma", "Arkansas", "Louisiana"]);

        output[2].StateName.Should().Be("Florida");
        output[2].Capital.Should().Be("<b>Tallahassee</b>");
        output[2].NeighboringStates.Should().BeEquivalentTo(["Georgia", "Alabama"]);

        output[3].StateName.Should().Be("Texas");
        output[3].Capital.Should().Be("<b>Austin</b>");
        output[3].NeighboringStates.Should().BeEquivalentTo(["New Mexico", "Oklahoma", "Arkansas", "Louisiana"]);
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
    private class AmericanState(string state) : ObjectWithId
    {
        [JsonConstructor] public AmericanState() : this(null!) { }

        public string StateName { get; init; } = state;

        [FillWithAI]
        [FillWithAIRule($"Fill with the name of the capital of the state defined in `{nameof(StateName)}`")]
        [FillWithAIRule("Wrap the value in <b></b> tag.")]
        public string? Capital { get; set; } = null;

        [FillWithAI]
        [FillWithAIRule($"Fill with the list of all neighboring states of the state in `{nameof(StateName)}`")]
        public List<string>? NeighboringStates { get; set; }
    }

}

