using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookToAnki.NotePropertiesDatabase;

[Table("NotePreferences")]
[PrimaryKey(nameof(Word), nameof(Sentence), nameof(PropertyName))]
public class NotePropertiesEntity
{
    public NotePropertiesEntity(string word, string sentence, string propertyName)
    {
        Word = word;
        Sentence = sentence;
        PropertyName = propertyName;
    }

    /// <summary>
    /// A part of compound key
    /// </summary>
    public string Word { get; set; }

    /// <summary>
    /// A part of compound key
    /// </summary>
    public string Sentence { get; set; }

    /// <summary>
    /// A part of compound key
    /// </summary>
    public string PropertyName { get; set; }

    public string? PropertyValue { get; set; }

}
