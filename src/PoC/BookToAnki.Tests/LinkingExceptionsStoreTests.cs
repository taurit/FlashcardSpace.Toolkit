using BookToAnki.Services;
using FluentAssertions;
using System.Reflection;

namespace BookToAnki.Tests;

[TestClass]
public class LinkingExceptionsStoreTests
{
    private LinkingExceptionsStore _store;
    private string _testFileName;

    [TestInitialize]
    public void Initialize()
    {
        // Use a unique file name for each test to avoid conflicts
        _testFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "testExceptions.json");
        _store = new LinkingExceptionsStore(_testFileName);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_testFileName))
            File.Delete(_testFileName);
    }

    [TestMethod]
    public void AddException_ShouldAddWordPair()
    {
        // Arrange
        var word1 = "hello";
        var word2 = "world";

        // Act
        _store.AddException(word1, word2);

        // Assert
        _store.IsInExceptionList(word1, word2).Should().BeTrue();
    }

    [TestMethod]
    public void IsInExceptionList_ShouldReturnTrueForAddedPair()
    {
        // Arrange
        var word1 = "good";
        var word2 = "morning";
        _store.AddException(word1, word2);

        // Act
        var result = _store.IsInExceptionList(word1, word2);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void IsInExceptionList_ShouldReturnFalseForNonAddedPair()
    {
        // Arrange
        var word1 = "good";
        var word2 = "night";

        // Act
        var result = _store.IsInExceptionList(word1, word2);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Persistence_ShouldPersistExceptions()
    {
        // Arrange
        var word1 = "test";
        var word2 = "case";
        _store.AddException(word1, word2);

        // Act
        var newStore = new LinkingExceptionsStore(_testFileName);

        // Assert
        newStore.IsInExceptionList(word1, word2).Should().BeTrue();
    }

    [TestMethod]
    public void WordOrderIrrelevant_ShouldReturnTrueRegardlessOfOrder()
    {
        // Arrange
        var word1 = "first";
        var word2 = "second";
        _store.AddException(word1, word2);

        // Act & Assert
        _store.IsInExceptionList(word2, word1).Should().BeTrue();
    }
}
