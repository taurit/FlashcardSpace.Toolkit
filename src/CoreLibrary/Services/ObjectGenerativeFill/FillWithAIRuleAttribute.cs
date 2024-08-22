namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Attribute used to decorate fields that should be filled by AI with specific rules.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class FillWithAIRuleAttribute(string ruleText) : Attribute
{
    public string RuleText { get; } = ruleText;
}
