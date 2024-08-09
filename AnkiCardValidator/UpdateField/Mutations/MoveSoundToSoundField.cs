using AnkiCardValidator.ViewModels;
using UpdateField.Utilities;

namespace UpdateField.Mutations;
public static class MoveSoundToSoundField
{
    public static void RunMigration(List<AnkiNote> notes)
    {
        foreach (var note in notes)
        {
            MigrateSoundToFrontAudioField(note);
        }
    }

    /// <summary>
    /// If a given note has an image in the FrontText field or BackText field, move it to the Image field.
    /// </summary>
    public static void MigrateSoundToFrontAudioField(AnkiNote note)
    {
        if (!String.IsNullOrWhiteSpace(note.FrontAudio))
        {
            return;
        }

        // Use HTMLAgilityPack to detect if there's an image in the FrontText or BackText field
        MutationHelpers.MoveSoundTagFromFrontTextToFrontAudio(note);

    }
}
