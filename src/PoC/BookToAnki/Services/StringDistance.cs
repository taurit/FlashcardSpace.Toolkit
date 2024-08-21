using Fastenshtein;

namespace BookToAnki.Services;
public static class StringDistance
{
    public static bool AreStringsVerySimilar(string string1, string string2)
    {
        int distance = Levenshtein.Distance(string1, string2); ;
        if (string1.Length > 3)
            return distance <= 3;
        if (string1.Length > 2)
            return distance <= 2;
        return false;
    }

}
