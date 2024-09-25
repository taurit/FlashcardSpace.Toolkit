using Anki.NET.Models;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Anki.NET.Helpers;

internal static class GeneralHelper
{
    internal static string ConcatFields(FieldList fieldList, AnkiItem item, string separator)
    {
        var fieldsValues = fieldList.Select(t => item[t.Name]);
        return string.Join(separator, fieldsValues);
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
