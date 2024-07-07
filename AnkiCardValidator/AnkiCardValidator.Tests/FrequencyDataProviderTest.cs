using AnkiCardValidator.Utilities;
using FluentAssertions;
using System.Diagnostics;

namespace AnkiCardValidator.Tests;

[TestClass]
[IgnoreIfEnvironmentVariableNotSet("RUN_LOCAL_TESTS", "I'm not committing the dictionaries at the moment so those tests wouldn't work in pipelines. Todo: set up LFS and un-hardcode paths someday.")]
public class FrequencyDataProviderTest
{
    private static FrequencyDataProvider _sut = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        // Time-consuming operation, so it's done only once for all tests
        Stopwatch sw = Stopwatch.StartNew();
        _sut = new FrequencyDataProvider(Settings.FrequencyDictionarySpanish);
        _sut.LoadFrequencyData();
        sw.Stop();

        // Assert
        sw.Elapsed.Should().BeLessOrEqualTo(TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void WhenRealWorldFrequencyDictionaryIsLoaded_FrequenciesForCommonWordsAreReported()
    {
        // Assert
        _sut.GetPosition("el").Should().BeLessOrEqualTo(10, because: "it's 6th most frequent word in a typical large-scale Spanish dataset");
        _sut.GetPosition("la").Should().BeLessOrEqualTo(10, because: "it's 5th most frequent word in a typical large-scale Spanish dataset");
    }

    [TestMethod]
    public void WhenWordDoesNotExistInDictionary_NullIsReturned()
    {
        // Assert
        _sut.GetPosition("e8756d11-2a07-41c0-8ddf-782b5c3ee664").Should().BeNull(because: "A random guid is not a known Spanish word");
    }

    [TestMethod]
    public void WhenWeAskForSameWordButWithDifferentCasing_SameResultShouldBeReported()
    {
        // Arrange
        // Act
        var position1 = _sut.GetPosition("que");
        var position2 = _sut.GetPosition("Que");
        var position3 = _sut.GetPosition("QuE");
        var position4 = _sut.GetPosition("QUE");

        // Assert
        position1.Should().Be(1);
        position2.Should().Be(1);
        position3.Should().Be(1);
        position4.Should().Be(1);
    }

    [DataTestMethod]
    [DataRow("el teléfono", "teléfono")]
    [DataRow("por un lado...", "por un lado")]
    [DataRow("¡Hola!", "hola")]
    [DataRow("¿Cómo?", "cómo")]
    public void FrequencyDataProviderTests(string word, string expectedSanitizedWord)
    {
        // Act
        var sanitizedWord = _sut.SanitizeWordForFrequencyCheck(word);

        // Assert
        sanitizedWord.Should().Be(expectedSanitizedWord);
    }

}
