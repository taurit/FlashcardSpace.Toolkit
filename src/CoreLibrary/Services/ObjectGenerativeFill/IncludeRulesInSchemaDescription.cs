using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Includes data from the `[FillWithAIRule]` attributes into the schema `description` property.
/// This way, OpenAI API can use the annotations when generating the output.
/// </summary>
class IncludeRulesInSchemaDescription : JSchemaGenerationProvider
{
    public override JSchema? GetSchema(JSchemaTypeGenerationContext context)
    {
        JSchema? schemaForType = null;

        // read data from custom attributes and put them into the schema description
        if (context.MemberProperty?.AttributeProvider is not null)
        {
            var ruleAttributes = context.MemberProperty.AttributeProvider.GetAttributes(typeof(FillWithAIRuleAttribute), false).ToList();
            if (ruleAttributes.Count > 0)
            {
                var description = String.Join(". ", ruleAttributes.Select(a => ((FillWithAIRuleAttribute)a).RuleText.TrimEnd('.'))) + ".";
                schemaForType = context.Generator.Generate(context.ObjectType);
                schemaForType.Description = description;
            }
        }

        return schemaForType;

    }
}
