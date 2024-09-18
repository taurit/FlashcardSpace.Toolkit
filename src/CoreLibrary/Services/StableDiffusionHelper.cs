using System.Diagnostics;

namespace CoreLibrary.Services;

// A small util (stateless service) to interact with the local Stable Diffusion API's functions
// other than txt2img image generation (which has a dedicated service).
internal class StableDiffusionHelper
{
    /// <summary>
    /// Tests if local Stable Diffusion API is running.
    ///
    /// For image generation, this service must be configured and running locally:
    /// https://github.com/AUTOMATIC1111/stable-diffusion-webui/
    /// </summary>
    public static async Task<bool> IsAlive()
    {
        var testUrl = "http://127.0.0.1:7860/favicon.ico";
        var timeout = TimeSpan.FromSeconds(1);
        var testHttpClient = new HttpClient { Timeout = timeout };
        try
        {
            var response = await testHttpClient.GetAsync(testUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task TryStartStableDiffusionApi(string stableDiffusionApiRunBatPath)
    {
        // argument is sth like `d:\ProgramData\Tools\sd.webui\run.bat`
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start {stableDiffusionApiRunBatPath}",
                WorkingDirectory = Path.GetDirectoryName(stableDiffusionApiRunBatPath),
                UseShellExecute = true
            }
        };
        process.Start();
        await Task.Delay(5000);

    }
}
