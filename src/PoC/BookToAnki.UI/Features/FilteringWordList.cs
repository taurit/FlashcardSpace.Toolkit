using BookToAnki.UI.ViewModels;
using NickBuhro.Translit;
using System;
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

    private void UpdateViewFilter()
    {
        var view = CollectionViewSource.GetDefaultView(WordsDataGrid.ItemsSource);

        view.Filter = obj =>
        {
            if (obj is not WordDataViewModel model) return false;

            var searchBoxTextAllowsDisplay =
                model.Word.Word.Contains(_searchBoxText, StringComparison.InvariantCultureIgnoreCase);

            var searchBoxTextAllowsDisplayTransliteration =
                model.Word.Word.Contains(_searchBoxTextTransliteratedToCyrillic,
                    StringComparison.InvariantCultureIgnoreCase);

            var shouldDisplayWord = (string.IsNullOrEmpty(_searchBoxText)
                                     || searchBoxTextAllowsDisplay
                                     || searchBoxTextAllowsDisplayTransliteration);
            return shouldDisplayWord;
        };
    }
}
