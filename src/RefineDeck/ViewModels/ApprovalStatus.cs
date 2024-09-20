namespace RefineDeck.ViewModels;

public enum ApprovalStatus
{
    NotReviewedYet = 0,

    // Card looks fine, and can already be published to wide audience
    Approved,

    // Flashcard is inconsistent, confusing, or contains errors not worth fixing
    Rejected,

    // Card is interesting, but requires some team discussion before we can publish
    RequiresDiscussion
}
