using BookToAnki.Models;
using BookToAnki.Models.GoogleCloudTranscripts;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace BookToAnki.Services;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public static class GoogleCloudTranscriptReader
{
    private static readonly Dictionary<string, string> WordReplacements = new(StringComparer.OrdinalIgnoreCase) {
        { "дурсслі", "Дурслі" },
        { "дурслі", "Дурслі" },
        { "дурстлі", "Дурслі" },
        { "дурслів", "Дурслі" },
        { "дуслі", "Дурслі" },
        { "дурсліс", "Дурслі" },
        { "дурс", "Дурслі" },
        { "дателів", "Дадлі" },
        { "гранінгс", "Ґраннінґс" },
        { "потерів", "Поттерів" },
        { "привід", "прівіт" },
        { "drive", "драйв" },
        { "матію", "мантію" },
        { "комагонекл", "Макґонеґел" },
        { "макконекел", "Макґонеґел" },
        { "макгоннеґел", "Макґонеґел" },
        { "макгонекл", "Макґонеґел" },
        { "магли", "маґли" },
        { "маккли", "маґли" },
        { "маглібські", "маґлівські" },
        { "дамблдор", "Дамблдор" },
        { "дамблтора", "Дамблдора" },
        { "макклі", "маґлів" },
        { "гоплінга", "ґобліна" },
        { "гобліна", "ґобліна" },
        { "гоблін", "ґоблін" },
        { "гегрід", "Геґрід" },
        { "гетрід", "Геґрід" },
        { "гекріт", "Геґрід" },
        { "геквітові", "Геґрідові" },
        { "гогварсом", "Гоґвортсом" },
        { "олівантер", "Олівандер" },
        { "діагон", "Діаґон" },
        { "паддінгтон", "Педінґтон" },
        { "потире", "Поттере" },
        { "грифіндор", "Ґрифіндор" },
        { "маггів", "маґлів" },
        { "малфой", "мелфой" },
        { "мелпой", "мелфой" },
        { "маклівських", "маґлівських" },
        { "локорда", "локарта" },
        { "лукарда", "локарта" },
        { "лукарта", "локарта" },
        { "гільдероя", "ґільдероя" },
        { "гільдеро", "ґільдеро" },

        // if word is correct, but has an equivalent, add it to SentenceMatcher exception list instead

    };

    public static Transcript ReadTranscript(string transcriptFileName, string audioFilePath)
    {
        var content = File.ReadAllText(transcriptFileName);

        // newer Google text to speech v2 results seem to differ by names of those fields
        content = content.Replace("\"startOffset\"", "\"StartTime\"")
            .Replace("\"endOffset\"", "\"EndTime\"");


        var transcript = JsonSerializer.Deserialize<GoogleCloudTranscriptJsonModel>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // flatten, and only keep alternatives with maximum confidence
        var fragments = transcript.Results.Select(x => x.Alternatives).Where(x => x is not null);
        List<GoogleCloudTranscriptAlternativeJsonModel> bestAlternatives = new List<GoogleCloudTranscriptAlternativeJsonModel>();
        foreach (var fragment in fragments)
        {
            var bestAlternative = fragment.Where(x => x.Words != null).MaxBy(x => x.Confidence);
            if (bestAlternative is not null)
                bestAlternatives.Add(bestAlternative);
        }

        var words = bestAlternatives.SelectMany(x => x.Words).ToList();

        // Convert startTime and endTime to seconds

        // hack: adjust some own names
        var adjustedWords = new List<GoogleCloudTranscriptWord>(words.Count);
        foreach (var word in words.Where(x => x.Word is not null))
        {
            var startTimeInSeconds = double.Parse(word.StartTime.TrimEnd('s'), CultureInfo.InvariantCulture);
            var endTimeInSeconds = double.Parse(word.EndTime.TrimEnd('s'), CultureInfo.InvariantCulture);
            var wordString = WordReplacements.TryGetValue(word.Word!, out var replacement) ? replacement : word.Word!;
            var wordToAdd = new GoogleCloudTranscriptWord(wordString, startTimeInSeconds, endTimeInSeconds);
            adjustedWords.Add(wordToAdd);
        }

        return new Transcript(adjustedWords, audioFilePath);
    }
}
