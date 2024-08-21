

using BookToAnki.Services;
using BookToAnki.UI.ViewModels;
using NickBuhro.Translit;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace BookToAnki.UI;

public partial class MainWindow
{
    private string _searchBoxText = "";
    private string _searchBoxTextTransliteratedToCyrillic = "";

    public string SearchBoxText
    {
        get => _searchBoxText;
        set
        {
            _searchBoxText = value;
            _searchBoxTextTransliteratedToCyrillic =
                Transliteration.LatinToCyrillic(value, NickBuhro.Translit.Language.Ukrainian);
            UpdateViewFilter();
        }
    }

    private void MultiSelectListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // skip SetBusy, as the operation will call SelectionChanged that already locks
        UpdateViewFilter();
    }

    private void UpdateViewFilter()
    {
        var visiblePartsOfSpeech =
            Enumerable.Cast<PartOfSpeech>(MultiSelectListBox.SelectedItems).Select(x => x.Name).ToHashSet();
        var view = CollectionViewSource.GetDefaultView(WordsDataGrid.ItemsSource);

        view.Filter = obj =>
        {
            if (obj is not WordDataViewModel model) return false;

            var partOfSpeechFilterAllowsDisplay =
                visiblePartsOfSpeech.Count == 0 || visiblePartsOfSpeech.Contains(model.PartOfSpeech);
            var searchBoxTextAllowsDisplay =
                model.Word.Word.Contains(_searchBoxText, StringComparison.InvariantCultureIgnoreCase);

            var searchBoxTextAllowsDisplayTransliteration =
                model.Word.Word.Contains(_searchBoxTextTransliteratedToCyrillic,
                    StringComparison.InvariantCultureIgnoreCase);

            var shouldDisplayWord = partOfSpeechFilterAllowsDisplay && (string.IsNullOrEmpty(_searchBoxText) ||
                                                                        searchBoxTextAllowsDisplay ||
                                                                        searchBoxTextAllowsDisplayTransliteration);
            return shouldDisplayWord;
        };
    }
}
