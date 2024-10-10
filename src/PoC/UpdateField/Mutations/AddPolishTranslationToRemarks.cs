using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients;
using CoreLibrary.Services.ObjectGenerativeFill;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace UpdateField.Mutations;

internal static class AddPolishTranslationToRemarks
{
    const string RemarkId = "pl";

    const string SystemChatMessage =
        "You are an assistant who helps students of Ukrainian language improve their flashcards. Students already know Polish and English.";

    public static List<AnkiNote> LoadNotesThatRequireAdjustment()
    {
        // Load notes that need adjustment
        var notes = AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "s06")
            .Where(x => !x.Remarks.HasRemark(RemarkId))
            .ToList();
        return notes;
    }


    class FlashcardToFill : ObjectWithId
    {

        [Key]
        public string Ukrainian { get; set; }
        public string English { get; set; }

        [FillWithAIRule("The closest Polish equivalent of given term in Ukrainian")]
        [FillWithAIRule("English translation or comment is only provided to clarify the context.")]
        [FillWithAIRule("The output value in 'Polish' should be ready to put onto the reverse side of the flashcard, so don't be verbose (provide a brief translation). Stick to plain text.")]
        [FillWithAIRule("If you encounter Anki tags like <img/> or [sound:...], ignore them as if they weren't in the input.")]
        [FillWithAIRule("Many cards contain two words - the imperfective and perfective form of verbs. In such case, provide translations of both to Polish.")]
        [FillWithAI] public string Polish { get; set; }
    }

    public static async Task AddPolishTranslation(List<AnkiNote> notesChunk)
    {
        if (notesChunk.Count > 40)
            throw new NotImplementedException("Probably too many notes to successfully process in one prompt/call. Did you forget to chunk the collection?");

        AnsiConsole.WriteLine($"Fetching translations from ChatGPT API for a chunk of {notesChunk.Count} notes...");

        var fillModel = notesChunk.Select(x => new FlashcardToFill() { Ukrainian = x.FrontText, English = x.BackText }).ToList();

        var config = new ConfigurationBuilder().AddUserSecrets<FlashcardToFill>().Build();
        var openAiDeveloperKey = config["OPENAI_DEVELOPER_KEY"];
        var openAiOrganizationId = config["OPENAI_ORGANIZATION_ID"];
        var azureOpenAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var azureOpenAiKey = config["AZURE_OPENAI_KEY"];
        var openAiCredentials = new OpenAiCredentials(azureOpenAiEndpoint, azureOpenAiKey, openAiOrganizationId, openAiDeveloperKey);


        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ChatGptClient>();
        var gfLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<GenerativeFill>();
        var chatGptClientCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "UpdateField.ChatGptClient");
        var gfCacheFolder = Path.Combine(Path.GetTempPath(), "FlashcardSpaceToolkitCaches", "UpdateField.GenerativeFill");

        Directory.CreateDirectory(chatGptClientCacheFolder);
        var chatGptClient = new ChatGptClient(logger, openAiCredentials, chatGptClientCacheFolder);
        var settings = new GenerativeFillSettings(gfCacheFolder);
        var generativeFill = new GenerativeFill(gfLogger, chatGptClient, settings);
        var filledNotes = await generativeFill.FillMissingProperties("gpt-4o-2024-08-06", "gpt-4o", fillModel);

        foreach (var filledNote in filledNotes)
        {
            var noteToUpdate = notesChunk.Single(x => x.FrontText == filledNote.Ukrainian);
            var updatedRemarks = noteToUpdate.Remarks.AddOrUpdateRemark(RemarkId, filledNote.Polish);
            noteToUpdate.Remarks = updatedRemarks;
        }
    }
}
