using CoreLibrary.Services;

namespace AnkiCardValidator.Utilities;

public class DuplicateDetectionEqualityComparer(StringSanitizer stringSanitizer) : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        var xNormalized = stringSanitizer.GetNormalizedFormOfLearnedTermWithCache(x);
        var yNormalized = stringSanitizer.GetNormalizedFormOfLearnedTermWithCache(y);

        // normalization goes a bit too far and considers "el artista" and "la artista" duplicates, which it's not
        var seemsLikeADuplicate = (xNormalized == yNormalized);

        if (seemsLikeADuplicate)
        {
            var xStartsWithEl = x.StartsWith("el ", StringComparison.InvariantCultureIgnoreCase);
            var xStartsWithLa = x.StartsWith("la ", StringComparison.InvariantCultureIgnoreCase);

            var yStartsWithEl = y.StartsWith("el ", StringComparison.InvariantCultureIgnoreCase);
            var yStartsWithLa = y.StartsWith("la ", StringComparison.InvariantCultureIgnoreCase);

            var articlesDiffer = (xStartsWithEl && yStartsWithLa) || (yStartsWithEl && xStartsWithLa);
            seemsLikeADuplicate = !articlesDiffer;
        }

        return seemsLikeADuplicate;
    }

    public int GetHashCode(string obj)
    {
        return stringSanitizer.GetNormalizedFormOfLearnedTermWithCache(obj).GetHashCode();
    }
}
