using Anki.NET.Helpers;
using Anki.NET.Models;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.IO.Compression;
using System.Text.Json;

namespace Anki.NET;

public class AnkiDeck
{
    private readonly Dictionary<string, string> _registeredMediaFiles = new();
    private readonly string _css = GeneralHelper.ReadResource("Anki.NET.AnkiData.CardStyle.css");

    private readonly List<AnkiItem> _ankiItems = new();
    private string _collectionFilePath;

    private SqliteConnection _conn;

    private readonly AnkiDeckModel _ankiDeckModel;
    private readonly string _temporaryDeckPath;
    private readonly ImageProcessor _imageProcessor = new();

    /// <summary>
    ///     Creates a AnkiDeck object
    /// </summary>
    public AnkiDeck(AnkiDeckModel ankiDeckModel)
    {
        _ankiDeckModel = ankiDeckModel;
        // unique temporary directory in %TEMP%
        _temporaryDeckPath = Path.Combine(Path.GetTempPath(), "Anki.NET", Path.GetRandomFileName());

        if (Directory.Exists(_temporaryDeckPath) == false)
            Directory.CreateDirectory(_temporaryDeckPath);
    }

    public string CreateApkgFile(string folderPath)
    {
        CreateDbFile();
        CreateMediaFile();
        ExecuteSqLiteCommands();
        var packageFileName = CreateZipFile(folderPath);
        return packageFileName;
    }

    /// <summary>
    ///     Creates an AnkiItem and add it to the AnkiDeck object
    /// </summary>
    public void AddItem(params string[] properties)
    {
        var item = new AnkiItem(_ankiDeckModel.FieldList, properties);
        _ankiItems.Add(item);
    }

    private void CreateDbFile(string name = "collection.db")
    {
        _collectionFilePath = Path.Combine(_temporaryDeckPath, name);

        if (File.Exists(_collectionFilePath))
            File.Delete(_collectionFilePath);

        File.Create(_collectionFilePath).Close();
    }


    private string CreateZipFile(string path)
    {
        Directory.CreateDirectory(path);
        var anki2FilePath = Path.Combine(_temporaryDeckPath, "collection.anki2");
        var mediaFilePath = Path.Combine(_temporaryDeckPath, "media");

        File.Move(_collectionFilePath, anki2FilePath);
        var zipPath = Path.Combine(path, _ankiDeckModel.ModelName + ".apkg");

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(_temporaryDeckPath, zipPath);

        File.Delete(anki2FilePath);
        File.Delete(mediaFilePath);

        var i = 0;
        var currentFile = Path.Combine(_temporaryDeckPath, i.ToString());

        while (File.Exists(currentFile))
        {
            File.Delete(currentFile);
            ++i;
            currentFile = Path.Combine(_temporaryDeckPath, i.ToString());
        }
        return zipPath;
    }

    private string CreateCol()
    {
        _ankiItems.ForEach(x => x.Mid = _ankiDeckModel.ModelId);
        var collection = new Collection(_ankiDeckModel, _css);

        SqLiteHelper.ExecuteSqLiteCommand(_conn, collection.Query);

        return collection.DeckId;
    }

    private void CreateNotesAndCards(string deckId, AnkiDeck ankiDeck = null)
    {
        var currentAnki = ankiDeck ?? this;

        foreach (var ankiItem in currentAnki._ankiItems)
        {
            var note = new Note(_ankiDeckModel, ankiItem);

            SqLiteHelper.ExecuteSqLiteCommand(currentAnki._conn, note.Query);

            var card = new Card(note, deckId);

            SqLiteHelper.ExecuteSqLiteCommand(currentAnki._conn, card.Query);
        }
    }

