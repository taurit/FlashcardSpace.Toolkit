using System.Reflection;
using System.Text;

namespace CoreLibrary.Services.ObjectGenerativeFill;

public static class ClassFingerprintProvider
{
    // in-memory cache of Type Fingerprints
    private static readonly Dictionary<Type, string> _typeFingerprints = new();

    /// <summary>
    /// This method generates unique string fingerprint for the type of single item.
    /// The fingerprint should change whenever:
    /// - any of the property names changes
    /// - properties order change
    /// - presence of [FillWithAI] attribute on any property changed
    /// - presence of [FillWithAIRule] attribute on any property changed
    /// - the content of [FillWithAIRule] attribute changed
    /// </summary>
    /// <param name="typeOfSingleItem">A type to generate the fingerprint for</param>
    /// <param name="recursionDepth">Indentation level for the output</param>
    /// <returns>An arbitrary string that wil remain the same if type didn't change, but differs if it changed.</returns>
    internal static string GenerateTypeFingerprint(Type typeOfSingleItem, int recursionDepth = 0)
    {
        if (recursionDepth == 0 && _typeFingerprints.TryGetValue(typeOfSingleItem, out var cachedFingerprint))
            return cachedFingerprint;

        if (recursionDepth > 4)
            throw new ArgumentOutOfRangeException(nameof(recursionDepth),
                "Recursion depth is suspiciously high. Most likely the recursion went into some 3rd party type it shouldn't. I suggest debugging.");


        var properties = typeOfSingleItem.GetRuntimeProperties().ToList();
        var fingerprint = new StringBuilder();

        fingerprint.AppendLine($"Full type name: {typeOfSingleItem.FullName}");
        fingerprint.AppendLine("");

        foreach (var property in properties)
        {
            fingerprint.AppendLine($"Property: {property.Name}");
            fingerprint.AppendLine($"FillWithAIAttribute: {property.GetCustomAttribute<FillWithAIAttribute>() != null}");

            var ruleAttributes = property.GetCustomAttributes<FillWithAIRuleAttribute>();
            foreach (var ruleAttribute in ruleAttributes)
                fingerprint.AppendLine($"FillWithAIRuleAttribute text: {ruleAttribute.RuleText}");

            fingerprint.AppendLine($"");
        }

        // Also, include recursively fingerprints of all properties if their type is from the `GenerateFlashcards.*` namespace
        var childrenTypes = properties
            // Filter out properties that are not from the same assembly
            // e.g. don't try to recurse into System.String, JObject etc. as it ends with StackOverflowException
            .Where(x => x.PropertyType.Module.Name == typeOfSingleItem.Module.Name)
            .Select(x => x.PropertyType)
            .Distinct();

        foreach (var childType in childrenTypes)
        {
            var propertyFingerprint = GenerateTypeFingerprint(childType, recursionDepth + 1);
            fingerprint.AppendLine($"---\n");
            fingerprint.AppendLine(propertyFingerprint);
        }

        var generateTypeFingerprint = fingerprint.ToString();

        if (recursionDepth == 0)
            _typeFingerprints[typeOfSingleItem] = generateTypeFingerprint;

        return generateTypeFingerprint;
    }

}
