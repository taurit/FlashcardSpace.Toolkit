using BookToAnki.Models;
using System;
using System.Windows;

namespace BookToAnki.UI.Components;
/// <summary>
/// Interaction logic for SoundEditorDialog.xaml
/// </summary>
public partial class SoundEditorDialog : Window
{
    public SoundEditorDialog(string paddedAudioFileName, TimeSpan padding)
    {
        InitializeComponent();
        this.soundEditor.LoadFile(paddedAudioFileName, padding);
    }

    public AudioShift? Shift { get; private set; }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedFragment = this.soundEditor.Selection;
        Shift = new AudioShift(selectedFragment.BeginningShiftRelativeToInitial, selectedFragment.EndShiftRelativeToInitial);
        DialogResult = true;
    }
}
