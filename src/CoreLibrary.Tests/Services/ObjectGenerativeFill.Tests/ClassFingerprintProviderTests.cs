using CoreLibrary.Services.ObjectGenerativeFill;
using FluentAssertions;

namespace CoreLibrary.Tests.Services.ObjectGenerativeFill.Tests;

[TestClass]
public class ClassFingerprintProviderTests
{
    private class MyTestClass
    {
        internal MyTestClassDepth1 MyProperty1 { get; set; }
    }

    private class MyTestClassDepth1
    {
        [FillWithAIRule("Test rule 123")]
        internal MyTestClassDepth2 MyProperty2 { get; set; }
    }

    private class MyTestClassDepth2
    {
        [FillWithAIRule("Test rule 456")]
        internal int MyProperty3 { get; set; }
    }

    [TestMethod]
    public void GenerateTypeFingerprint_ShouldContainTypeNamesFromAllDepthsWithinFingerprint()
    {
        // Arrange
        var typeOfSingleItem = typeof(MyTestClass);

        // Act
        var fingerprint = ClassFingerprintProvider.GenerateTypeFingerprint(typeOfSingleItem);
        Console.WriteLine(fingerprint);

        // Assert
        fingerprint.Should().Contain(typeOfSingleItem.FullName);
        fingerprint.Should().Contain(typeof(MyTestClassDepth1).FullName);
        fingerprint.Should().Contain(typeof(MyTestClassDepth2).FullName);
    }

    [TestMethod]
    public void GenerateTypeFingerprint_ShouldContainPropertiesFromAllDepthsNameInFingerprint()
    {
        // Arrange
        var typeOfSingleItem = typeof(MyTestClass);

        // Act
        var fingerprint = ClassFingerprintProvider.GenerateTypeFingerprint(typeOfSingleItem);
        Console.WriteLine(fingerprint);

        // Assert
        fingerprint.Should().Contain("MyProperty1");
        fingerprint.Should().Contain("MyProperty2");
        fingerprint.Should().Contain("MyProperty3");
    }

    [TestMethod]
    public void GenerateTypeFingerprint_ShouldContainFillWithAIRuleAttributesInFingerprint()
    {
        // Arrange
        var typeOfSingleItem = typeof(MyTestClass);

        // Act
        var fingerprint = ClassFingerprintProvider.GenerateTypeFingerprint(typeOfSingleItem);
        Console.WriteLine(fingerprint);

        // Assert
        fingerprint.Should().Contain("Test rule 123");
        fingerprint.Should().Contain("Test rule 456");
    }
}
