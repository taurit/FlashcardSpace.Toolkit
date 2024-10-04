using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace BookToAnki.Tests;

/// <summary>
/// Tests that can be run manually during development, but are not meant to be run regularly.
/// Typically: ones that use external services, like Dall-A API.
/// </summary>
[TestClass]
[TestCategory("SkipInGitHubActions")] // todo: experimental code and not tests, really
[Ignore]
public class ManualTests
{
    [Ignore]
    [TestMethod]
    public async Task GenerateImageUsingDalleApi()
    {
        var userSecrets = UserSecretsRetriever.GetUserSecrets();
        var sut = new DalleServiceWrapper(userSecrets.OpenAiDeveloperKey, userSecrets.OpenAiOrganizationId);

        // Act
        var prompt = "A portrait of a woman with long hair, outdoor, in elegant dress, posing in blooming garden, golden hour";
        var imageName = "garden";
        var quality = DalleServiceWrapper.Dalle3ImageQuality.hd;

        Stopwatch s = Stopwatch.StartNew();
        var image = await sut.CreateDalle3Image(prompt, quality);
        s.Stop();
        var imageBytes = Convert.FromBase64String(image.ImageContentBase64);
        await File.WriteAllBytesAsync(Path.Combine("d:\\DalleParametersCompared\\q\\", $"{imageName}.{quality}.webp"), imageBytes);
        Console.WriteLine($"Total time: {s.Elapsed.TotalMilliseconds}");
    }

    [Ignore]
    [DataRow("""d:\Flashcards\Books\alice_in_wonderland\alice_en.txt""")]
    [DataTestMethod]
    public void ConvertEbookToSentencesAndSaveToFile(string ebookFileName)
    {
        // Arrange
        var ukrainianWordExplainer = new UkrainianWordExplainer(null, null);
        var bookLoader = new BookLoader(ukrainianWordExplainer);

        // Act
        var allSentences = bookLoader
            .LoadSentencesFromBook(ebookFileName)
            .Select(x => x.Text);

        var outputFileName = Path.ChangeExtension(ebookFileName, "sentences.original.txt");

        File.WriteAllLines(outputFileName, allSentences);
    }

