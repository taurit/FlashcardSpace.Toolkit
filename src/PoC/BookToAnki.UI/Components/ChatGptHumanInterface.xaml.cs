using PropertyChanged;
using System.Windows;
using System.Windows.Navigation;

namespace BookToAnki.UI.Components;

[AddINotifyPropertyChangedInterface]
public class ChatGptHumanInterfaceViewModel
{
    public ChatGptHumanInterfaceViewModel(string promptToCopyToChatGpt)
    {
        Prompt = promptToCopyToChatGpt;
    }

    public string Prompt { get; set; }
    public string Response { get; set; } = "";
}

public partial class ChatGptHumanInterface : Window
{
    public ChatGptHumanInterface(string promptToCopyToChatGpt)
    {
        DataContext = new ChatGptHumanInterfaceViewModel(promptToCopyToChatGpt);
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
        e.Handled = true;
    }
}
