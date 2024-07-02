using AnkiCardValidator.ViewModels;
using System.Windows;

namespace AnkiCardValidator;

public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }

    private void LoadFlashcards_OnClick(object sender, RoutedEventArgs e)
    {
        Console.WriteLine(this.ViewModel.SelectedFlashcard is null);
    }
}
