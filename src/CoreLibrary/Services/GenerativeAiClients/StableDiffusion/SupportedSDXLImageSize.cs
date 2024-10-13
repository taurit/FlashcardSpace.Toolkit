namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public record SupportedSDXLImageSize(int Width, int Height)
{
    public static SupportedSDXLImageSize Square =>
        new SupportedSDXLImageSize(1024, 1024);

    public static SupportedSDXLImageSize Wide =>
        new SupportedSDXLImageSize(1216, 832);

    public static SupportedSDXLImageSize FromWidthAndHeight(int width, int height)
    {
        // if unsupported size, throw exception
        if (width == 1024 && height == 1024)
            return Square;
        if (width == 1216 && height == 832)
            return Wide;

        throw new ArgumentException($"Unsupported image size: {width}x{height}");
    }
}
