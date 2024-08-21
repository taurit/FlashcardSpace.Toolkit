using BookToAnki.Models;
using PropertyChanged;
using System;
using System.Diagnostics;

namespace BookToAnki.UI.ViewModels;

[DebuggerDisplay("{Word.Word}, HasPicture={HasPicture}, IsLinkedToSelectedWord={IsLinkedToSelectedWord}, Sim={SimilarityScore}")]
[AddINotifyPropertyChangedInterface]
public sealed class WordDataViewModel(WordData word, bool hasPicture, bool isLinkedToSelectedWord, bool isLinkedToAnyWord, string? partOfSpeech)
{
    public WordData Word { get; } = word ?? throw new ArgumentNullException(nameof(word));
    public bool HasPicture { get; set; } = hasPicture;
    public string? PartOfSpeech { get; } = partOfSpeech;
    public bool IsLinkedToAnyWord { get; set; } = isLinkedToAnyWord;

    [DependsOn(nameof(HasPicture))]
    public string HasPictureText => HasPicture ? "🖼️" : "";

    public int Occurrences => Word.Occurrences;

    // todo consider refactoring to another class, e.g. SimilarWordDataViewModel : WordDataViewModel
    // those properties only make sense in the context of "similar words" pane, in relation to a word selected in the main words pane
    public bool IsLinkedToSelectedWord { get; set; } = isLinkedToSelectedWord;
    [DependsOn(nameof(IsLinkedToAnyWord), nameof(IsLinkedToSelectedWord))]
    public bool IsLinkedToNonSelectedWord => IsLinkedToAnyWord && !IsLinkedToSelectedWord;

    public double SimilarityScore { get; set; } = 0;

}
