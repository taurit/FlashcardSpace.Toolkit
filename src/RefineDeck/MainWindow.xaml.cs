using RefineDeck.Utils;
using RefineDeck.ViewModels;
using System.Windows;

namespace RefineDeck;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
        ViewModel.Deck = DeckLoader.LoadDeck();
    }
}
