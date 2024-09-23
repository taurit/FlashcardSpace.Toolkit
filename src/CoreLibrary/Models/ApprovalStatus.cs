namespace CoreLibrary.Models;

public enum ApprovalStatus
{
    NotReviewedYet = 0,

    // Card looks fine, and can already be published to wide audience
    Approved,

    // Flashcard is inconsistent, confusing, or contains errors not worth fixing
    Rejected,
}
