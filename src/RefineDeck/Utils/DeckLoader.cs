using CoreLibrary.Models;
using RefineDeck.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace RefineDeck.Utils;
internal static class DeckLoader
{
    public static DeckViewModel LoadDeck()
    {
        var deckPath = CommandLineHelper.GetDeckFolderPath();

        var deckManifestPath = Path.Combine(deckPath, "flashcards.json");

        var deckSerialized = File.ReadAllText(deckManifestPath);
        var deckDeserialized = Deck.Deserialize(deckSerialized);

        var flashcardsViewModels = deckDeserialized.Flashcards.Select(flashcard => new ReviewedCardViewModel()
        {
            OriginalFlashcard = flashcard,

            Term = flashcard.Term,
            TermTranslation = flashcard.TermTranslation,

            SentenceExample = flashcard.Context,
            SentenceExampleTranslation = flashcard.ContextTranslation,

            ImageCandidates = new ObservableCollection<string>(flashcard.ImageCandidates
            .Select(relativePath => Path.Combine(deckPath, relativePath))
            ),

            ApprovalStatus = ApprovalStatus.Approved
        });

        var deckViewModel = new DeckViewModel()
        {
            DeckFolderPath = deckPath,
            Flashcards = new ObservableCollection<ReviewedCardViewModel>(flashcardsViewModels)
        };
        return deckViewModel;
    }
}
