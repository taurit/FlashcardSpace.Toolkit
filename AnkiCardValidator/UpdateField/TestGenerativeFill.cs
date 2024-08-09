using AnkiCardValidator.Utilities.JsonGenerativeFill;

namespace UpdateField;

class CountriesModel(string country) : ObjectWithId
{
    public string Country { get; init; } = country;

    [FillWithAI]
    [FillWithAIRule("Fill with the first historically known capital of the country, not the current one.")]
    public string? Capital { get; set; }

    [FillWithAI]
    [FillWithAIRule("Fill the value with name of the current president of the country")]
    [FillWithAIRule("Fill the value in CAPITAL LETTERS")]
    [FillWithAIRule("After the name, add a year when the person first became president (format: `BARRACK OBAMA (2009)`)")]
    public string? President { get; set; }
}

internal class TestGenerativeFill
{
    public async Task DoTestGenerativeFill()
    {
        List<CountriesModel> inputs =
        [
            new CountriesModel("Poland"),
            new CountriesModel("Ukraine"),
            new CountriesModel("Germany")
        ];

        var response = await JsonGenerativeFill.FillMissingProperties(inputs);

        foreach (var job in response)
        {
            Console.WriteLine($"{job.Country} capital is {job.Capital}, president is {job.President}");
        }
    }
}
