using CoreLibrary.Services;
using FluentAssertions;

namespace CoreLibrary.Tests.Services;

[TestClass]
public class StableDiffusionHelperTests
{

    [TestMethod]
    public async Task RestoringPromptFromOutputJpegSucceeds()
    {
        // Arrange
        var testImageFilePath = Path.Combine("Resources", "stable-diffusion-image-with-metadata.jpg");

        var imageWidth = 1216;
        var imageHeight = 832;

        // Act
        var sdParams = StableDiffusionHelper.GetStableDiffusionParametersFromImage(testImageFilePath);

        // Assert
        sdParams.Should().NotBeNull();
        sdParams!.Prompt.Should().Be("She is dressed in blue.,dressed,drone shot,minimalist,desaturated");
        sdParams.NegativePrompt.Should().Be("lowres,bad anatomy,bad hands,text,error,missing fingers,extra digit,fewer digits,cropped,worst quality,low quality,jpeg artifacts,signature,watermark,username,blurry,nsfw,painting,drawing,illustration,cartoon,anime,sketch,");
        sdParams.Steps.Should().Be(24);
        sdParams.Sampler.Should().Be("DPM++ 2M");
        sdParams.ScheduleType.Should().Be("Karras");
        sdParams.CfgScale.Should().Be("4.0");
        sdParams.Seed.Should().Be("-353763303");
        sdParams.FaceRestoration.Should().Be("CodeFormer");
        sdParams.Width.Should().Be(1216);
        sdParams.Height.Should().Be(832);
        sdParams.ModelHash.Should().Be("31e35c80fc");
        sdParams.Model.Should().Be("sd_xl_base_1.0");
        sdParams.Rng.Should().Be("NV");
        sdParams.Refiner.Should().Be("sd_xl_refiner_1.0 [7440042bbd]");
        sdParams.RefinerSwitchAt.Should().Be("0.7");
        sdParams.Version.Should().Be("v1.10.1");
    }
}
