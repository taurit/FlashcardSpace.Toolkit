using CoreLibrary.Models;
using FluentAssertions;
using System.Reflection;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;

namespace CoreLibrary.Tests;

/// <summary>
/// The model of the 'Deck' is represented in few places:
/// - as a C# class (<see cref="Deck"/>)
/// - as a JSON schema (\src\DeckBrowser\deck.schema.json)
/// - as TypeScript interface (\src\DeckBrowser\src\models\FlashcardDeck.tsx)
///
/// These tests help detect inconsistencies between these representations.
/// </summary>
[TestClass]
public class DeckSchemaIntegrityTests
{
    private const string PathToJsonSchemaOfDeckInBrowserComponent = @"../../../../../DeckBrowser/deck.schema.json";

    private const string PathToTypescriptFlashcardDeck = @"../../../../../DeckBrowser/src/models/FlashcardDeck.tsx";
    private const string PathToTypescriptFlashcardNote = @"../../../../../DeckBrowser/src/models/FlashcardNote.tsx";
    private const string PathToTypeScriptFlashcardNoteEditablePart = @"../../../../../DeckBrowser/src/models/FlashcardNoteEditablePart.tsx";

    [TestMethod]
    public async Task DeckJsonSchemaMatchesDeckClass()
    {
        // Arrange
        var expectedDeckSchema = DeckSerializationOptions.SerializationOptions.GetJsonSchemaAsNode(typeof(Deck)).ToString();

        // Act
        var actualDeckSchemaInDeckBrowserFolder = await File.ReadAllTextAsync(PathToJsonSchemaOfDeckInBrowserComponent);

        // Assert
        expectedDeckSchema.Should().Be(actualDeckSchemaInDeckBrowserFolder);

        // if not, update the schema file with the following code:
        //await UpdateSchema(expectedDeckSchema);
    }


    [TestMethod]
    public async Task TypeScriptModel_OfFlashcardNoteEditablePart_ContainsAllPropertiesOfCsharpModel()
    {
        // Arrange
        var actualFlashcardModel = await File.ReadAllTextAsync(PathToTypeScriptFlashcardNoteEditablePart);

        // Assert that the TypeScript model contains all properties of the C# model
        foreach (var property in typeof(FlashcardNoteEditablePart).GetProperties())
        {
            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
            actualFlashcardModel.Should().Contain($" {propertyName}:");
        }
    }


    [TestMethod]
    public async Task TypeScriptModel_OfFlashcardNote_ContainsAllPropertiesOfCsharpModel()
    {
        // Arrange
        var actualFlashcardModel = await File.ReadAllTextAsync(PathToTypescriptFlashcardNote);

        // Assert that the TypeScript model contains all properties of the C# model
        var numTestedProps = 0;

        foreach (var property in typeof(FlashcardNote).GetProperties())
        {
            bool isInherited = property.DeclaringType != typeof(FlashcardNote);
            if (!isInherited)
            {
                var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                actualFlashcardModel.Should().Contain($" {propertyName}:");
                numTestedProps++;
            }
        }

        if (numTestedProps == 0)
            throw new Exception("No properties were tested. Check if the C# model has any properties.");
        else
            Console.WriteLine($"Tested {numTestedProps} properties.");
    }

    [TestMethod]
    public async Task TypeScriptModel_OfFlashcardDeck_ContainsAllPropertiesOfCsharpModel()
    {
        // Arrange
        var actualFlashcardModel = await File.ReadAllTextAsync(PathToTypescriptFlashcardDeck);

        // Assert that the TypeScript model contains all properties of the C# model
        foreach (var property in typeof(Deck).GetProperties())
        {
            var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
            actualFlashcardModel.Should().Contain($" {propertyName}:");
        }
    }


    // updates JSON schema file for `Deck` object, for accurate IntelliSense when editing/previewing Deck JSON files
    private static async Task UpdateSchema(string expectedDeckSchema)
    {
        await File.WriteAllTextAsync(PathToJsonSchemaOfDeckInBrowserComponent, expectedDeckSchema);
    }

}
