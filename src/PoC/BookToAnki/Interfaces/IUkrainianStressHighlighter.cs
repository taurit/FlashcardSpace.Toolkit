namespace BookToAnki.Interfaces;

public interface IUkrainianStressHighlighter
{
    Task<string?> HighlightStresses(string inputText);
}
