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

    [TestMethod]
    public void When_InputHasItemsInTwoCategories_Expect_ProperMapping()
    {
        // Arrange
        var sut = new PartOfSpeechDictionaryBuilder();

        var input = @"
        # Rzeczownik
        krowa
        pies

        # Przymiotnik
        czarny
        ";

        // Act
        var result = sut.BuildPartOfSpeechDictionary(input);

        // Assert
        result.Should().NotBeNull();
        result.WordToPartOfSpeech.Keys.Should().HaveCount(3);
        result.WordToPartOfSpeech["krowa"].Should().Be("Rzeczownik");
        result.WordToPartOfSpeech["pies"].Should().Be("Rzeczownik");
        result.WordToPartOfSpeech["czarny"].Should().Be("Przymiotnik");
        result.PartsOfSpeech.Single(x => x.Name == "Rzeczownik").Count.Should().Be(2);
        result.PartsOfSpeech.Single(x => x.Name == "Przymiotnik").Count.Should().Be(1);
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

