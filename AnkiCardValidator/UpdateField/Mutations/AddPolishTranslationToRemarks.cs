using AnkiCardValidator;
using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;
using Spectre.Console;

namespace UpdateField.Mutations;
internal static class AddPolishTranslationToRemarks
{
    const string RemarkId = "pl";
    const string SystemChatMessage = "You are an assistant who helps students of Ukrainian language improve their flashcards. Students already know Polish and English.";

    public static List<AnkiNote> LoadNotesThatRequireAdjustment()
    {
        // Load notes that need adjustment
        var notes = AnkiHelpers.GetNotes(Settings.AnkiDatabaseFilePath, limitToTag: "addSmartExampleUkr")
            .Where(x => !x.Remarks.HasRemark(RemarkId))
            .ToList();
        ;
        return notes;
    }

    public static async Task AddPolishTranslation(List<AnkiNote> notes)
    {
        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var fetchTranslationsTask = ctx.AddTask("[green]Fetching translations from ChatGPT API...[/]");
                fetchTranslationsTask.MaxValue = notes.Count;

                foreach (var note in notes)
                {
                    var prompt = $@"Given input in Ukrainian, provide the closest equivalent in Polish. Be as accurate as possible.
English translation or comment is provided to clarify the context.
If you encounter Anki tags like <img/> or [sound:...], ignore them as if they weren't in the input.

The output should be ready to put onto the reverse side of the flashcard, so don't be verbose (provide a brief translation). Stick to plain text. Do NOT wrap output in backticks.

The flashcard content to translate is:
```
{note.FrontText}
```
The context of the usage (English equivalent) is:
```
{note.BackText}
```";

                    var responseFileName = await ChatGptHelper.GetAnswerToPromptUsingChatGptApi(SystemChatMessage, prompt, 1, false);
                    var response = await File.ReadAllTextAsync(responseFileName);
                    var responseUnwrapped = StringHelpers.RemoveBackticksBlockWrapper(response);
                    var updatedRemarks = note.Remarks.AddOrUpdateRemark(RemarkId, responseUnwrapped);
                    note.Remarks = updatedRemarks;

                    fetchTranslationsTask.Increment(1);
                }
            });

    }
}
