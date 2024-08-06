using AnkiCardValidator.ViewModels;

namespace UpdateField.Mutations;
internal class AddPolishTranslationToRemarks
{
    public static void AddPolishTranslation(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            if (note.BackText.Contains(" "))
            {
                note.BackText = note.BackText.Replace(" ", "&nbsp;");
            }
        }

    }
}
