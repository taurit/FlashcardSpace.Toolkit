using MemoryPack;

namespace BookToAnki.Models;

[MemoryPackable]
public partial record BilingualSentence
{
    public BilingualSentence(string PrimaryLanguage, string SecondaryLanguage)
    {
        this.PrimaryLanguage = PrimaryLanguage ?? throw new ArgumentNullException(nameof(PrimaryLanguage));
        this.SecondaryLanguage = SecondaryLanguage ?? throw new ArgumentNullException(nameof(SecondaryLanguage));

        if (String.IsNullOrWhiteSpace(PrimaryLanguage))
            throw new ArgumentException("Value cannot be null or empty", nameof(PrimaryLanguage));
        if (String.IsNullOrWhiteSpace(SecondaryLanguage))
            throw new ArgumentException("Value cannot be null or empty", nameof(SecondaryLanguage));
    }

    public string PrimaryLanguage { get; init; }
    public string SecondaryLanguage { get; init; }
}

