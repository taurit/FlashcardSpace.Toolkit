using BookToAnki.NotePropertiesDatabase;
using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using BookToAnki.UI.Components;
using BookToAnki.UI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace BookToAnki.UI;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;
    public ServiceProvider ServiceProvider => _serviceProvider;

    public App()
    {
        StartupPerformanceMeasurement.StartMeasuring();

        // Set up Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Set up exception handling
        _serviceProvider.GetService<GlobalExceptionHandler>()!.SetUpExceptionHandling(this);
    }



    private void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<GlobalExceptionHandler>();
        services.AddSingleton<LocalWebServer>();
        services.AddSingleton<BookListLoader>();
        services.AddSingleton<ImageCompression>();

        var pictureSelectionService = new PictureSelectionService(Settings.ImagesRepositoryFolder);
        services.AddSingleton(pictureSelectionService);

        services.AddSingleton<MainWindow>();
        services.AddTransient<NoteRatingFlow>();

        var wordsLinked = new WordsLinker(Settings.LinkedWordsFilePath);
        services.AddSingleton(wordsLinked);

        var settings = new Settings();
        services.AddTransient<Settings>(_ => settings);

        var embeddingsCacheManager = new EmbeddingsCacheManager(Settings.UkrainianEmbeddingsDatabaseFilePath);
        var embeddingsServiceWrapper = new EmbeddingsServiceWrapper(settings.OpenAiDeveloperKey, settings.OpenAiOrganization, embeddingsCacheManager);
        services.AddSingleton(embeddingsServiceWrapper);

        services.AddTransient<UkrainianWordSimilarityEvaluator>();


        var openAiService = new OpenAiServiceWrapper(
            settings.OpenAiDeveloperKey,
            settings.OpenAiOrganization,
            Settings.OpenAiResponsesDatabase,
            new FallbackManualOpenAiServiceWrapper()
        );
        services.AddSingleton(_ => openAiService);

        var selectedWordHighlighter = new SelectedWordHighlighter();
        services.AddSingleton(_ => selectedWordHighlighter);

        var dalleService = new DalleServiceWrapper(settings.OpenAiDeveloperKey, settings.OpenAiOrganization);
        services.AddSingleton(_ => dalleService);

        var noteProperties = new NoteProperties(Settings.NotePropertiesDatabaseFileName);
        services.AddSingleton(_ => noteProperties);

        var audioSampleSelector = new AudioSampleSelector(noteProperties);
        services.AddSingleton(_ => audioSampleSelector);

        var partsOfSpeech =
            new PartOfSpeechDictionaryBuilder().BuildPartOfSpeechDictionaryFromFile(Settings.UkrainianPartsOfSpeech);
        services.AddSingleton(_ => partsOfSpeech);

        var ukrainianStressHighlighter = new UkrainianStressHighlighterWithCache(Settings.UkrainianStressCache);
        services.AddSingleton(_ => ukrainianStressHighlighter);

        var audioExampleProvider = new AudioExampleProvider(Settings.AudioFilesCacheFolder);
        services.AddSingleton(_ => audioExampleProvider);

        var ukrainianWordExplainer = new UkrainianWordExplainer(openAiService, noteProperties);
        services.AddSingleton(_ => ukrainianWordExplainer);

        var ankiNoteGenerator = new AnkiNoteGenerator(ukrainianStressHighlighter, audioExampleProvider,
            selectedWordHighlighter, ukrainianWordExplainer, audioSampleSelector, noteProperties);
        services.AddSingleton(_ => ankiNoteGenerator);

        var hasPictureService = new HasPictureService(wordsLinked, Settings.ImagesRepositoryFolder);
        services.AddSingleton(_ => hasPictureService);
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var mainWindow = _serviceProvider.GetService<MainWindow>();
        mainWindow!.Show();
    }
}
