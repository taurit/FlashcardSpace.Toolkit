namespace CoreLibrary.Services;

public record SupportedSDXLImageSize(int Width, int Height)
{
    public static SupportedSDXLImageSize Square =>
        new SupportedSDXLImageSize(1024, 1024);

    public static SupportedSDXLImageSize Wide =>
        new SupportedSDXLImageSize(1216, 832);

}
