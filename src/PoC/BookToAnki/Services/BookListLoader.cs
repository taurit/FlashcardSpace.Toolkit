using BookToAnki.Models;

namespace BookToAnki.Services;

public class BookListLoader
{
    public List<SingleBookMetadata> GetBookList(string booksFolder)
    {
        var books = new List<SingleBookMetadata>();
        var subdirectories = Directory
            .GetDirectories(booksFolder)
            .Where(path =>
                path.Contains("hp_01") ||
                path.Contains("hp_02") ||
                path.Contains("hp_03") ||
                path.Contains("hp_04") ||
                path.Contains("hp_05") ||
                path.Contains("hp_06") ||
                path.Contains("hp_07")
            );

        foreach (var subdirectory in subdirectories)
        {
            var audioFiles = Directory.GetFiles(subdirectory, "*.mp3")
                .ToList();

            if (audioFiles.Any())
            {
                var sentenceFiles = Directory.GetFiles(subdirectory, "*.sentences.*");
                var en = sentenceFiles.Single(x => x.Contains("_en_") && x.Contains(".original."));
                var pl = sentenceFiles.Single(x => x.Contains("_pl_") && x.Contains(".original."));
                var uk = sentenceFiles.Single(x => x.Contains("_uk_") && x.Contains(".original."));

                var enUk = sentenceFiles.Single(x => x.Contains("_en_") && x.Contains(".translated_to_uk."));
                var enPl = sentenceFiles.Single(x => x.Contains("_en_") && x.Contains(".translated_to_pl."));

                var ukEn = sentenceFiles.Single(x => x.Contains("_uk_") && x.Contains(".translated_to_en."));
                var ukPl = sentenceFiles.Single(x => x.Contains("_uk_") && x.Contains(".translated_to_pl."));
                var plEn = sentenceFiles.Single(x => x.Contains("_pl_") && x.Contains(".translated_to_en."));

                var discoveredBook =
                    new SingleBookMetadata(subdirectory, audioFiles, en, enUk, uk, ukEn, enPl, pl, plEn, ukPl);
                books.Add(discoveredBook);
            }
        }

        return books;
    }
}
