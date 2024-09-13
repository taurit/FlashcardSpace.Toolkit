namespace GenerateFlashcards.Tests.Infrastructure;

record ProcessResult(int StatusCode, string StandardOutput, string StandardError)
{
    /// <summary>
    /// Exception messages might be written to either result.StandardOutput or result.StandardError, so it's convenient
    /// to have them available in one property for automated testing purposes.
    /// 
    /// This is because Spectre.Console.CLI has the `config.PropagateExceptions()` flag I like to have enabled for debugging
    /// but not necessary for releases. When it's enabled, error message lands in standard error stream instead of standard output.
    /// </summary>
    public string ProcessOutput => $"Standard output:\n" +
                                   $"{StandardOutput}\n" +
                                   $"\n" +
                                   $"Standard error:\n" +
                                   $"{StandardError}";
}
