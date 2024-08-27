using System.Security.Cryptography;
using System.Text;

namespace CoreLibrary.Utilities;
public static partial class StringExtensionMethodsHashing
{
    /// <summary>
    /// string.GetHashCode() in modern .NET is not stable, i.e. it gives different results after the application is restarted.
    /// I need a stable hash to help me create a cache key for long content like ChatGPT prompts and use as filename, so this helper method helps achieve that.
    /// </summary>
    public static string GetHashCodeStable(this string input)
    {
        const int lengthOfHash = 20;
        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty).Substring(0, lengthOfHash);

        // arbitrary choice, but I'll shorten the hash so filenames are not that long.
        // Assuming cache might reach 1 000 000 unique prompt responses cached
        // and limiting hash length to 20 characters (`lengthOfHash`)
        // we still have estimated probability of collision of 2.5×10^(−17)

        return stableHash;
    }
}
