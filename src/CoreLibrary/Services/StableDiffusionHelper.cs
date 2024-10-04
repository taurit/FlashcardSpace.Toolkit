using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Text.RegularExpressions;

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


    internal record StableDiffusionParameters(string Prompt, string NegativePrompt, string Steps, string Sampler, string ScheduleType, string CfgScale, string Seed, string FaceRestoration, string Size, string ModelHash, string Model, string Rng, string Refiner, string RefinerSwitchAt, string Version)
    {

        /// <summary>
        /// Example input string:
        ///
        /// ```
        /// She is dressed in blue.,dressed,drone shot,minimalist,desaturated
        /// Negative prompt: lowres,bad anatomy,bad hands,text,error,missing fingers,extra digit,fewer digits,cropped,worst quality,low quality,jpeg artifacts,signature,watermark,username,blurry,nsfw,painting,drawing,illustration,cartoon,anime,sketch,
        /// Steps: 24, Sampler: DPM++ 2M, Schedule type: Karras, CFG scale: 4.0, Seed: -353763303, Face restoration: CodeFormer, Size: 1216x832, Model hash: 31e35c80fc, Model: sd_xl_base_1.0, RNG: NV, Refiner: sd_xl_refiner_1.0 [7440042bbd], Refiner switch at: 0.7, Version: v1.10.1
        /// ```
        /// </summary>
        internal static StableDiffusionParameters? FromString(string userCommentTagValue)
        {
            if (String.IsNullOrWhiteSpace(userCommentTagValue)) return null;

            Regex regex = new Regex(@"(?<prompt>.+)Negative prompt: (?<negativePrompt>.+).*Steps: (?<steps>\d+), Sampler: (?<sampler>.+), Schedule type: (?<scheduleType>.+), CFG scale: (?<cfgScale>.+), Seed: (?<seed>.+), Face restoration: (?<faceRestoration>.+), Size: (?<size>.+), Model hash: (?<modelHash>.+), Model: (?<model>.+), RNG: (?<rng>.+), Refiner: (?<refiner>.+), Refiner switch at: (?<refinerSwitchAt>.+), Version: (?<version>.+)", RegexOptions.Compiled | RegexOptions.Singleline);
            var match = regex.Match(userCommentTagValue);
            if (match.Success)
            {
                return new StableDiffusionParameters(
                    match.Groups["prompt"].Value.Trim(),
                    match.Groups["negativePrompt"].Value.Trim(),
                    match.Groups["steps"].Value,
                    match.Groups["sampler"].Value,
                    match.Groups["scheduleType"].Value,
                    match.Groups["cfgScale"].Value,
                    match.Groups["seed"].Value,
                    match.Groups["faceRestoration"].Value,
                    match.Groups["size"].Value,
                    match.Groups["modelHash"].Value,
                    match.Groups["model"].Value,
                    match.Groups["rng"].Value,
                    match.Groups["refiner"].Value,
                    match.Groups["refinerSwitchAt"].Value,
                    match.Groups["version"].Value
                );
            }
            return null;
        }
    }

    public static StableDiffusionParameters? GetStableDiffusionParametersFromImage(string filePath)
    {
        // Read all metadata from the image
        var directories = ImageMetadataReader.ReadMetadata(filePath);

        // Find the ExifSubIFDDirectory which contains the UserComment tag
        var exifDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

        if (exifDirectory != null)
        {
            var userComment = exifDirectory.Tags.FirstOrDefault(x => x.Name == "User Comment")?.Description;
            var result = (userComment is not null) ? StableDiffusionParameters.FromString(userComment) : null;
            return result;
        }

        return null;
    }
}
