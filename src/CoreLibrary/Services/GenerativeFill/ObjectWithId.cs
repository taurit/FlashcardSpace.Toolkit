namespace CoreLibrary.Services.GenerativeFill;

/// <summary>
/// Represents a single item to process. User should inherit from this class.
/// </summary>
public abstract class ObjectWithId
{
    /// <summary>
    /// Id to correlate item in input (prompt) with item in response (result from OpenAI Completions API).
    /// Id is managed by helper, not the caller.
    /// </summary>
    public int? Id { get; set; }
}
