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
        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty);
        return stableHash;
    }
}
