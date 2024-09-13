using System.Diagnostics;

namespace GenerateFlashcards.Tests.Infrastructure;

static class ProcessRunner
{
    public static async Task<ProcessResult> Run(string executablePath, string arguments)
    {
        var processStartInfo = new ProcessStartInfo(executablePath)
        {
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();
        await process.WaitForExitAsync();

        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();

        return new ProcessResult(process.ExitCode, standardOutput, standardError);
    }
}
