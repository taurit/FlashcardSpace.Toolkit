using GenerateFlashcards.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated by the Spectre.Console.Cli framework")]
internal sealed class GenerateFlashcardsCommandSettings : CommandSettings
{
    [Description("Path to search. Defaults to current directory.")]
    [CommandArgument(0, "<inputFile>")] // <angleBrackets> mean required, [squareBrackets] mean optional
    public required string InputFilePath { get; init; }

    [CommandOption("--inputLanguage")]
    [DefaultValue(SupportedInputLanguage.Autodetect)]
    public SupportedInputLanguage InputLanguage { get; init; }

    [CommandOption("--outputLanguage")]
    [DefaultValue(SupportedOutputLanguage.Unknown)]
    public required SupportedOutputLanguage OutputLanguage { get; init; }

    public override ValidationResult Validate()
    {
        // ensure the `outputLanguage` option is set
        // currently, custom validation is the only way to make option required : https://github.com/spectreconsole/spectre.console/discussions/538
        var outputLanguageIsNotSet = OutputLanguage == SupportedOutputLanguage.Unknown;
        if (outputLanguageIsNotSet)
            return ValidationResult.Error("The `--outputLanguage` must be set.");

        // make sure the input and output languages are different
        var inputLanguageName = InputLanguage.ToString();
        var outputLanguageName = OutputLanguage.ToString();
        var inputAndOutputLanguageIsTheSame = inputLanguageName == outputLanguageName;

        if (inputAndOutputLanguageIsTheSame)
            ValidationResult.Error("The `--outputLanguage` must be different from the `--inputLanguage`.");

        return ValidationResult.Success();
    }
}
