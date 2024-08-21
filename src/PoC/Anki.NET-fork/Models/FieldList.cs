namespace Anki.NET.Models;

public class FieldList : List<Field>
{
    public new void Add(Field field)
    {
        field.SetOrdinalNumber(Count);
        base.Add(field);
    }

    public string ToJson()
    {
        var json = from field in FindAll(x => x != null)
                   select field.ToJson();

        return string.Join(",\n", json.ToArray());
    }

    public override string ToString()
    {
        return string.Join("\\n<br>\\n", (object[])ToArray());
    }

}
