using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace GenerateFlashcards.Commands;
internal class GenerateFromNaturalLanguageSettings : CommandSettings
{
    [Description("Path to search. Defaults to current directory.")]
    [CommandArgument(0, "<inputFile>")] // <angleBrackets> mean required, [squareBrackets] mean optional
    public required string InputFilePath { get; init; }

    [Description("Name of the language of the content in the input file (typically, a foreign language we learn).")]
    [CommandOption("--inputLanguage")]
    [DefaultValue(SupportedLanguage.Unspecified)]
    public SupportedLanguage Language { get; init; }

    [Description("Name of the language to use for translation and explanations (typically, our native language).")]
    [CommandOption("--outputLanguage")]
    [DefaultValue(SupportedLanguage.Unspecified)]
    public required SupportedLanguage OutputLanguage { get; init; }

    public override ValidationResult Validate()
    {
        // ensure the `outputLanguage` option is set
        // currently, custom validation is the only way to make option required : https://github.com/spectreconsole/spectre.console/discussions/538
        var outputLanguageIsNotSet = OutputLanguage == SupportedLanguage.Unspecified;
        if (outputLanguageIsNotSet)
            return ValidationResult.Error("The `--outputLanguage` must be set.");

        // make sure the input and output languages are different
        var inputLanguageName = Language.ToString();
        var outputLanguageName = OutputLanguage.ToString();
        var inputAndOutputLanguageIsTheSame = inputLanguageName == outputLanguageName;

        if (inputAndOutputLanguageIsTheSame)
            return ValidationResult.Error("The `--outputLanguage` must be different from the `--inputLanguage`.");

        if (!File.Exists(InputFilePath))
            return ValidationResult.Error($"The input file `{InputFilePath}` cannot be found.");

        if (Language == SupportedLanguage.Unspecified)
            return ValidationResult.Error("The `--inputLanguage` must be set explicitly (the auto-detection is not implemented).");

        return ValidationResult.Success();
    }
}
