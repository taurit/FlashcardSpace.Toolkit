using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;
using FluentAssertions.Execution;
using GenerateFlashcards.Models.Spanish;
using GenerateFlashcards.Tests.TestInfrastructure;

namespace GenerateFlashcards.Tests.Models.Spanish;

[TestClass]
[TestCategory("RequiresGenerativeAi")]
public class SpanishPartsOfSpeechTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [DataTestMethod]
    [DataRow("joven")]
    public async Task ThisWord_CanOnlyBeAdjectiveOrNoun(string word)
    {
        int numFailuresAdjective = 0;
        int numFailuresNoun = 0;
        int numFailuresVerb = 0;

        for (int i = 1; i <= 5; i++)
        {
            var input1 = new SpanishAdjectiveDetector() { IsolatedWord = word };
            var input2 = new SpanishNounDetector() { IsolatedWord = word };
            var input3 = new SpanishVerbDetector() { IsolatedWord = word };

            var output1 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input1, seed: i);
            var output2 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input2, seed: i);
            var output3 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input3, seed: i);

            // sanity check
            output1.IsolatedWord.Should().Be(word);
            output2.IsolatedWord.Should().Be(word);
            output3.IsolatedWord.Should().Be(word);

            if (output1.IsolatedWord != word || output1.IsAdjective == false)
            {
                output1.Dump();
                numFailuresAdjective++;
            }

            if (output2.IsolatedWord != word || output2.IsNoun == false || output2.BaseForm != $"el {word}")
            {
                output2.Dump();
                numFailuresNoun++;
            }

            if (output3.IsolatedWord != word || output3.IsVerb == true)
            {
                output3.Dump();
                numFailuresVerb++;
            }
        }

        // Assert
        using (new AssertionScope())
        {
            numFailuresAdjective.Should().Be(0);
            numFailuresNoun.Should().Be(0);
            numFailuresVerb.Should().Be(0);
        }
    }

    [DataTestMethod]
    [DataRow("mono")]
    public async Task ThisWord_CanOnlyBeNoun(string word)
    {
        int numFailuresAdjective = 0;
        int numFailuresNoun = 0;
        int numFailuresVerb = 0;

        for (int i = 1; i <= 5; i++)
        {
            var input1 = new SpanishAdjectiveDetector() { IsolatedWord = word };
            var input2 = new SpanishNounDetector() { IsolatedWord = word };
            var input3 = new SpanishVerbDetector() { IsolatedWord = word };

            var output1 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input1, seed: i);
            var output2 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input2, seed: i);
            var output3 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input3, seed: i);

            // sanity check
            output1.IsolatedWord.Should().Be(word);
            output2.IsolatedWord.Should().Be(word);
            output3.IsolatedWord.Should().Be(word);

            if (output2.IsolatedWord != word || output2.IsNoun == false || output2.BaseForm != $"el {word}")
            {
                output2.Dump();
                numFailuresNoun++;
            }


            if (output1.IsolatedWord != word || output1.IsAdjective)
            {
                output1.Dump();
                numFailuresAdjective++;
            }

            if (output3.IsolatedWord != word || output3.IsVerb == true)
            {
                output3.Dump();
                numFailuresVerb++;
            }
        }

        // Assert
        using (new AssertionScope())
        {
            numFailuresAdjective.Should().Be(0);
            numFailuresNoun.Should().Be(0);
            numFailuresVerb.Should().Be(0);
        }
    }

    [DataTestMethod]
    [DataRow("zxckjnzkxcjn")] // word doesn't exist
    [DataRow("123123123123")] // numeral maybe
    [DataRow("de")]
    [DataRow("la")]
    [DataRow("que")]
    [DataRow("el")]
    [DataRow("en")]
    [DataRow("y")]
    [DataRow("a")]
    [DataRow("los")]
    [DataRow("se")]
    [DataRow("del")]
    public async Task ThisWord_IsNeitherAdjectiveNorNounNorVerb(string word)
    {
        // Arrange
        var input1 = new SpanishAdjectiveDetector() { IsolatedWord = word };
        var input2 = new SpanishNounDetector() { IsolatedWord = word };
        var input3 = new SpanishVerbDetector() { IsolatedWord = word };

        // Act
        var output1 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input1);
        var output2 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input2);
        var output3 = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input3);

        // Assert
        output1.Dump();
        output2.Dump();
        output3.Dump();

        output1.IsolatedWord.Should().Be(word);
        output1.IsAdjective.Should().BeFalse();
        output1.BaseForm.Should().BeNull();
        output1.SentenceExample.Should().BeNull();

        output2.IsolatedWord.Should().Be(word);
        output2.IsNoun.Should().BeFalse();
        output2.BaseForm.Should().BeNull();
        output2.SentenceExample.Should().BeNull();

        output3.IsolatedWord.Should().Be(word);
        output3.IsVerb.Should().BeFalse();
        output3.BaseForm.Should().BeNull();
        output3.SentenceExample.Should().BeNull();
    }


    [DataTestMethod]
    [DataRow("comer", "comer")]
    [DataRow("bebiendo", "beber")]
    [DataRow("corre", "correr")]
    [DataRow("saltando", "saltar")]
    public async Task ThisWord_CanOnlyBeVerb(string word, string expectedInfinitiveForm)
    {
        // Arrange
        var inputAdjective = new SpanishAdjectiveDetector() { IsolatedWord = word };
        var inputNoun = new SpanishNounDetector() { IsolatedWord = word };
        var inputVerb = new SpanishVerbDetector() { IsolatedWord = word };

        // Act
        var outputAdjective = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputAdjective);
        var outputNoun = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputNoun);
        var outputVerb = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, inputVerb);

        // Assert
        outputAdjective.Dump();
        outputNoun.Dump();
        outputVerb.Dump();

        outputAdjective.IsolatedWord.Should().Be(word);
        outputAdjective.IsAdjective.Should().BeFalse();
        outputAdjective.BaseForm.Should().BeNull();
        outputAdjective.SentenceExample.Should().BeNull();

        outputNoun.IsolatedWord.Should().Be(word);
        outputNoun.IsNoun.Should().BeFalse();
        outputNoun.BaseForm.Should().BeNull();
        outputNoun.SentenceExample.Should().BeNull();

        outputVerb.IsolatedWord.Should().Be(word);
        outputVerb.IsVerb.Should().BeTrue();
        outputVerb.BaseForm.Should().Be(expectedInfinitiveForm);
        outputVerb.SentenceExample.Should().ContainAny(word, expectedInfinitiveForm);
    }


    [TestMethod]
    public async Task WordShouldBeConsistentlyClassifiedCorrectly()
    {
        int numIterations = 10;
        int numWrongClassifications = 0;

        for (int i = 1; i <= numIterations; i++)
        {
            var input = new SpanishAdjectiveDetector() { IsolatedWord = "medio" };
            var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input, seed: i);

            // Assert
            Console.WriteLine($"Iteration {i}/{numIterations}");
            output.Dump();

            if (output.IsAdjective == false)
                numWrongClassifications++;

            output.SentenceExample.Should().NotContain("medio ambiente"); // frequent error made by the model
        }

        numWrongClassifications.Should().Be(0);
    }
}
