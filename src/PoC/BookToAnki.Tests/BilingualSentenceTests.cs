using BookToAnki.Models;

namespace BookToAnki.Tests;

[TestClass]
public class BilingualSentenceTests
{
    [TestMethod]
    public void When_SentenceIsCreated_Expect_NullOrEmptyValuesAreNotAccepted()
    {
        // Arrange

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new BilingualSentence("Primary language", null!));
        Assert.ThrowsException<ArgumentNullException>(() => new BilingualSentence(null!, "Secondary language"));

        Assert.ThrowsException<ArgumentException>(() => new BilingualSentence("Primary language", ""));
        Assert.ThrowsException<ArgumentException>(() => new BilingualSentence("Primary language", " \t"));
        Assert.ThrowsException<ArgumentException>(() => new BilingualSentence("", "Secondary language"));
        Assert.ThrowsException<ArgumentException>(() => new BilingualSentence("\t ", "Secondary language"));

    }
}
