﻿using System.Security.Cryptography;
using System.Text;

namespace CoreLibrary.Utilities;
public static class StringExtensionMethodsHashing
{
    /// <summary>
    /// string.GetHashCode() in modern .NET is not stable, i.e. it gives different results after the application is restarted.
    /// I need a stable hash to help me create a cache key for long content like ChatGPT prompts and use as filename, so this helper method helps achieve that.
    /// </summary>
    public static string GetHashCodeStable(this string input, int reduceLengthToCharacters = 8)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return bytes.GetHashCodeStable(reduceLengthToCharacters);
    }

    public static string GetHashCodeStable(this byte[] input, int reduceLengthToCharacters = 8)
    {
        // Probabilities of collision of 1000000 hashes assuming alphabet of 64 characters, for different hash lengths:
        // source: https://hash-collisions.progs.dev/
        // 5 characters: 0.36539143049797307
        // 6 characters: 0.001774778278169853
        // 7 characters: 0.000006938862891048281
        // 8 characters: 0.000000027105026889628903
        // 9 characters: 0.0000000001048576

        // 8 is a reasonable default giving as short hash as possible but still very low probability of collision

        var sha256Bytes = SHA256.HashData(input);
        var sha256Base64 = Convert.ToBase64String(sha256Bytes);
        var stringBuilder = new StringBuilder(reduceLengthToCharacters);

        for (int i = 0; i < sha256Base64.Length && stringBuilder.Length < reduceLengthToCharacters; i++)
        {
            char c = sha256Base64[i];
            if (c != '+' && c != '=' && c != '/')
            {
                stringBuilder.Append(c);
            }
        }

        var stableHash = stringBuilder.ToString();
        return stableHash;
    }

    public static int GetHashCodeStableInt(this string input)
    {
        var stableHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var stableHash = BitConverter.ToInt32(stableHashBytes, 0);
        return stableHash;
    }
}