    private void ExecuteSqLiteCommands(AnkiDeck ankiDeck = null)
    {
        try
        {
            Batteries.Init();

            _conn = new SqliteConnection("Data Source=" + _collectionFilePath + ";");
            _conn.Open();

            var column = GeneralHelper.ReadResource("Anki.NET.SqLiteCommands.ColumnTable.sql");
            var notes = GeneralHelper.ReadResource("Anki.NET.SqLiteCommands.NotesTable.sql");
            var cards = GeneralHelper.ReadResource("Anki.NET.SqLiteCommands.CardsTable.sql");
            var revLogs = GeneralHelper.ReadResource("Anki.NET.SqLiteCommands.RevLogTable.sql");
            var graves = GeneralHelper.ReadResource("Anki.NET.SqLiteCommands.GravesTable.sql");
            var indexes = GeneralHelper.ReadResource("Anki.NET.SqLiteCommands.Indexes.sql");

            SqLiteHelper.ExecuteSqLiteCommand(_conn, column);
            SqLiteHelper.ExecuteSqLiteCommand(_conn, notes);
            SqLiteHelper.ExecuteSqLiteCommand(_conn, cards);
            SqLiteHelper.ExecuteSqLiteCommand(_conn, revLogs);
            SqLiteHelper.ExecuteSqLiteCommand(_conn, graves);
            SqLiteHelper.ExecuteSqLiteCommand(_conn, indexes);

            var deckId = CreateCol();
            CreateNotesAndCards(deckId, ankiDeck);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        finally
        {
            _conn.Close();
            _conn.Dispose();
            SqliteConnection.ClearAllPools();
        }
    }

    private void CreateMediaFile()
    {
        // 'media' is a JSON file that maps media files like `0` to media file names like `abc.mp3`.
        var mediaFilePath = Path.Combine(_temporaryDeckPath, "media");

        if (File.Exists(mediaFilePath))
            File.Delete(mediaFilePath);

        File.WriteAllText(mediaFilePath, JsonSerializer.Serialize(_registeredMediaFiles));
    }

    [Obsolete("Use more specific methods to register media files")]
    public void RegisterMediaFile(string audioFilePath, string audioFileName)
    {
        var index = _registeredMediaFiles.Count;
        var filenameInZipArchive = $"{index}";
        var mediaFilePath = Path.Combine(_temporaryDeckPath, filenameInZipArchive);

        // make sure that the file lands in the zip archive
        File.Copy(audioFilePath, mediaFilePath, true);
        // add it to the index, too
        _registeredMediaFiles.Add(filenameInZipArchive, audioFileName);
    }

    public string RegisterAudioFile(string audioFilePath)
    {
        var originalFileNameWithExtension = Path.GetFileName(audioFilePath);
        var index = _registeredMediaFiles.Count;
        var filenameInZipArchive = $"{index}"; // ordinal number, no extension
        var fileNameInUserCollection = $"{_ankiDeckModel.ShortUniquePrefixForMediaFiles}-{originalFileNameWithExtension}";

        if (_registeredMediaFiles.ContainsValue(fileNameInUserCollection))
        {
            throw new ArgumentException($"Cannot register file named {originalFileNameWithExtension} - a media file wih identical derived name ({fileNameInUserCollection}) was already registered");
        }

        // make sure that the file lands in the zip archive
        var mediaFilePath = Path.Combine(_temporaryDeckPath, filenameInZipArchive);
        File.Copy(audioFilePath, mediaFilePath, true);

        // add it to the index, too
        _registeredMediaFiles.Add(filenameInZipArchive, fileNameInUserCollection);

        return fileNameInUserCollection;
    }

    public string RegisterImageFile(string imageFilePathRaw)
    {
        // scale down and compress as webp if needed
        var webpFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(imageFilePathRaw)}.webp");
        _imageProcessor.ConvertToWebpAndResize(imageFilePathRaw, webpFilePath);

        var originalFileNameWithExtension = Path.GetFileName(webpFilePath);
        var dotAndExtension = Path.GetExtension(webpFilePath);
        var hash = originalFileNameWithExtension.GetHashCode();

        var index = _registeredMediaFiles.Count;
        var filenameInZipArchive = $"{index}"; // ordinal number, no extension
        var fileNameInUserCollection = $"{_ankiDeckModel.ShortUniquePrefixForMediaFiles}-{hash}{dotAndExtension}";


        if (_registeredMediaFiles.ContainsValue(fileNameInUserCollection))
        {
            var alreadyRegisteredFile = _registeredMediaFiles.Single(x => x.Value == fileNameInUserCollection);
            var alreadyRegisteredFileFullPath = Path.Combine(_temporaryDeckPath, alreadyRegisteredFile.Key);
            if (new FileInfo(alreadyRegisteredFileFullPath).Length == new FileInfo(webpFilePath).Length)
            {
                // let's assume it's the same file content
                return alreadyRegisteredFile.Value;
            }

            throw new ArgumentException($"Cannot register file named {originalFileNameWithExtension} - a media file wih identical derived name ({fileNameInUserCollection}) was already registered");
        }

        // make sure that the file lands in the zip archive
        var mediaFilePath = Path.Combine(_temporaryDeckPath, filenameInZipArchive);
        File.Copy(webpFilePath, mediaFilePath, true);
        File.Delete(webpFilePath); // from temporary folder

        // add it to the index, too
        _registeredMediaFiles.Add(filenameInZipArchive, fileNameInUserCollection);

        return fileNameInUserCollection;
    }
}
