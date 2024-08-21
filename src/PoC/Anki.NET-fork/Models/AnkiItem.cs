using System.Dynamic;

namespace Anki.NET.Models;

public class AnkiItem : DynamicObject
{
    private readonly Dictionary<string, object> _dictionary = new();

    public AnkiItem(FieldList fields, params string[] properties)
    {
        if (fields.Count != properties.Length) throw new ArgumentException($"{fields.Count} fields are required, but received {properties.Length} values!");
        for (var i = 0; i < properties.Length; ++i)
        {
            _dictionary[fields[i].Name] = properties[i].Replace("'", "’");
        }
    }

    public object this[string elem] => _dictionary[elem];

    public string Mid { get; set; } = "";

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        var name = binder.Name.ToLower();

        return _dictionary.TryGetValue(name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        _dictionary[binder.Name.ToLower()] = value;

        return true;
    }
}
