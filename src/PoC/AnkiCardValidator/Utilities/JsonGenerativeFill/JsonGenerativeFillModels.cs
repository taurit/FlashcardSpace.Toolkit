namespace AnkiCardValidator.Utilities.JsonGenerativeFill;

/// <summary>
/// Attribute used to decorate fields that should be filled by AI.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class FillWithAIAttribute : Attribute
{
}

/// <summary>
/// Attribute used to decorate fields that should be filled by AI with specific rules.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class FillWithAIRuleAttribute(string ruleText) : Attribute
{
    public string RuleText { get; } = ruleText;
}

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

internal class ArrayOfItemsWithIds<T>(List<T> items) where T : ObjectWithId
{
    public List<T> Items { get; set; } = items;
}
