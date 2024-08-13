using PropertyChanged;
using System.Diagnostics;

namespace AnkiCardValidator.ViewModels;

[AddINotifyPropertyChangedInterface]
[DebuggerDisplay("{FrontText} -> {BackText}")]
public class AnkiNote
{
    public AnkiNote(long id, string noteTemplateName, string tags, string fieldsRawOriginal)
    {
        Id = id;
        NoteTemplateName = noteTemplateName;
        FieldsRawOriginal = fieldsRawOriginal;
        Tags = tags;

        // Assumption: this code works on my typical deck with a set of fields I almost always use
        var fields = fieldsRawOriginal.Split('\x1f');

        // works for BothDirections and OneDirection in my collection:
        var frontText = fields[0];
        var frontAudio = fields[1];
        var backText = fields[2];
        var backAudio = fields[3];
        var image = fields[4];
        var remarks = fields[5];

        FrontText = frontText;
        BackText = backText;
        FrontAudio = frontAudio;
        BackAudio = backAudio;
        Image = image;
        Remarks = remarks;
    }


    public string Tags { get; set; }

    /// <summary>
    /// Tagged for removal by the duplicate detection flow.
    /// </summary>
    public bool IsScheduledForRemoval => Tags.Contains(" toDelete ");

    /// <summary>
    /// Tagged for manual resolution by the user in Anki, outside the flow (e.g., both cards present correct, different meanings of word and need clarification)
    /// </summary>
    public bool IsScheduledForManualResolution => Tags.Contains(" toResolveManually ");

    public string FieldsRawCurrent
    {
        get
        {
            // Throw exception if any of the fields contains the Unit Separator character
            var newFieldValueRaw = SerializeFields(FrontText, FrontAudio, BackText, BackAudio, Image, Remarks);
            return newFieldValueRaw;
        }
    }

    public static string SerializeFields(string frontText, string frontAudio, string backText, string backAudio, string image, string remarks)
    {
        if (frontText.Contains('\x1f') ||
            frontAudio.Contains('\x1f') ||
            backText.Contains('\x1f') ||
            backAudio.Contains('\x1f') ||
            image.Contains('\x1f') ||
            remarks.Contains('\x1f'))
        {
            throw new ArgumentException("Field value must not contain the Unit Separator character.");
        }

        var newFieldValueRaw = $"{frontText}\x1f{frontAudio}\x1f{backText}\x1f{backAudio}\x1f{image}\x1f{remarks}";
        return newFieldValueRaw;
    }

    public long Id { get; }
    public string NoteTemplateName { get; }
    public string FieldsRawOriginal { get; init; }

    // fields deserialized:
    public string FrontText { get; set; }
    public string BackText { get; set; }
    public string FrontAudio { get; set; }
    public string BackAudio { get; set; }
    public string Image { get; set; }
    public string Remarks { get; set; }

}
