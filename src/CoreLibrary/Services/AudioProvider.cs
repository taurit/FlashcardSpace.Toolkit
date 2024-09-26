using CoreLibrary.Interfaces;
using CoreLibrary.Models;
using CoreLibrary.Services.GenerativeAiClients.TextToSpeech;
using CoreLibrary.Utilities;
using Microsoft.Extensions.Logging;

namespace CoreLibrary.Services;

public record AudioProviderSettings(string AudioCacheFolder);

/// <summary>
/// Provides audio files for flashcards.
/// </summary>
public class AudioProvider(AudioProviderSettings settings, TextToSpeechClient ttsClient, ILogger<AudioProvider> logger)
{
    public async Task<List<FlashcardNote>> AddAudio(
        List<FlashcardNote> notes,
        SupportedLanguage sourceLanguage,
        SupportedLanguage targetLanguage
        )
    {
        settings.AudioCacheFolder.EnsureDirectoryExists();

        var newNotes = new List<FlashcardNote>();

        foreach (var note in notes)
        {
            logger.LogInformation("Generating audio file for {Term}", note.Term);

            var termAudio = await GenerateAudioOrUseCached(note.Term, sourceLanguage);
            var termTranslationAudio = await GenerateAudioOrUseCached(note.TermTranslation, targetLanguage);
            var contextAudio = await GenerateAudioOrUseCached(note.Context, sourceLanguage);
            var contextTranslationAudio = await GenerateAudioOrUseCached(note.ContextTranslation, targetLanguage);

            var noteWithAudio = note with
            {
                TermAudio = termAudio,
                TermTranslationAudio = termTranslationAudio,
                ContextAudio = contextAudio,
                ContextTranslationAudio = contextTranslationAudio
            };
            newNotes.Add(noteWithAudio);

        }
        return newNotes;
    }

    public async Task<string> GenerateAudioOrUseCached(string text, SupportedLanguage language)
    {
        settings.AudioCacheFolder.EnsureDirectoryExists();

        var textFingerprint = text.GetHashCodeStable(5);
        var audioFileName = $"{language}_{text.ToFilenameFriendlyString(15)}_{textFingerprint}.mp3";
        var audioFilePath = Path.Combine(settings.AudioCacheFolder, audioFileName);
        if (!File.Exists(audioFilePath))
        {
            var audioData = await ttsClient.GenerateAudioFile(text, language);
            await File.WriteAllBytesAsync(audioFilePath, audioData);
        }
        return audioFilePath;
    }
}
