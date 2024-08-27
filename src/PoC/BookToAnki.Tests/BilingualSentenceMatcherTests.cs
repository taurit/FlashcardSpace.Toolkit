using BookToAnki.Models;
using BookToAnki.Services;
using CoreLibrary.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Text;

namespace BookToAnki.Tests;

[TestClass]
public class BilingualSentenceMatcherTests
{
    private readonly IWordTokenizer _wordTokenizer = new WordTokenizer();

    [TestMethod]
    public void When_ThereIsNoMatch_Expect_EmptyListOfMatchesAndSuccessRate0Percent()
    {
        // Arrange
        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        var language1 = new BilingualSentence("Hello", "Cześć");
        var language2 = new BilingualSentence("Siemka", "Hi");

        // Act
        var match = sut.Match([language1], [language2]);

        // Assert
        match.MatchedSentences.Should().BeEmpty();
        match.SuccessRatePercent.Should().BeApproximately(0f, 0.01f);
    }

    [TestMethod]
    public void When_ThereIsAnExactMatchInOneMachineTranslation_Expect_Match()
    {
        // Arrange
        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        var language1 = new BilingualSentence("Hello", "Cześć");
        var language2 = new BilingualSentence("Cześć", "Hi");

        // Act
        var match = sut.Match([language1], [language2]);

        // Assert
        match.MatchedSentences.Should().HaveCount(1);
        match.MatchedSentences[0].PrimaryLanguage.Should().Be("Hello");
        match.MatchedSentences[0].SecondaryLanguage.Should().Be("Cześć");
        match.SuccessRatePercent.Should().BeApproximately(100f, 0.01f);
    }

    [TestMethod]
    public void When_ThereIsAnExactMatchInAlternativeMachineTranslation_Expect_Match()
    {
        // Arrange
        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        var language1 = new BilingualSentence("Hello", "Siemka");
        var language2 = new BilingualSentence("Cześć", "Hello");

        // Act
        var match = sut.Match([language1], [language2]);

        // Assert
        match.MatchedSentences.Should().HaveCount(1);
        match.MatchedSentences[0].PrimaryLanguage.Should().Be("Hello");
        match.MatchedSentences[0].SecondaryLanguage.Should().Be("Cześć");
        match.SuccessRatePercent.Should().BeApproximately(100f, 0.01f);
    }

    [TestMethod]
    public void When_OnlyOneOfTwoSentencesMatch_Expect_50percentSuccessRate()
    {
        // Arrange
        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        var language1_sentence1 = new BilingualSentence("Hello", "Cześć");
        var language1_sentence2 = new BilingualSentence("aaa", "bbb");

        var language2_sentence1 = new BilingualSentence("ccc", "ddd");
        var language2_sentence2 = new BilingualSentence("Cześć", "Hello");


        // Act
        var match = sut.Match([language1_sentence1, language1_sentence2],
            [language2_sentence1, language2_sentence2]);

        // Assert
        match.MatchedSentences.Should().HaveCount(1);
        match.MatchedSentences[0].PrimaryLanguage.Should().Be("Hello");
        match.MatchedSentences[0].SecondaryLanguage.Should().Be("Cześć");
        match.SuccessRatePercent.Should().BeApproximately(50f, 0.01f);
    }

    [DataRow("Hello", "Hello")]
    [DataRow("Hello", "heLLo")]
    [DataRow("They should never be compared for equality.", "They should never be compared for QUALITY.")]
    [DataRow("The number is between 3.139 and 3.141.", "The number is between THREE and FOUR")]
    [DataRow("Хлопчик, який вижив", "Хлопчик, що вижив.")]
    [DataRow("— РОЗДІЛ ПЕРШИЙ —.", "РОЗДІЛ ПЕРШИЙ.")]
    [DataRow("Містер і місіс Дурслі з будинку номер чотири на Прайвіт-драйв із гордістю сказали, що вони абсолютно нормальні, щиро дякую.", "Містер і місіс Дурслі, що жили в будинку номер чотири на вуличці Прівіт-драйв, пишалися тим, що були, слава Богу, абсолютно нормальними.")]
    [DataRow("Містер Дурслі був директором фірми Grunnings, яка виготовляла свердла.", "Містер Дурслі керував фірмою \"Ґраннінґс\", яка виготовляла свердла.")]
    [DataRow("Вони не думали, що зможуть винести, якщо хтось дізнається про Поттерів.", "Їм здавалося, що вони помруть, коли хтось почує про Поттерів.")]
    [DataRow("The police had never read an odder report.", "The police have never seen a stranger report.")]
    [DataRow("But Frank did not leave.", "But Frank did not go away.")]
    [DataRow("The wealthy owner continued to pay Frank to do the gardening, however.", "However, the rich man continued to pay Frank for his work.")]
    [DataRow("The fire, he now saw, had been lit in the grate.", "Now he could see the fire flickering in the fireplace.")]
    [DataRow("This surprised him.", "This surprised him quite a bit.")]
    [DataRow("Move me closer to the fire, Wormtail.", "\"Put me closer to the fire, Wormtail.\"")]
    [DataRow("Then he disappeared from sight again.", "Then he disappeared again.")]
    [DataRow("My Lord, may I ask how long we are going to stay here?", "\"My lord, may I ask, how long shall we stay here?\"")]
    [DataRow("It would be foolish to act before the Quidditch World Cup is over.", "It's pointless to start until the Quidditch World Cup is over.")]
    [DataRow("Frank inserted a gnarled finger into his ear and rotated it.", "Frank stuck his hooked finger in his ear and twirled it.")]
    [DataRow("So we wait.", "So we should wait.")]
    [DataRow("A few more months will make no difference.", "A few more months won't matter.")]
    [DataRow("I only wish that I could do it myself, but in my present condition…", "I'd rather do it myself, but in my current situation…")]
    [DataRow("The scar", "Scar")]
    [DataRow("The Invitation.", "Invitation.")]
    [DataRow("Hoping to see Harry soon,.", "Hope to see Harry soon,.")]
    [DataRow("Гедвіґа з гідністю ухнула.", "Гедвіґа вигукнула з гідністю.")]
    [DataTestMethod]
    public void When_MostWordsInASentenceMatch_Expect_Match(string sentence1, string sentence2)
    {
        // Arrange
        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        // Act
        var match = sut.SentenceMatches(sentence1, sentence2);

        // Assert
        match.Should().BeTrue();
    }

