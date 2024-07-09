namespace AnkiCardValidator.Utilities;
internal class DefinitionCounter(NormalFormProvider normalFormProvider)
{
    public int CountDefinitions(string word)
    {
        return word.Split(",").Length;
    }
}
