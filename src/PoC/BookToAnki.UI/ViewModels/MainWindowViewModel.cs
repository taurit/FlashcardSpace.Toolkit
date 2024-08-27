using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace BookToAnki.UI.ViewModels;
[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    public bool IsBusy { get; set; } = true;
    public int NumberUniqueWordsInAllProcessedBooks { get; set; } = 0;
    public ObservableCollection<WordDataViewModel> Words { get; set; } = new();

    public ObservableCollection<WordDataViewModel> SimilarWords { get; set; } = new();

    public ObservableCollection<WordUsageExampleViewModel> SelectedWordUsages { get; set; } = new();

    public WordDataViewModel? SelectedWord { get; set; }
    public WordDataViewModel? SelectedSimilarWord { get; set; }

    public WordUsageExampleViewModel? SelectedWordUsage { get; set; }

    [DependsOn(nameof(Words), nameof(SelectedWordUsage), nameof(SelectedWordUsages))]
    public string HasPictureRatio => Words.Count == 0 ? "-" : (Words.Count(x => x.HasPicture) / (decimal)Words.Count).ToString("P2", CultureInfo.InvariantCulture);

    public decimal TotalCostUsdNumber { get; set; } = 0m;
    public string TotalCostUsd => TotalCostUsdNumber.ToString("C4", CultureInfo.GetCultureInfo("en-US"));

    public string TotalCostPlnWithTax => (TotalCostUsdNumber * 4.15103m * 1.23m).ToString("C", CultureInfo.GetCultureInfo("pl-PL"));
    public TimeSpan StartupTime { get; set; }
}
