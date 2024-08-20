using AnkiNet.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AnkiNet.Helpers;

internal static class GeneralHelper
{
    internal static string ConcatFields(FieldList fieldList, AnkiItem item, string separator)
    {
        var fields = fieldList
            .Where(t => item[t.Name] as string != "")
            .Select(t => item[t.Name])
            .ToArray();

        return string.Join(separator, fields);
    }

    internal static string ReadResource(string path)
    {
        var a = Assembly.GetExecutingAssembly();
        var resourceStream = a.GetManifestResourceStream(path);

        return new StreamReader(resourceStream).ReadToEnd();
    }

    internal static string CheckSum(string sfld)
    {
        using var sha1 = SHA1.Create();

        //var length = sfld.Length >= 9 ? 8 : sfld.Length;
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(sfld));
        var sb = new StringBuilder(hash.Length);

        foreach (byte b in hash)
        {
            sb.Append(b);
        }

        return sb.ToString().Substring(0, 10);
    }
}