    /// <summary>
    /// Important to guard quality of examples: makes sure we don't match sentences that are different! False positives degrate quality of generated deck
    /// </summary>
    /// <param name="sentence1"></param>
    /// <param name="sentence2"></param>
    [DataRow("The Boy Who Lived", "In fact, Mrs. Dursley pretended she didn't have a sister at all, because Mrs. Potter and her pathetic husband were completely different kinds of people.")]
    [DataRow("they didn't want Dudley around such a child.", "Mr. Dursley turned the corner and drove onto the highway, watching the cat in the mirror.")]
    [DataRow("He yelled at five employees in turn.", "‘I suppose so,’ said Mrs Dursley stiffly.")]
    [DataRow("So much was being done to stop him, until suddenly...", "for God's sake, how did Harry manage to survive?")]
    [DataRow("Professor McGonagall took out a lace handkerchief and began to dry her eyes under her glasses.", "I couldn't read anything from it, though, because he put it back in his pocket and said:")]
    [DataRow("Enough of this sleeping!", "Harry woke up and jumped on the bed.")]
    [DataRow("Dudley's birthday - how could he have forgotten it!", "\"Thirty-six,\" he announced, looking at his mother and father.")]
    [DataRow("Не буду!", "Гаррі озирнувся через плече, але все одно там нікого не було.")]
    [DataRow("І на завершення:", "Чи справді він був у кімнаті, повній невидимих ​​людей, і хитрість цього дзеркала полягала в тому, що воно відбивало їх, невидимі чи ні?")]
    [DataRow("Заїжджаючи на подвір'я будинку номер чотири, Дурслі відразу зауважив (і це не поліпшило його настрою) смугасту кицьку, яку бачив уранці.", "Він знову подивився в дзеркало.")]
    [DataRow("не хочу в це вірити…", "Дядько Вернон мало не врізався в машину попереду.")]
    [DataRow("— Хіба я знаю?", "Я знаю, що ні, — сказав Гаррі.")]
    [DataRow("Це був лише сон.", "Ніхто не знає, чому і як, але кажуть, ніби, не спромігшись убити Гаррі Поттера, Волдеморт утратив свою силу, — і саме тому його не стало.")]
    [DataRow("Але не зміг…", "Але він шкодував, що нічого не сказав.")]
    [DataRow("Берту я вбити мусив.", "вбити мене теж?")]
    [DataRow("I had to kill Berta", "to kill me, too?")]
    [DataRow("TWENTY-ONE.", "— CHAPTER ONE —.")]
    [DataRow("The House-Elf Liberation Front.", "The Radley House.")]
    [DataRow("Later,’ said a second voice.", "asked a cold voice.")]
    [DataRow("A week,’ said the cold voice.", "asked a cold voice.")]
    [DataRow("Certainly I am determined, Wormtail.", "\"I always feel everything, Wormtail!\"")]
    [DataRow("The boy is nothing to me, nothing at all!", "You regret that you came back to me at all.")]
    [DataRow("Френк не розумів, що діється.", "Френк не мав можливості сховатися.")]
    [DataRow("Ніхто не знає, що ти тут.", "— Це означає, що ти не чаклун.")]
    [DataRow("It was all becoming confused;", "Was it scar pain?")]
    [DataRow("All the curtains were closed.", "Harry closed the book.")]
    [DataRow("See you five o’clock tomorrow.", "See you soon — Ron.")]
    [DataRow("then a quarter past five…", "Ten past five...")]
    [DataRow("No consideration at all.", "If they come at all.")]
    [DataRow(" said Harry.", "Harry jumped.")]
    [DataRow("Що це?", "— Що сталося?")]
    [DataRow("Harry, can you hear us?", "\"Dad, maybe Harry will hear us...\"")]
    [DataTestMethod]
    public void When_SentencesDoNotMatch_Expect_NoMatch(string sentence1, string sentence2)
    {
        // Arrange
        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        // Act
        var match = sut.SentenceMatches(sentence1, sentence2);

        // Assert
        match.Should().BeFalse();
    }

