using BookToAnki.Services;
using FluentAssertions;

namespace BookToAnki.Tests;

[TestClass]
public class PartOfSpeechDictionaryBuilderTests
{
    [DataRow("")]
    [DataRow(" \n\n  ")]
    [DataRow(" \t \n\t ")]
    [DataTestMethod]
    public void When_InputIsEmpty_Expect_EmptyDictionary(string inputFileContent)
    {
        // Arrange
        var sut = new PartOfSpeechDictionaryBuilder();

        // Act
        var result = sut.BuildPartOfSpeechDictionary(inputFileContent);

        // Assert
        result.Should().NotBeNull();
        result.WordToPartOfSpeech.Keys.Should().BeEmpty();
    }

    [DataRow("# Noun")]
    [DataRow(
@"
#Noun

#Verb
#Adjective
")]
    [DataTestMethod]
    public void When_InputHasOnlyLabelsButNoWordsInSections_Expect_EmptyDictionary(string inputFileContent)
    {
        // Arrange
        var sut = new PartOfSpeechDictionaryBuilder();

        // Act
        var result = sut.BuildPartOfSpeechDictionary(inputFileContent);

        // Assert
        result.Should().NotBeNull();
        result.WordToPartOfSpeech.Keys.Should().BeEmpty();
    }

    [Ignore]
    [TestMethod]
    public void When_InputHasSameWordInTwoConflictingCategories_Expect_ArgumentException()
    {
        // Arrange
        var sut = new PartOfSpeechDictionaryBuilder();

        var input = @"
        # Rzeczownik
        krowa

        # Nierozpoznano
        krowa
        ";

        // Act
        sut.Invoking(x => x.BuildPartOfSpeechDictionary(input))
            .Should().Throw<ArgumentException>()
            .WithMessage("*krowa*");

        // Assert
    }
}

