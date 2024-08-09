using AnkiCardValidator.Utilities.JsonGenerativeFill;

namespace UpdateField;

class CountriesModel(string country) : ItemWithId
{
    public string Country { get; init; } = country;

    [FilledByAI] public string? Capital { get; set; }

    [FilledByAI] public string? President { get; set; }
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

        var response = await JsonGenerativeFill.FillMissingProperties(inputs, hints: "Provide the most recent data available.");

        foreach (var job in response)
        {
            Console.WriteLine($"{job.Country} capital is {job.Capital}, president is {job.President}");
        }
    }
}
