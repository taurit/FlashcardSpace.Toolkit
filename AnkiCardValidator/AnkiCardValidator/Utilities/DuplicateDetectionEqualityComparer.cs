namespace AnkiCardValidator.Utilities;

public class DuplicateDetectionEqualityComparer(NormalFormProvider normalFormProvider) : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        var xNormalized = normalFormProvider.GetNormalizedFormOfLearnedTermWithCache(x);
        var yNormalized = normalFormProvider.GetNormalizedFormOfLearnedTermWithCache(y);
        return xNormalized == yNormalized;
    }

    public int GetHashCode(string obj)
    {
        return normalFormProvider.GetNormalizedFormOfLearnedTermWithCache(obj).GetHashCode();
    }
}
