using NAudio.Wave;
using NAudio.WaveFormRenderer;
using SoundEditorControl.Models;
using SoundEditorControl.Utils;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundEditorControl;

/// <summary>
///     Interaction logic for SoundTrimmer.xaml
/// </summary>
public partial class SoundEditor : UserControl
{
    public SoundEditor()
    {
        InitializeComponent();
    }

    public void LoadFile(string fileLocation, TimeSpan initialPadding)
    {
        CurrentFileLocation = fileLocation;
        CurrentMp3FileReader = new Mp3FileReader(CurrentFileLocation);
        InitialPadding = initialPadding;

        WaveFormRenderer waveFormRenderer = new();
        using var waveStream = new AudioFileReader(fileLocation);
        var image = waveFormRenderer.Render(waveStream, new StandardWaveFormRendererSettings
        {
            TopHeight = 500,
            BottomHeight = 500,
            Width = 1200,
            BackgroundColor = Color.White,
            BottomPeakPen = new Pen(Color.IndianRed),
            TopPeakPen = new Pen(Color.MediumVioletRed)
        });

        waveformImage.Loaded += (sender, args) =>
        {
            SimulateClickToDrawInitialShift(initialPadding);
        };

        waveformImage.Source = image.ConvertDrawingImageToWpfImage();
    }

    private TimeSpan InitialPadding { get; set; }

    private void SimulateClickToDrawInitialShift(TimeSpan initialShiftBeginning)
    {
        var imageWidth = waveformImage.ActualWidth;
        var percentagePosition = (double)initialShiftBeginning.Ticks / CurrentMp3FileReader.TotalTime.Ticks;
        var initialPaddingInPixels = percentagePosition * imageWidth;

        leftBox.Width = initialPaddingInPixels;
        rightBox.Width = initialPaddingInPixels;
    }

    private Mp3FileReader? CurrentMp3FileReader { get; set; }

    private string? CurrentFileLocation { get; set; }

    private long MaxPosition => CurrentMp3FileReader.Length;

    private long BeginningShiftPosition
    {
        get
        {
            var shiftBeginningPercent = leftBox.ActualWidth / waveformImage.ActualWidth;
            var startPosition = (long)(shiftBeginningPercent * MaxPosition);

            return startPosition;
        }
    }

    private long EndShiftPosition
    {
        get
        {
            if (rightBox.ActualWidth == 0) return MaxPosition;// default position of indicator => no end trimming

            var shiftEndPercent = (waveformImage.ActualWidth - rightBox.ActualWidth) / waveformImage.ActualWidth;
            var endPosition = (long)(shiftEndPercent * MaxPosition);

            return endPosition;
        }
    }

    public SelectedSoundFragment Selection
    {
        get
        {
            var beginningShift = ConvertStreamPositionToTimeSpan(BeginningShiftPosition);
            var endShift = ConvertStreamPositionToTimeSpan(EndShiftPosition);
            var beginningShiftRelativeToInitial = beginningShift - InitialPadding;
            var endShiftRelativeToInitial = endShift - (CurrentMp3FileReader.TotalTime - InitialPadding);

            return new SelectedSoundFragment(beginningShift, endShift, beginningShiftRelativeToInitial, endShiftRelativeToInitial);
        }
    }


    private TimeSpan ConvertStreamPositionToTimeSpan(long streamPosition)
    {
        var maxTicks = CurrentMp3FileReader.Length;
        var totalTime = CurrentMp3FileReader.TotalTime;


        var ratio = (double)streamPosition / maxTicks;
        var convertedTicks = (long)(totalTime.Ticks * ratio);

        return new TimeSpan(convertedTicks);
    }

    private void WaveformImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var clickedPoint = e.GetPosition(waveformImage);
        leftBox.Width = clickedPoint.X;
    }

    private void WaveformImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var clickedPoint = e.GetPosition(waveformImage);
        var left = clickedPoint.X;
        rightBox.Width = waveformImage.ActualWidth - left;
    }

    private void PlayTrimmedSamplePreview_OnClick(object sender, RoutedEventArgs e)
    {
        var trimmedStream = new TrimmedStream(CurrentMp3FileReader, BeginningShiftPosition, EndShiftPosition);

        var waveOut = new WaveOut();
        waveOut.Init(trimmedStream);
        waveOut.Play();
    }
}
