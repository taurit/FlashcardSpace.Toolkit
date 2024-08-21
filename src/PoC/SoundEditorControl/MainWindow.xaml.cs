using System;
using System.Windows;

namespace SoundEditorControl;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        mySoundEditor.LoadFile("s:\\Caches\\BookToAnki\\AudioSentences\\А_біля_Герміони_й_тієї_рейвенкловської_старости_знайшли_дзеркальце.mp3",
            new TimeSpan(0, 0, 0, 1)
            );
    }


    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(mySoundEditor.Selection.ToString());
    }
}
