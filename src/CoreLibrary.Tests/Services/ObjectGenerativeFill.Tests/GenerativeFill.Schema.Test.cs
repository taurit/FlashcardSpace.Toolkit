using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

[TestClass]
public class GenerativeFillSchemaTests
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "We only need the type in this test")]
    private class AmericanState(string state) : ObjectWithId
    {
        public string StateName { get; init; } = state;

        [FillWithAI]
        [FillWithAIRule($"Fill with the name of the capital of the state defined in `{nameof(StateName)}`")]
        [FillWithAIRule("Use ALL CAPITAL LETTERS for the value.")]
        public string? Capital { get; set; } = null;

        [FillWithAI]
        [FillWithAIRule($"Fill with the list of all neighboring states of the state in `{nameof(StateName)}`")]
        public List<string>? NeighboringStates { get; set; }
    }

    [TestMethod]
    public void WhenObjectIsGiven_GeneratesOutputSchemaAcceptableByOpenAiAPI()
    {
        // Arrange
        var sut = new GenerativeFillSchemaProvider(GenerativeFillTestFactory.GenerativeFillCacheFolder);

        // Act
        var schemaString = sut.GenerateJsonSchemaForArrayOfItems<AmericanState>();
        Console.WriteLine(schemaString);

        // Assert
        var schema = JObject.Parse(schemaString);

        schema["type"]!.Value<string>().Should().Be("object", because: "Arrays are best wrapped in object in OpenAI API");
        schema["additionalProperties"]!.Value<Boolean>().Should().Be(false, because: "It's required by the API");

        schema["required"]!.Values<string>().Should().ContainSingle("Items");
        schema["properties"]!["Items"]!["type"]!.Value<string>().Should().Be("array");
        schema["properties"]!["Items"]!["items"]!["properties"]!["Id"]!["type"]!.Value<string>().Should().Be("integer");
        schema["properties"]!["Items"]!["items"]!["properties"]!["Capital"]!["type"]!.Value<string>().Should().Be("string");
        schema["properties"]!["Items"]!["items"]!["properties"]!["NeighboringStates"]!["type"]!.Value<string>().Should().Be("array");

        schema["properties"]!["Items"]!["items"]!["properties"]!["StateName"].Should().BeNull(because: "Input properties other than Id should not be repeated in output");

        schema["properties"]!["Items"]!["items"]!["required"]!.Values<string>().Should().BeEquivalentTo(["Capital", "NeighboringStates", "Id"]);
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "We only need the type in this test")]
    private class EnglishWord(string word) : ObjectWithId
    {
        public string Word { get; init; } = word;

        [FillWithAI]
        [FillWithAIRule("Word translated to Polish.")]
        [FillWithAIRule("Value is in CAPITAL LETTERS.")]
        public string? TranslationToPolish { get; set; } = null;
    }

    [TestMethod]
    public void WhenMultipleRulesArePresentForProperty_ConcatenateThemAsSentences()
    {
        // Arrange
        var sut = new GenerativeFillSchemaProvider(GenerativeFillTestFactory.GenerativeFillCacheFolder);

        // Act
        var schemaString = sut.GenerateJsonSchemaForArrayOfItems<EnglishWord>();
        Console.WriteLine(schemaString);

        // Assert
        var schema = JObject.Parse(schemaString);

        schema["type"]!.Value<string>().Should().Be("object", because: "Arrays are best wrapped in object in OpenAI API");
        schema["additionalProperties"]!.Value<Boolean>().Should().Be(false, because: "It's required by the API");

        schema["required"]!.Values<string>().Should().ContainSingle("Items");
        schema["properties"]!["Items"]!["type"]!.Value<string>().Should().Be("array");
        schema["properties"]!["Items"]!["items"]!["properties"]!["TranslationToPolish"]!["description"]!.Value<string>().Should().Be(
            "Word translated to Polish. Value is in CAPITAL LETTERS."
            );

        schema["properties"]!["Items"]!["items"]!["$id"]!.Value<string>().Should().Be(
            nameof(EnglishWord), because: "Providing type name can help OpenAI API determine its purpose");
    }

}

