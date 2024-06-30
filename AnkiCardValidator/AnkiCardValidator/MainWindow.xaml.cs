using AnkiCardValidator.ViewModels;
using System.Windows;

namespace AnkiCardValidator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }
}
