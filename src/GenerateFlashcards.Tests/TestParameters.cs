namespace GenerateFlashcards.Tests;
internal static class TestParameters
{
    // "gpt-4o-mini" fails some Generative Fill tests:
    // - it has troubles enumerating all grammatical roles a specific word can have. I didn't find a prompt that would help.
    //
    // I can retest with newer versions.
    //public const string OpenAiModelId = "gpt-4o-2024-08-06";
    public const string OpenAiModelId = "gpt-4o-mini";
}