    [Ignore]
    [TestMethod]
    public async Task CompareEmbeddings()
    {
        var userSecrets = UserSecretsRetriever.GetUserSecrets();
        var embeddingsCacheManager = new EmbeddingsCacheManager("d:\\Flashcards\\Words\\ukrainian_sentences_embeddings_3s.bin");
        var sut = new EmbeddingsServiceWrapper(userSecrets.OpenAiDeveloperKey, userSecrets.OpenAiOrganizationId, embeddingsCacheManager);

        // Act
        var examplesEn = new[] {
    "Mr and Mrs Dursley, of number four, Privet Drive, were proud to say that they were perfectly normal, thank you very much.",
    "They were the last people you’d expect to be involved in anything strange or mysterious, because they just didn’t hold with such nonsense.",
    "Mr Dursley was the director of a firm called Grunnings, which made drills.",
    "He was a big, beefy man with hardly any neck, although he did have a very large moustache.",
    "Mrs Dursley was thin and blonde and had nearly twice the usual amount of neck, which came in very useful as she spent so much of her time craning over garden fences, spying on the neighbours.",
    "The Dursleys had a small son called Dudley and in their opinion there was no finer boy anywhere.",
    "The Dursleys had everything they wanted, but they also had a secret, and their greatest fear was that somebody would discover it.",
    "They didn’t think they could bear it if anyone found out about the Potters.",
    "Mrs Potter was Mrs Dursley’s sister, but they hadn’t met for several years; in fact, Mrs Dursley pretended she didn’t have a sister, because her sister and her good-for-nothing husband were as unDursleyish as it was possible to be.",
    "The Dursleys shuddered to think what the neighbours would say if the Potters arrived in the street.",
    "The Dursleys knew that the Potters had a small son, too, but they had never even seen him.",
    "This boy was another good reason for keeping the Potters away; they didn’t want Dudley mixing with a child like that.",
    "dog"
        };

        var examplesPl = new[] {
    "Państwo Dursleyowie spod numeru czwartego przy Privet Drive mogli z dumą twierdzić, że są całkowicie normalni, chwała Bogu.",
    "Byli ostatnimi ludźmi, których można by posądzić o udział w czymś dziwnym lub tajemniczym, bo po prostu nie wierzyli w takie bzdury.",
    "Pan Dursley był dyrektorem firmy Grunnings produkującej świdry.",
    "Był to rosły, otyły mężczyzna pozbawiony szyi, za to wyposażony w wielkie wąsy.",
    "Natomiast pani Dursley była drobną blondynką i miała szyję dwukrotnie dłuższą od normalnej, co bardzo jej pomagało w życiu, ponieważ większość dnia spędzała na podglądaniu sąsiadów.",
    "Syn Dursleyów miał na imię Dudley, a rodzice uważali go za najwspanialszego chłopca na świecie.",
    "Dursleyowie mieli wszystko, czego dusza zapragnie, ale mieli też swoją tajemnicę i nic nie budziło w nich większego przerażenia, jak myśl, że może zostać odkryta.",
    "Uważali, że znaleźliby się w sytuacji nie do zniesienia, gdyby ktoś dowiedział się o istnieniu Potterów.",
    "Pani Potter była siostrą pani Dursley, ale nie widziały się od wielu lat; Prawdę mówiąc, pani Dursley udawała, że w ogóle nie ma siostry, ponieważ pani Potter i jej żałosny mąż byli ludźmi całkowicie innego rodzaju.",
    "Dursleyowie wzdrygali się na samą myśl, co by powiedzieli sąsiedzi, gdyby Potterowie pojawili się na ich ulicy.",
    "Oczywiście wiedzieli, że Potterowie też mają synka, ale nigdy nie widzieli go na oczy i z całą pewnością nie chcieli go nigdy oglądać.",
    "Ten chłopiec był jeszcze jednym powodem, by Dursleyowie trzymali się jak najdalej od Potterów; nie życzyli sobie, by Dudley przebywał w towarzystwie takiego dziecka.",
    "Losowe zdanie, żebo zobacyzć która os jest dla przykładów po polsku.",
    "puppy"

    };

        await sut.PrimeEmbeddingsCache(examplesPl);
        await sut.PrimeEmbeddingsCache(examplesEn);

        var result = new double[examplesPl.Length, examplesEn.Length];

        for (int pl = 0; pl < examplesPl.Length; pl++)
        {
            var plEmbedding = await sut.CreateEmbedding(examplesPl[pl]);
            for (int en = 0; en < examplesEn.Length; en++)
            {
                var enEmbedding = await sut.CreateEmbedding(examplesEn[en]);
                var cosineSimilarity = sut.CosineSimilarity(plEmbedding, enEmbedding);
                Console.WriteLine($"Cosine Similarity {pl}-{en}: {cosineSimilarity}");
                result[pl, en] = cosineSimilarity;
            }
        }

        DisplayPlot(result);
    }

    private void DisplayPlot(double[,] result)
    {
        var resultAsString = ArrayToString(result);

        var htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <script src=""https://cdn.plot.ly/plotly-latest.min.js""></script>
</head>
<body>

<div id=""heatmap""></div>

<script>
    var data = [
        {{
            z: {resultAsString},
            type: 'heatmap'
        }}
    ];

    Plotly.newPlot('heatmap', data);
</script>

</body>
</html>
";
        var fileName = "d:\\plotly-output.temp.html";
        File.WriteAllText(fileName, htmlContent);

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = true // Important for .NET Core
        };
        Process.Start(psi);
    }

    public string ArrayToString(double[,] array)
    {
        var rows = array.GetLength(0);
        var columns = array.GetLength(1);
        var sb = new StringBuilder();

        sb.Append("[");

        var maxInRow = new double[rows];
        var maxInColumn = new double[columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                var val = array[i, j];
                if (val > maxInRow[i]) maxInRow[i] = val;
                if (val > maxInColumn[j]) maxInColumn[j] = val;
            }
        }

        for (int i = 0; i < rows; i++)
        {
            sb.Append("[");
            for (int j = 0; j < columns; j++)
            {
                var value = array[i, j];

                sb.Append(array[i, j].ToString(CultureInfo.InvariantCulture).Replace(",", "."));

                //sb.Append(Math.Abs(value - maxInColumn[j]) < 0.00001 ? 1 : 0);
                if (j < columns - 1)
                    sb.Append(", ");
            }
            sb.Append("]");
            if (i < rows - 1)
                sb.Append(", ");
        }

        sb.Append("]");

        return sb.ToString();
    }

}
