using CoreLibrary.Services;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GenerateFlashcards.Commands;

/// <summary>
/// A temporary command just for convenience of testing some pieces code with "F5" (Run) in Visual Studio.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class DebugCommand(ImageGenerator imageGenerator) : AsyncCommand<DebugCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, DebugCommandSettings settings)
    {
        var image = await imageGenerator.GenerateImage("test");
        var htmlFragmentDisplayingBase64Image = $"<img src=\"data:image/png;base64,{image.Base64EncodedImage}\" />";
        await File.WriteAllTextAsync("d:/testAAA.html", htmlFragmentDisplayingBase64Image);

        // Launch html
        var processStartInfo = new ProcessStartInfo("d:/testAAA.html")
        {
            UseShellExecute = true
        };
        Process.Start(processStartInfo);

        return 0;
    }
}
