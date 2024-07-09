namespace AnkiCardValidator.Utilities;

public class DefinitionCounter(NormalFormProvider normalFormProvider)
{
    public int CountDefinitions(string word)
    {
        return word.Split(",").Length;
    }
}