    [DataRow(1, "en", "uk")]
    [DataRow(1, "uk", "en")]
    [DataRow(1, "en", "pl")]
    [DataRow(1, "pl", "en")]
    [DataRow(1, "pl", "uk")]
    [DataRow(1, "uk", "pl")]

    [DataRow(2, "en", "uk")]
    [DataRow(2, "uk", "en")]
    [DataRow(2, "en", "pl")]
    [DataRow(2, "pl", "en")]
    [DataRow(2, "pl", "uk")]
    [DataRow(2, "uk", "pl")]

    [DataRow(3, "en", "uk")]
    [DataRow(3, "uk", "en")]
    [DataRow(3, "en", "pl")]
    [DataRow(3, "pl", "en")]
    [DataRow(3, "pl", "uk")]
    [DataRow(3, "uk", "pl")]

    [DataRow(4, "en", "uk")]
    [DataRow(4, "uk", "en")]
    [DataRow(4, "en", "pl")]
    [DataRow(4, "pl", "en")]
    [DataRow(4, "pl", "uk")]
    [DataRow(4, "uk", "pl")]
    [DataTestMethod]
    public Task When_FullEbookIsMatched_Expect_SuccessRateAbove35Percent(int bookPart, string language1, string language2)
    {
        LicensedContentGuard.EnsureLicensedContentIsAvailableOnTheMachine();

        var sut = new BilingualSentenceMatcher(_wordTokenizer);

        // Arrange
        var pathOriginalsL1 = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_{language1}_0{bookPart}.sentences.original.txt");
        var pathTranslationsL1 = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_{language1}_0{bookPart}.sentences.translated_to_{language2}.txt");

        var pathOriginalsL2 = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_{language2}_0{bookPart}.sentences.original.txt");
        var pathTranslationsL2 = Path.Combine(LicensedContentGuard.RootFolderForLicensedContent, $"hp_{language2}_0{bookPart}.sentences.translated_to_{language1}.txt");

        var bookContentL1 = sut.LoadSentencesWithMachineTranslations(pathOriginalsL1, pathTranslationsL1);
        var bookContentL2 = sut.LoadSentencesWithMachineTranslations(pathOriginalsL2, pathTranslationsL2);

        // Prepare embeddings
        //var userSecrets = UserSecretsRetriever.GetUserSecrets();
        //var embeddingsCacheManager = new EmbeddingsCacheManager("d:\\Flashcards\\Words\\ukrainian_sentences_embeddings.bin");
        //var embeddings = new EmbeddingsServiceWrapper(userSecrets.OpenAiDeveloperKey, userSecrets.OpenAiOrganization, embeddingsCacheManager);

        //await embeddings.PrimeEmbeddingsCache(bookContentL1.Select(x => x.PrimaryLanguage).Distinct().ToList());

        // Act
        var match = sut.Match(bookContentL1, bookContentL2);

        // Assert

        // debug
        //WriteOutputToHtmlFile(match, bookContentL1);

        Console.WriteLine($"Success rate: {match.SuccessRatePercent:#.##}%");

        using (new AssertionScope())
        {
            match.MatchedSentences.Should().NotBeEmpty();
            match.SuccessRatePercent.Should().BeGreaterThan(35);
        }

        return Task.CompletedTask;
    }

    private static void WriteOutputToHtmlFile(BilingualSentenceMatchingResult match, List<BilingualSentence> bookContentOriginalLanguage)
    {
        var matchesLookup = match.MatchedSentences.ToLookup(x => x.PrimaryLanguage);
        StringBuilder sb = new StringBuilder();
        for (var sIndex = 0; sIndex < bookContentOriginalLanguage.Count; sIndex++)
        {
            var s = bookContentOriginalLanguage[sIndex];

            sb.AppendLine($"<b>{sIndex}</b>:");
            if (matchesLookup.Contains(s.PrimaryLanguage))
            {
                var pairedHumanTranslation = matchesLookup[s.PrimaryLanguage].First().SecondaryLanguage;
                sb.AppendLine($"<font color='green'>{s.PrimaryLanguage}</font><br />");
                sb.AppendLine($"<font color='black'>{pairedHumanTranslation}</font><br />");
                sb.AppendLine($"<font color='blue'>{s.SecondaryLanguage}</font><br />");
                sb.AppendLine("<br />");
            }
            else
            {
                sb.AppendLine($"<font color='red'>{s.PrimaryLanguage}</font><br />");
                sb.AppendLine("<br />");
            }
        }

        var outputHtml = "d:/output.html";
        File.WriteAllText(outputHtml, sb.ToString());
    }
}
