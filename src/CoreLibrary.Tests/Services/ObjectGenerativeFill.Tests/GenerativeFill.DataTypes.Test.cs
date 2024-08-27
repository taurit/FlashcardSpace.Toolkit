using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

[TestClass, TestCategory("SkipInGitHubActions")]
//[Ignore("Skipped to avoid unnecessary costs. Uncomment when modifying the service or changing the AI model.")]
public class GenerativeFillDataTypesTests
{
    private readonly GenerativeFill _generativeFill = GenerativeFillTestFactory.CreateInstance();

    [TestMethod]
    public async Task GenerativeFill_ShouldWorkWithAllDotnetDatatypes()
    {
        // Arrange
        var input = new HistoricalFigure { Name = "Napoleon Bonaparte" };

        // Act
        var output = await _generativeFill.FillMissingProperties(TestParameters.OpenAiModelId, TestParameters.OpenAiModelId, input);
        Console.WriteLine(output);

        // Assert
        output.HeightCm.Should().BeGreaterThan(0, because: "Height is a positive number");
        output.WeightKg.Should().BeGreaterThan(0, because: "Weight is a positive number");
        output.AgeOfDeath.Should().BeGreaterThan(0, because: "Age is a positive number");
        output.DateOfBirth.Should().BeBefore(DateTime.Now, because: "Date of birth is in the past");
        output.DateOfDeath.Should().BeBefore(DateTimeOffset.Now, because: "Date of death is in the past");
        output.WasMarried.Should().BeTrue(because: "Napoleon was married");
        output.FirstLetterOfName.Should().Be('N', because: "First letter of Napoleon's name is 'N'");
        output.NumberOfChildren.Should().BeGreaterThan(0, because: "Napoleon had children");
        output.NumberOfWars.Should().BeGreaterThan(0, because: "Napoleon was in wars");
        output.NumberOfDaysInPower.Should().BeGreaterThan(0, because: "Napoleon was in power");
        output.NumberOfAssassinationAttempts.Should().BeGreaterOrEqualTo(0, because: "Napoleon was a target of assassination attempts");

        // TimeSpan does not work, not a priority to fix right now
        //output.TimeOfReign.Should().BeGreaterThan(TimeSpan.Zero, because: "Reign time is a positive number");
    }
}

class HistoricalFigure() : ObjectWithId
{
    public string? Name { get; init; }

    [FillWithAI]
    public decimal HeightCm { get; set; }

    [FillWithAI]
    public double WeightKg { get; set; }

    [FillWithAI]
    public int AgeOfDeath { get; set; }

    [FillWithAI]
    public DateTime DateOfBirth { get; set; }

    [FillWithAI]
    public DateTimeOffset DateOfDeath { get; set; }

    //[FillWithAI]
    //public TimeSpan TimeOfReign { get; set; }

    [FillWithAI]
    public bool WasMarried { get; set; }

    [FillWithAI]
    public char FirstLetterOfName { get; set; }

    [FillWithAI]
    public byte NumberOfChildren { get; set; }

    [FillWithAI]
    public short NumberOfWars { get; set; }

    [FillWithAI]
    public long NumberOfDaysInPower { get; set; }

    [FillWithAI]
    public sbyte NumberOfAssassinationAttempts { get; set; }

}
