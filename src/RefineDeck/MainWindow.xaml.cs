using System.Windows;

namespace RefineDeck;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        string[] args = Environment.GetCommandLineArgs();

        if (args.Length > 1)
        {
            var launchParameter = args[1];
            if (launchParameter.StartsWith("refinedeck:///"))
                launchParameter = launchParameter.Replace("refinedeck:///", "");

            this.Title = $"Refine deck: {launchParameter}";
        }


    }
}
