namespace AnkiCardValidator.Utilities.JsonGenerativeFill;

/// <summary>
/// Attribute used to decorate fields that should be filled by AI.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FilledByAIAttribute : Attribute
{
}

/// <summary>
/// Represents a single item to process. User should inherit from this class.
/// </summary>
public abstract class ItemWithId
{
    /// <summary>
    /// Id to correlate item in input (prompt) with item in response (result from OpenAI Completions API).
    /// Id is managed by helper, not the caller.
    /// </summary>
    public int? Id { get; set; }
}

internal class ArrayOfItemsWithIds<T>(List<T> items) where T : ItemWithId
{
    public List<T> Items { get; set; } = items;
}
