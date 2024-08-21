using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BookToAnki.UI;

public class Settings
{
    public const string BooksRootFolder = """d:\Flashcards\Books\""";
    public const string ImagesRepositoryFolder = """d:\Flashcards\Pictures\""";

    public const string RootServerFolder = """d:\Flashcards\""";
    public const string OpenAiResponsesDatabase = """d:\Flashcards\Words\OpenAiResponses\OpenAiResponses.sqlite""";


    public const string AudioFilesCacheFolder = """s:\Caches\BookToAnki\AudioSentences\""";
    public const string UkrainianStressCache = """s:\Caches\BookToAnki\ukrainian_stresses_cache.json""";
    public const string UkrainianEmbeddingsDatabaseFilePath = "d:\\Flashcards\\Words\\ukrainian_sentences_embeddings.bin";

    public const string UkrainianPartsOfSpeech = """d:\Flashcards\Words\ukrainian_parts_of_speech_automated.txt""";
    public const string NotePropertiesDatabaseFileName = """d:\Flashcards\Words\note_properties_database.sqlite""";

    public const string LinkedWordsFilePath = """d:\Flashcards\Words\linked_words.json""";
    public const string LinkingExceptionsStore = """d:\Flashcards\Words\linked_words_exceptions.json""";

    public const string SentenceMatchesCacheFolder = """s:\Caches\BookToAnki\SentenceMatches""";

    // How many examples do I need refined to consider word "refined enough to include in a card set?"
    // I started with 3, but reducing to 2 should make releasing the product notably more realistic, and perhaps 2 examples are enough to explain a word and not confuse...
    public const int NumPerfectExamplesToConsiderCardDone = 2;

    public readonly string OpenAiDeveloperKey;
    public readonly string OpenAiOrganization;

    public Settings()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<Settings>();
        var configuration = builder.Build();
        OpenAiDeveloperKey = configuration["OpenAiDeveloperKey"] ??
                             throw new InvalidOperationException(
                                 "OpenAiDeveloperKey is missing in User Secrets configuration");
        OpenAiOrganization = configuration["OpenAiOrganization"] ??
                             throw new InvalidOperationException(
                                 "OpenAiOrganization is missing in User Secrets configuration");

        if (!Directory.Exists(AudioFilesCacheFolder))
        {
            Directory.CreateDirectory(AudioFilesCacheFolder);
        }

    }


}
