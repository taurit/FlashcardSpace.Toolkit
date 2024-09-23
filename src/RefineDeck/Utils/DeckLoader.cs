using CoreLibrary.Models;
using RefineDeck.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace RefineDeck.Utils;

public record ImageCandidate(int? Index, string? RelativePath, string? AbsolutePath);

internal static class DeckLoader
{
    public static DeckViewModel LoadDeck()
    {
        var deckFolderPath = CommandLineHelper.GetDeckFolderPath();
        var deckManifestPath = GetDeckManifestPath(deckFolderPath);

        var deck = Deck.DeserializeFromFile(deckManifestPath);

        var flashcardsViewModels = new List<ReviewedCardViewModel>();

        foreach (var flashcard in deck.Flashcards)
        {
            var imageCandidates = new ObservableCollection<ImageCandidate>();
            for (int i = 0; i < flashcard.ImageCandidates.Count; i++)
            {
                var relativePath = flashcard.ImageCandidates[i];
                var absolutePath = Path.Combine(deckFolderPath, relativePath);
                imageCandidates.Add(new ImageCandidate(i, relativePath, absolutePath));
            }

            // add "no image" option
            imageCandidates.Add(new ImageCandidate(null, null, null));

            var flashcardViewModel = new ReviewedCardViewModel()
            {
                OriginalFlashcard = flashcard,

                Term = flashcard.Overrides?.Term ?? flashcard.Term,
                TermTranslation = flashcard.Overrides?.TermTranslation ?? flashcard.TermTranslation,
                TermDefinition = flashcard.Overrides?.TermDefinition ?? flashcard.TermDefinition,
                SentenceExample = flashcard.Overrides?.Context ?? flashcard.Context,
                SentenceExampleTranslation = flashcard.Overrides?.ContextTranslation ?? flashcard.ContextTranslation,
                SelectedImageIndex = flashcard.Overrides?.SelectedImageIndex ?? flashcard.SelectedImageIndex,

                ApprovalStatus = flashcard.ApprovalStatus,
                ImageCandidates = imageCandidates,
            };

            flashcardsViewModels.Add(flashcardViewModel);
        }

        var deckViewModel = new DeckViewModel()
        {
            DeckFolderPath = deckFolderPath,
            Flashcards = new ObservableCollection<ReviewedCardViewModel>(flashcardsViewModels)
        };
        return deckViewModel;
    }

    public static void SaveChangesInDeck(DeckViewModel viewModel)
    {
        var deckFolderPath = CommandLineHelper.GetDeckFolderPath();

        var deckManifestPath = Path.Combine(deckFolderPath, "flashcards.json");
        var deckManifestEditedPath = Path.Combine(deckFolderPath, "flashcards.edited.json");

        var originalDeck = Deck.DeserializeFromFile(deckManifestPath);

        // construct edited deck based on original deck and ViewModel containing changes
        if (originalDeck.Flashcards.Count != viewModel.Flashcards.Count)
            throw new InvalidOperationException("Number of flashcards in original deck and ViewModel differ");

        var editedDeck = new Deck(originalDeck.DeckName, new List<FlashcardNote>());
        for (var index = 0; index < originalDeck.Flashcards.Count; index++)
        {
            var original = originalDeck.Flashcards[index];
            var vm = viewModel.Flashcards[index];

            // sanity check - ensure that the card in ViewModel corresponds to the card in the original deck
            if (original.Term != vm.OriginalFlashcard.Term)
                throw new InvalidOperationException("Term in original deck and ViewModel differ");
            if (original.TermTranslation != vm.OriginalFlashcard.TermTranslation)
                throw new InvalidOperationException("TermTranslation in original deck and ViewModel differ");

            // update
            var imageIndexToSet = vm.IsSelectedImageIndexOverridden ? vm.SelectedImageIndex : null;
            var edited = original with
            {
                ApprovalStatus = vm.ApprovalStatus,
                // only save overrides if anything changed, to make DIFF views clean from unintended changes
                Overrides = vm.IsAnythingOverridden ? new FlashcardNoteEditablePart
                {
                    Term = vm.IsTermOverridden ? vm.Term : null!,
                    TermTranslation = vm.IsTermTranslationOverridden ? vm.TermTranslation : null!,
                    TermDefinition = vm.IsTermDefinitionOverridden ? vm.TermDefinition : null!,
                    Context = vm.IsSentenceExampleOverridden ? vm.SentenceExample : null!,
                    ContextTranslation = vm.IsSentenceExampleTranslationOverridden ? vm.SentenceExampleTranslation : null!,
                    SelectedImageIndex = imageIndexToSet
                } : null
            };
            editedDeck.Flashcards.Add(edited);
        }

        var deckSerialized = editedDeck.Serialize();
        File.WriteAllText(deckManifestEditedPath, deckSerialized);
    }

    private static string GetDeckManifestPath(string deckFolderPath)
    {
        // save edited deck to a separate file for easier debugging (diffing),
        // and keep the original file immutable
        var deckManifestEditedPath = Path.Combine(deckFolderPath, "flashcards.edited.json");
        var deckManifestPath = Path.Combine(deckFolderPath, "flashcards.json");
        var deckPathToUse = File.Exists(deckManifestEditedPath) ? deckManifestEditedPath : deckManifestPath;
        return deckPathToUse;
    }
}
