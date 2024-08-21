namespace Anki.NET.Models;

public class Field
{
    private int _ord;
    public readonly string Name;
    private readonly string _font;
    private readonly int _size;

    public Field(string name, string font = "Arial", int size = 12)
    {
        Name = name;
        _font = font;
        _size = size;
    }

    public void SetOrdinalNumber(int ord)
    {
        _ord = ord;
    }

    public string ToJson()
    {
        return "{\"name\": \"" + Name + "\", \"rtl\": false, \"sticky\": false, \"media\": [], \"ord\": " + _ord + ", \"font\": \"" + _font + "\", \"size\": " + _size + "}";
    }

    public override string ToString()
    {
        return "{{" + Name + "}}";
    }
}
