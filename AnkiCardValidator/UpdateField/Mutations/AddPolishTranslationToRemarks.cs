using AnkiCardValidator.Utilities;
using AnkiCardValidator.ViewModels;

namespace UpdateField.Mutations;
internal class AddPolishTranslationToRemarks
{
    const string SystemChatMessage = "You are an assistant who helps students of Ukrainian language improve their flashcards. Students already know Polish and English.";

    public static async Task AddPolishTranslation(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            var prompt = $@"Your job is to provide the closest translation of a flashcard content from Ukrainian to Polish.
Since flashcards often lack enough context to understand which meaning of the word of phrase I want to focus on, I'll lso provide a translation or comment in English to clarify the context.

If you encounter Anki tags like <img ... /> or [sound ...], just ignore them as if they weren't present in input.

Output the best translation of the provided content to Polish language. Output should be ready to put onto the flashcard reverse side, so don't be verbose (just provide a brief translation), and don't use any formatting (stick to plain text).

The flashcard content to translate is:
```
{note.FrontText}
```

The context of the usage (English translation) is:
```
{note.BackText}
```";

            var response = await ChatGptHelper.GetAnswerToPromptUsingChatGptApi(SystemChatMessage, prompt, 1);
            var updatedRemarks = note.Remarks.AddOrUpdateRemark("polishEquivalent", response);
            note.Remarks = updatedRemarks;
        }

    }
}
