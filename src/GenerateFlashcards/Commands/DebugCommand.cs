using CoreLibrary.Services.GenerativeAiClients.StableDiffusion;
using CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
using CoreLibrary.Utilities;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

/// <summary>
/// A temporary command just for convenience of testing some pieces code with "F5" (Run) in Visual Studio.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DebugCommand(
        TextToSpeechClient ttsClient,
        ImageGenerator imageGenerator
    ) : AsyncCommand<DebugCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DebugCommandSettings settings)
    {
        var prompt = "woman surgeon performing heart operation, (extremely detailed 8k wallpaper), Intricate, High Detail, Sharp focus, smiling";
        var negativePrompt = "lowres,bad anatomy,bad hands,text,error,missing fingers,extra digit,fewer digits,cropped,worst quality,low quality,jpeg artifacts,signature,watermark,username,blurry,nsfw,painting,drawing,illustration,cartoon,anime,sketch";

        // test - measure time of all operations
        var rootOutputFolder = "d:/test5-10vs24steps";
        rootOutputFolder.EnsureDirectoryExists();
        var draftsPaths = new List<string>();

        // 1) Generate 10 drafts of images
        Stopwatch s = Stopwatch.StartNew();
        for (int i = 0; i < 10; i++)
        {
            var generatedImage = await imageGenerator.GenerateImageBatch(
                new StableDiffusionPrompt(prompt, negativePrompt),
                1, 4.0m, i * 100, SupportedSDXLImageSize.Wide, ImageQualityProfile.DraftProfile);
            var imageBytes = Convert.FromBase64String(generatedImage[0].Base64EncodedImage);
            // save to root folder
            var draftPath = $"{rootOutputFolder}/draft{i}.jpg";
            draftsPaths.Add(draftPath);
            await File.WriteAllBytesAsync(draftPath, imageBytes);
        }
        s.Stop();
        AnsiConsole.MarkupLine($"[bold green]Generated 10 drafts in {s.ElapsedMilliseconds} ms[/]");

        // 2) Upgrade quality of all drafts with await imageGenerator.ImproveImageQualityIfNeeded("d:\\draft.jpg");
        s.Restart();
        foreach (var draftPath in draftsPaths)
        {
            await imageGenerator.ImproveImageQualityIfNeeded(draftPath);
        }
        s.Stop();

        AnsiConsole.MarkupLine($"[bold green]Improved quality of 10 drafts in {s.ElapsedMilliseconds} ms[/]");

        return 0;
    }


    private async Task GenerateAudioFile()
    {
        (await ttsClient.GenerateAudioFile("La casa es grande y bonita.", SupportedLanguage.Spanish)).SaveToTemporaryFileAndPlay();
        (await ttsClient.GenerateAudioFile("dom", SupportedLanguage.Polish)).SaveToTemporaryFileAndPlay();
        (await ttsClient.GenerateAudioFile("a house", SupportedLanguage.English)).SaveToTemporaryFileAndPlay();

        await Task.Delay(1000);
    }


    private record WordOccurrence(string Word, Int64 NumOccurrences);
    /// <summary>
    /// Takes frequency dictionary containing words from Wikipedia and number of their occurrences.
    /// Displays statistics about how much of the total words used is covered by the first N lines.
    /// </summary>
    private static void CalculateBestCutOffLine()
    {
        var frequencyDictionary = File.ReadAllLines(
            "d:\\Projekty\\FlashcardSpace.Toolkit\\src\\GenerateFlashcards\\Resources\\FrequencyDictionaries\\Spanish.Words.txt"
            );

        var occurrences = frequencyDictionary.Select(line => line.Split(' ')).Select(x => new WordOccurrence(x[0], Int64.Parse(x[1]))).ToList();

        // calculate total number of occurrences
        decimal totalOccurrencesInDictionary = occurrences.Sum(entry => entry.NumOccurrences);
        decimal processedRatio = 0;

        int lineNumber = 0;
        foreach (var line in occurrences)
        {
            var lineContributionToProcessedRatio = line.NumOccurrences / totalOccurrencesInDictionary;
            processedRatio += lineContributionToProcessedRatio;

            if ((lineNumber % 1000) == 0)
            {
                Console.WriteLine($"Line {lineNumber}: {processedRatio * 100:F2}% of language use covered already. ({line.Word} with {line.NumOccurrences} occurrences)");
            }
            lineNumber++;
        }
    }
}

static class AudioFileExtensions
{

    public static async void SaveToTemporaryFileAndPlay(this byte[] audioFile)
    {
        var audioFilePath = $"d:/testAAA.mp3";
        await File.WriteAllBytesAsync(audioFilePath, audioFile);
        var processStartInfoA = new ProcessStartInfo(audioFilePath) { UseShellExecute = true };
        Process.Start(processStartInfoA);
    }
}
