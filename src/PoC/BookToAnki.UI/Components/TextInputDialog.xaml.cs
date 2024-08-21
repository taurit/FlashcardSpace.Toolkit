using System.Windows;

namespace BookToAnki.UI.Components;
/// <summary>
/// Interaction logic for TextInputDialog.xaml
/// </summary>
public partial class TextInputDialog : Window
{
    public string ResponseText { get; set; }
    public bool ScopeToWord { get; set; }

    public TextInputDialog(string question, string? defaultAnswer)
    {
        InitializeComponent();
        DataContext = this;
        Title = question;
        ResponseText = defaultAnswer ?? "";
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
