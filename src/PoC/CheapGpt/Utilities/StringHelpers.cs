using System.Security.Cryptography;
using System.Text;

namespace CheapGpt;
internal static class StringHelpers
{
    /// <summary>
    /// string.GetHashCode() in modern .NET is not stable, i.e. it gives different results after the application is restarted.
    /// I need a stable hash to help me create a cache key for long content like ChatGPT prompts and use as filename, so this helper method helps achieve that.
    /// </summary>
    internal static string GetHashCodeStable(this string input)
    {
        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var stableHash = BitConverter.ToString(stableHashBytes).Replace("-", string.Empty);
        return stableHash;
    }
}
