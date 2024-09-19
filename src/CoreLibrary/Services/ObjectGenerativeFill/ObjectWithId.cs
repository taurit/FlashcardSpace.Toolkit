namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Represents a single item to process. User should inherit from this class.
/// </summary>
public abstract class ObjectWithId
{
    /// <summary>
    /// Id to correlate item in input (prompt) with item in response (result from OpenAI Completions API).
    /// Id is managed by helper, not the caller.
    /// </summary>
    [FillWithAIRule($"{nameof(Id)} links elements in input (prompt) and output.")]
    [FillWithAIRule($"This value must always be provided.")]
    public int? Id { get; set; }
}
