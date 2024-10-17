﻿using CoreLibrary.Models;
using RefineDeck.ViewModels;
using System.Collections.ObjectModel;
using System.IO;

namespace RefineDeck.Utils;

public record ImageCandidate(int? Index, string? RelativePath, string? AbsolutePath);

internal static class DeckLoader
{
    public static DeckViewModel LoadDeck()
    {
        var deckPath = CommandLineHelper.GetDeckFolderPath();
        var deck = Deck.DeserializeFromFile(deckPath.DeckManifestEditsPathWithFallback);

        var flashcardsViewModels = new List<ReviewedCardViewModel>();

        foreach (var flashcard in deck.Flashcards)
        {
            var imageCandidates = new ObservableCollection<ImageCandidate>();
            for (int i = 0; i < flashcard.ImageCandidates.Count; i++)
            {
                var relativePath = flashcard.ImageCandidates[i];
                var absolutePath = Path.Combine(deckPath.DeckDataPath, relativePath);
                imageCandidates.Add(new ImageCandidate(i, relativePath, absolutePath));
            }

            // add "no image" option
            var noImagePlaceholderPath = String.Intern(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "no_image_placeholder.png"));
            imageCandidates.Add(new ImageCandidate(null, null, noImagePlaceholderPath));

            var flashcardViewModel = new ReviewedCardViewModel()
            {
                OriginalFlashcard = flashcard,

                Term = flashcard.Overrides?.Term ?? flashcard.Term,
                TermAudio = flashcard.Overrides?.TermAudio ?? flashcard.TermAudio,
                TermTranslation = flashcard.Overrides?.TermTranslation ?? flashcard.TermTranslation,
                TermTranslationAudio = flashcard.Overrides?.TermTranslationAudio ?? flashcard.TermTranslationAudio,
                Remarks = flashcard.Overrides?.Remarks ?? flashcard.Remarks,
                SentenceExample = flashcard.Overrides?.Context ?? flashcard.Context,
                SentenceExampleAudio = flashcard.Overrides?.ContextAudio ?? flashcard.ContextAudio,
                SentenceExampleTranslation = flashcard.Overrides?.ContextTranslation ?? flashcard.ContextTranslation,
                SelectedImageIndex = flashcard.Overrides?.SelectedImageIndex ?? flashcard.SelectedImageIndex,
                QaSuggestions = flashcard.Overrides?.QaSuggestions ?? flashcard.QaSuggestions,

                ApprovalStatus = flashcard.ApprovalStatus,
                ImageCandidates = imageCandidates,

            };

            flashcardsViewModels.Add(flashcardViewModel);
        }

        var deckViewModel = new DeckViewModel()
        {
            DeckPath = deckPath,
            MediaFileNamePrefix = deck.MediaFilesPrefix,
            Flashcards = new ObservableCollection<ReviewedCardViewModel>(flashcardsViewModels),
            SourceLanguage = deck.SourceLanguage,
            TargetLanguage = deck.TargetLanguage

        };
        return deckViewModel;
    }

    public static void SaveChangesInDeck(DeckViewModel viewModel)
    {
        var deckPath = CommandLineHelper.GetDeckFolderPath();
        var originalDeck = Deck.DeserializeFromFile(deckPath.DeckManifestPath);

        // construct edited deck based on original deck and ViewModel containing changes
        if (originalDeck.Flashcards.Count != viewModel.Flashcards.Count)
            throw new InvalidOperationException("Number of flashcards in original deck and ViewModel differ");

        var editedDeck = new Deck(originalDeck.DeckName, new List<FlashcardNote>(), originalDeck.MediaFilesPrefix, originalDeck.SourceLanguage, originalDeck.TargetLanguage);
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
                    Remarks = vm.IsRemarksFieldOverridden ? vm.Remarks : null!,
                    Context = vm.IsSentenceExampleOverridden ? vm.SentenceExample : null!,
                    ContextTranslation = vm.IsSentenceExampleTranslationOverridden ? vm.SentenceExampleTranslation : null!,
                    SelectedImageIndex = imageIndexToSet,

                    TermAudio = vm.IsTermAudioOverridden ? vm.TermAudio : null!,
                    TermTranslationAudio = vm.IsTermTranslationAudioOverridden ? vm.TermTranslationAudio : null!,
                    ContextAudio = vm.IsSentenceExampleAudioOverridden ? vm.SentenceExampleAudio : null!,
                    ContextTranslationAudio = vm.IsSentenceExampleTranslationAudioOverridden ? vm.SentenceExampleTranslationAudio : null!,

                    QaSuggestions = vm.IsQaSuggestionsOverridden ? vm.QaSuggestions : null!

                } : null
            };
            editedDeck.Flashcards.Add(edited);
        }

        var deckSerialized = editedDeck.Serialize();
        File.WriteAllText(deckPath.DeckManifestEditsPath, deckSerialized);
    }

}
