using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.Infrastructure;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;
[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class SpanishAdjectiveConcretenessTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task When_AdjectiveHasTangibleMeaning_Expect_ClassifiedAsConcrete()
    {
        // Arrange
        var inputs = new List<SpanishAdjectiveConcreteness>() {
            new() { Adjective = "rojo" },
            new() { Adjective = "grande" },
            new() { Adjective = "pequeño" },
            new() { Adjective = "dulce" },
            new() { Adjective = "salado" }
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputs);

        // Assert
        output.Dump();

        output.Should().HaveCount(5);
        output.Should().OnlyContain(noun => noun.IsConcrete);
    }

    [TestMethod]
    public async Task When_AdjectiveHasVagueOrAbstractMeaning_Expect_ClassifiedAsNonConcrete()
    {
        // Arrange
        var clearlyAbstractNouns = new List<SpanishAdjectiveConcreteness>() {
            new() { Adjective = "este" },
            new() { Adjective = "aquel" },
            new() { Adjective = "cada" },
            new() { Adjective = "cualquier" },
            new() { Adjective = "algunos" },
            new() { Adjective = "muchos" }
        };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, clearlyAbstractNouns);

        // Assert
        output.Dump();

        output.Should().HaveCount(6);
        output.Should().OnlyContain(noun => !noun.IsConcrete);
    }

}
