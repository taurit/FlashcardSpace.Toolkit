using CoreLibrary.Services;
using FluentAssertions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AnkiCardValidator.Tests.LocalOnly;

[TestClass]
[TestCategory("SkipInGitHubActions")] // todo: use LFS to commit large dictionary and allow run in pipelines? disable? not sure yet
public class FrequencyDataProviderTest
{
    private static FrequencyDataProvider _sut = null!;

    [SuppressMessage("VS", "IDE0060", Justification = "TestContext is needed for signature match, even when unused")]
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
        // Time-consuming operation, so it's done only once for all tests
        var sw = Stopwatch.StartNew();
        _sut = new FrequencyDataProvider(new NormalFormProvider(), Settings.FrequencyDictionarySpanish);
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
    [DataRow("przed (domem)", "przed")]
    [DataRow("ładny (<i>ang. pretty</i>)", "ładny")]
    [DataRow("wiadomość, news", "wiadomość")] // todo similar tests could be useful in deduplication!
    [DataRow("(jaka) szkoda", "szkoda")]
    [DataRow("depozyt, kaucja (np. na czynsz)", "depozyt")]
    [DataRow("tym... (tym lepiej)", "tym")]
    [DataRow("1) człowiek<br />2) mężczyzna", "człowiek")]
    [DataRow("droga<br />(szlak, metaforyczne pojęcie)", "droga")]
    [DataRow("statek *", "statek")]
    [DataRow("1) terco<br>2) testarudo", "terco")]
    [DataRow("<div>terco</div>", "terco")]
    [DataRow("<div>terco<br/>obstinado</div>", "terco")]
    [DataRow("<div>ter<strong>co</strong><br/>obstinado</div>", "terco")]
    [DataRow("<testTag/>te<testTag>r<testTag />co<testTag  />", "terco")]
    [DataRow("el&nbsp;lavavajillas", "lavavajillas")]
    [DataRow("el&NBSP;lavavajillas", "lavavajillas")]
    [DataRow("<div> </div> <div>&nbsp;zmywarka</div>", "zmywarka")]
    public void FrequencyDataProviderTests(string word, string expectedSanitizedWord)
    {
        // Act
        var sanitizedWord = _sut.SanitizeWordForFrequencyCheck(word);

        // Assert
        sanitizedWord.Should().Be(expectedSanitizedWord);
    }

}
