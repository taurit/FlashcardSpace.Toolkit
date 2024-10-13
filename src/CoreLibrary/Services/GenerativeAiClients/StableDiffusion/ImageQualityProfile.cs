namespace CoreLibrary.Services.GenerativeAiClients.StableDiffusion;

public record ImageQualityProfile(int Steps, bool IsRefinerEnabled, bool IsFaceRestorationEnabled)
{
    private const int DraftSteps = 10;
    private const int HighQualitySteps = 25;

    public static ImageQualityProfile DraftProfile => new ImageQualityProfile(DraftSteps, IsRefinerEnabled: false, IsFaceRestorationEnabled: false);
    public static ImageQualityProfile HighQualityProfile => new ImageQualityProfile(HighQualitySteps, IsRefinerEnabled: true, IsFaceRestorationEnabled: true);

    public static bool IsBelowHighQualityBar(int steps, string? idOfUsedRefiner, bool isFaceRestorationEnabled) =>
        steps < HighQualitySteps ||
        String.IsNullOrWhiteSpace(idOfUsedRefiner) ||
        !isFaceRestorationEnabled
        ;
}
