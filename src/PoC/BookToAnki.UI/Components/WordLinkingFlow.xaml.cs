using BookToAnki.Models;
using BookToAnki.Services;
using PropertyChanged;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace BookToAnki.UI.Components;

/// <summary>
/// Interaction logic for WordLinkingFlow.xaml
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class WordLinkingFlow : Window
{
    readonly IReadOnlyList<WordSimilarity> _candidates;
    private readonly WordsLinker _wordsLinker;

    public int CurrentCandidateIndex { get; set; } = -1;

    [DependsOn(nameof(CurrentCandidateIndex))]
    public WordSimilarity CurrentCandidate => _candidates[CurrentCandidateIndex];

    [DependsOn(nameof(CurrentCandidateIndex))]
    public double ProgressPercent => 100 * CurrentCandidateIndex / (double)(_candidates.Count);

    private readonly LinkingExceptionsStore _wordsLinkingExceptions = new(Settings.LinkingExceptionsStore);

    public WordLinkingFlow(IReadOnlyList<WordSimilarity> similaritiesByScore, WordsLinker wordsLinker)
    {
        _candidates = similaritiesByScore;
        _wordsLinker = wordsLinker;

        DataContext = this;
        ShowNextCandidate();

        InitializeComponent();
    }

    private void ShowNextCandidate()
    {
        do
        {
            CurrentCandidateIndex++;
            var knownException = _wordsLinkingExceptions.IsInExceptionList(CurrentCandidate.Word1, CurrentCandidate.Word2);

            var word1Group = _wordsLinker.GetAllLinkedWords(CurrentCandidate.Word1).ToList();
            if (!word1Group.Any()) word1Group.Add(CurrentCandidate.Word1);

            var word2Group = _wordsLinker.GetAllLinkedWords(CurrentCandidate.Word2).ToList();
            if (!word2Group.Any()) word2Group.Add(CurrentCandidate.Word2);

            foreach (var w1 in word1Group)
            {
                foreach (var w2 in word2Group)
                {
                    if (_wordsLinkingExceptions.IsInExceptionList(w1, w2))
                    {
                        knownException = true;
                    }
                }
            }

            // word could have been linked after flow started, in previous cards. This is to avoid repeating asking for words in groups that have already been linked
            var wordsAlreadyLinked = _wordsLinker.AreWordsLinked(CurrentCandidate.Word1, CurrentCandidate.Word2);

            if (!(knownException || wordsAlreadyLinked))
                break;


        } while (CurrentCandidateIndex < _candidates.Count);
    }


    private void NoNotSimilar_OnClick(object sender, RoutedEventArgs e)
    {
        _wordsLinkingExceptions.AddException(CurrentCandidate.Word1, CurrentCandidate.Word2);
        ShowNextCandidate();

        Debug.WriteLine($"Added to linking exceptions: {CurrentCandidate.Word1} and {CurrentCandidate.Word2}");
    }

    private void YesSimilar_OnClick(object sender, RoutedEventArgs e)
    {
        _wordsLinker.LinkWords(CurrentCandidate.Word1, CurrentCandidate.Word2);
        ShowNextCandidate();

        Debug.WriteLine($"Linked: {CurrentCandidate.Word1} and {CurrentCandidate.Word2}");
    }
}
