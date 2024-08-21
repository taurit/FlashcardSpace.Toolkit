using Anki.NET.Helpers;

namespace Anki.NET.Models;

internal class Note
{
    public Note(AnkiDeckModel ankiDeckModel, AnkiItem ankiItem)
    {
        NoteId = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var fields = ankiDeckModel.FieldList;
        var guidForSyncPurposes = ((ShortGuid)Guid.NewGuid()).ToString().Substring(0, 10);
        var modelId = ankiItem.Mid;
        var modificationTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        var fieldsConcatenated = GeneralHelper.ConcatFields(fields, ankiItem, "\x1f");
        var sortField = ankiItem[fields[0].Name].ToString();
        var checksum = GeneralHelper.CheckSum(sortField);

        Query = "INSERT INTO notes VALUES(" + NoteId + ", '" + guidForSyncPurposes + "', " + modelId + ", " + modificationTimestampSeconds + ", -1, '  ', '" + fieldsConcatenated + "', '" +
                sortField + "', " + checksum + ", 0, '');";
    }

    internal long NoteId { get; }
    internal string Query { get; }
}
