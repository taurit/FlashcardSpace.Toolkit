using BookToAnki.Services;
using BookToAnki.Services.OpenAi;
using BookToAnki.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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


}
