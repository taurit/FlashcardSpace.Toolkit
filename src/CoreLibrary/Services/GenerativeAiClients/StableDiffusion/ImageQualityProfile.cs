namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public record ImageQualityProfile(int Steps, bool IsRefinerEnabled, bool IsFaceRestorationEnabled)
{
    // 10 is a bit risky; image might significantly change between 10 and 25 steps (people disappear, composition significantly changes). I'll try 15.
    private const int DraftSteps = 15;
    private const int HighQualitySteps = 25;

    public static ImageQualityProfile DraftProfile => new ImageQualityProfile(DraftSteps, IsRefinerEnabled: false, IsFaceRestorationEnabled: false);
    public static ImageQualityProfile HighQualityProfile => new ImageQualityProfile(HighQualitySteps, IsRefinerEnabled: true, IsFaceRestorationEnabled: true);

    public static bool IsBelowHighQualityBar(int steps, string? idOfUsedRefiner, bool isFaceRestorationEnabled) =>
        steps < HighQualitySteps ||
        String.IsNullOrWhiteSpace(idOfUsedRefiner) ||
        !isFaceRestorationEnabled
        ;
}
