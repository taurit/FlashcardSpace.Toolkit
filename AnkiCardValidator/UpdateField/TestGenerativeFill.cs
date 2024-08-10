using AnkiCardValidator.Utilities.JsonGenerativeFill;

namespace UpdateField;

class Country(string countryName) : ObjectWithId
{
    public string CountryName { get; init; } = countryName;

    [FillWithAI]
    [FillWithAIRule("Fill with the first historically known capital of the country, not the current one!")]
    public string? FirstCapital { get; set; } = null;

    [FillWithAI]
    [FillWithAIRule("Fill the value with name of the current president of the country")]
    [FillWithAIRule("Use CAPITAL LETTERS for this value")]
    public string? President { get; set; } = null;
}

internal class TestGenerativeFill
{
    public async Task DoTestGenerativeFill()
    {
        //var partiallyCompleteCountry = new Country("Poland");
        //Country country = await GenerativeFill.FillMissingProperties(partiallyCompleteCountry);
        //return;

        var countries = await GenerativeFill.FillMissingProperties(new Country[] { new("USA"), new("Poland"), new("France") });




        foreach (var job in countries)
        {
            Console.WriteLine($"{job.CountryName}: first capital was {job.FirstCapital}, the current president is {job.President}.");
        }
    }
}
