using Spectre.Console.Cli;
using System.ComponentModel;

namespace GenerateFlashcards.Commands;

internal class GenerateFromTermListCommandSettings : CommandSettings
{
    [Description("Path to search. Defaults to current directory.")]
    [CommandArgument(0, "<inputFile>")] // <angleBrackets> mean required, [squareBrackets] mean optional
    public required string InputFilePath { get; init; }
}
