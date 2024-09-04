namespace CoreLibrary.Interfaces;

public interface IProvideTranslations
{
    Task<List<Note>> AddTranslations(List<TermInContext> terms);
}
