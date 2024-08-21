using System.Diagnostics;

namespace CoreLibrary.Interfaces;

[DebuggerDisplay("{Text}")]
public class Sentence
{
    public Sentence(string text, List<string> words, Sentence? previousSentence = null)
    {
        Text = text;
        Words = words;
        PreviousSentence = previousSentence;
        if (previousSentence is not null)
            previousSentence.NextSentence = this;
    }

    public string Text { get; }
    public List<string> Words { get; }
    public Sentence? PreviousSentence { get; }
    public Sentence? NextSentence { get; internal set; }

    public bool HasAnyWords => Words.Count != 0;
}
