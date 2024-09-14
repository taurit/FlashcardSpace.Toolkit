using CoreLibrary.Services.ObjectGenerativeFill;
using GenerateFlashcards.Models.Spanish;
using Spectre.Console;

namespace GenerateFlashcards.Services;

/// <summary>
/// An experimental selector helping me select reasonable subset of words for an upcoming Anki deck
/// with the code name "Easy Words".
/// </summary>
internal class EasyWordsSpanishAdjectivesSelector(GenerativeFill generativeFill)
{
    internal async Task<List<TermInContext>> SelectConcreteAdjectives(List<TermInContext> adjectives)
    {
        var concreteAdjectivesCandidates = adjectives
            .Select(x => new SpanishAdjectiveConcreteness() { Adjective = x.TermBaseForm })
            .ToList();

        var concreteAdjectivesAnalyzed = await generativeFill
            .FillMissingProperties(Parameters.OpenAiModelId, Parameters.OpenAiModelClassId, concreteAdjectivesCandidates);

        DisplaySelectedAndRejectedAdjectivesList(concreteAdjectivesAnalyzed);

        var concreteAdjectives = concreteAdjectivesAnalyzed
            .Where(x => x.IsConcrete)
            .Select(x => x.Adjective)
            .ToHashSet();

        var adjectivesToKeep = adjectives
            .Where(x => concreteAdjectives.Contains(x.TermBaseForm))
            .ToList();

        return adjectivesToKeep;
    }

    private static void DisplaySelectedAndRejectedAdjectivesList(List<SpanishAdjectiveConcreteness> concreteAdjectivesAnalyzed)
    {
        AnsiConsole.MarkupLine("Selected and rejected adjectives:");

        var table = new Table();
        table.AddColumn("Adjective");
        table.AddColumn("Explanation");
        foreach (var res in concreteAdjectivesAnalyzed)
        {
            var prefix = res.IsConcrete ? "[green]" : "[red]";
            var suffix = "[/]";
            table.AddRow($"{prefix}{res.Adjective}{suffix}", res.Explanation);
        }

        AnsiConsole.Write(table);
    }
}
