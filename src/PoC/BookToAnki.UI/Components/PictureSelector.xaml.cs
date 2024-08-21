using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using BookToAnki.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace BookToAnki.UI.Components;

[AddINotifyPropertyChangedInterface]
public partial class PictureSelector : UserControl, INotifyPropertyChanged
{
    private readonly PictureSelectionService _pictureSelectionService;
    private readonly DalleServiceWrapper _dalleService;

    public PictureSelector()
    {
        var serviceProvider = (Application.Current as App)?.ServiceProvider ??
                              throw new InvalidOperationException("Could not get DI container from App");
        _pictureSelectionService = serviceProvider?.GetService<PictureSelectionService>() ??
                                   throw new InvalidOperationException("Could not get PictureSelectionService dependency from DI container");
        _dalleService = serviceProvider?.GetService<DalleServiceWrapper>() ??
                                   throw new InvalidOperationException("Could not get DalleServiceWrapper dependency from DI container");

        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<ImageInfo>? ImageList { get; set; }

    private string? Word { get; set; }
    private string? Sentence { get; set; }
    private string? PreviousSentence { get; set; }
    private string? NextSentence { get; set; }

    public string Prompt =>
        $"Generate image suitable as illustration for a digital flashcard that will help explain the meaning of а Ukrainian word \"{Word}\" to a foreigner." +
        $"\n" +
        $"\nBelow is a fragment of a book where the word was found. It is only provided to help clarify the meaning it it has more than one. Don't try to illustrate the fragment in an image.\n\n" +
        $"```fragment\n" +
        $"{PreviousSentence} {Sentence} {NextSentence}\n" +
        $"```\n\n" +
        $"The goal of image is to best represent word's meaning. Avoid overly complex scenes. DO NOT show any text or symbols. The picture should have no references to Ukraine, its flag or symbols. Ideally, it should resemble a real photo. If the word might be related to copyrighted content, generate one close enough in meaning that doesn't violate copyright.";

    private ImageInfo? SelectedImage { get; set; }

    internal void SetNewContext(string word, string sentence, string? previousSentenceText, string? nextSentenceText)
    {
        Sentence = sentence;
        Word = word;
        PreviousSentence = previousSentenceText;
        NextSentence = nextSentenceText;

        var imageCandidates = _pictureSelectionService.GetImageCandidates(word, sentence);
        var imageCandidatesViewModel = imageCandidates.Select(x => new ImageInfo(x.FullPath, x.IsSelected));

        ImageList = new ObservableCollection<ImageInfo>(imageCandidatesViewModel);
    }

    private void SelectImage(object sender, MouseButtonEventArgs e)
    {
        if (ImageList is null) return;
        var border = sender as Border;
        if (border is null) return;
        SelectedImage = border.Tag as ImageInfo;
        if (SelectedImage is null) return;

        foreach (var image in ImageList) image.IsSelected = image == SelectedImage;

        _pictureSelectionService.ChooseImage(SelectedImage.FileName);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
        e.Handled = true;
    }

    private async void GenerateImageWithDalle3_OnClick(object sender, RoutedEventArgs e)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Word);

        var pathPartial = Path.Combine(Settings.ImagesRepositoryFolder, Word.ToLowerInvariant());
        if (!Directory.Exists(pathPartial)) Directory.CreateDirectory(pathPartial);

        string imagePath = Path.Combine(pathPartial, "dalle3.standard.webp");
        if (File.Exists(imagePath))
        {
            Debug.WriteLine($"Dalle3 image creation skipped for word {Word} - image already exists");
            return;
        }

        Stopwatch s = Stopwatch.StartNew();
        var image = await _dalleService.CreateDalle3Image(Prompt, DalleServiceWrapper.Dalle3ImageQuality.standard);
        s.Stop();

        var imageBytes = Convert.FromBase64String(image.ImageContentBase64);
        await File.WriteAllBytesAsync(imagePath, imageBytes);

        Debug.WriteLine($"Dalle3 image created in: {s.Elapsed.TotalMilliseconds}");
    }
}
