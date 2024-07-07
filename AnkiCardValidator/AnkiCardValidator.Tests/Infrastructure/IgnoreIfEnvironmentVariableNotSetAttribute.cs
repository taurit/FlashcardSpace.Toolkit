using System.Reflection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class IgnoreIfEnvironmentVariableNotSetAttribute(string environmentVariableName, string reasonMessage) : Attribute
{
    public void Decorate(ITestMethod testMethod)
    {
        var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
        if (string.IsNullOrEmpty(environmentVariableValue))
        {
            var skipAttribute = new IgnoreAttribute(reasonMessage);
            var skipMethodInfo = skipAttribute.GetType().GetMethod("SkipTestMethod", BindingFlags.Instance | BindingFlags.NonPublic);
            skipMethodInfo.Invoke(skipAttribute, new object[] { testMethod });
        }
    }
}
