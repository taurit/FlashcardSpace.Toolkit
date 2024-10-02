namespace CoreLibrary.Services;

public enum ImageGenerationProfile
{
    /// <summary>
    /// Best quality of generated images, at the cost of a performance, and wider array of image candidates.
    /// Uses more generation steps, refiner steps etc.
    /// </summary>
    PublicDeck,

    /// <summary>
    /// Profile optimized for generating flashcards for personal use. Faster, but less quality.
    /// Cuts corners on generation steps, refiner steps and number of generated image candidates to get a faster response.
    /// </summary>
    PrivateDeck,
}
