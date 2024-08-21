using BookToAnki.Models;
using BookToAnki.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookToAnki.NotePropertiesDatabase;

public record AlreadyRatedCard(PrefKey PrefKey, Rating Rating);

public class NoteProperties
{
    private readonly string _databaseFileName;

    private ILookup<PrefKey, NotePropertiesEntity>? _cacheUntilFirstWrite;

    public NoteProperties(string databaseFileName, bool ensureCreated = false)
    {
        _databaseFileName = databaseFileName;

        using var dbContext = new NoteContext(_databaseFileName, ensureCreated);
        _cacheUntilFirstWrite = dbContext.Notes.AsNoTracking().ToLookup(x => new PrefKey(x.Word, x.Sentence));
    }

    public Rating? GetNoteRating(PrefKey key) => GetEnum<Rating>(key, "Rating");
    public void SetNoteRating(PrefKey key, Rating? value) => SetEnum(key, "Rating", value);

    public string? GetWordNominativeOriginal(PrefKey key) => GetString(key, "WordNominativeOriginal");
    public void SetWordNominativeOriginal(PrefKey key, string? value) => SetString(key, "WordNominativeOriginal", value, true);

    public string? GetWordNominativePl(PrefKey key) => GetString(key, "WordNominativePl");
    public void SetWordNominativePl(PrefKey key, string? value) => SetString(key, "WordNominativePl", value, true);

    public string? GetWordNominativeEn(PrefKey key) => GetString(key, "WordNominativeEn");
    public void SetWordNominativeEn(PrefKey key, string? value) => SetString(key, "WordNominativeEn", value, true);

    public string? GetWordExplanationPl(PrefKey key) => GetString(key, "WordExplanationPl");
    public void SetWordExplanationPl(PrefKey key, string? value) => SetString(key, "WordExplanationPl", value, true);

    public string? GetWordExplanationEn(PrefKey key) => GetString(key, "WordExplanationEn");
    public void SetWordExplanationEn(PrefKey key, string? value) => SetString(key, "WordExplanationEn", value, true);

    public string? GetSentenceTranslationPl(PrefKey key) => GetString(key, "SentenceTranslationPl");
    public void SetSentenceTranslationPl(PrefKey key, string? value) => SetString(key, "SentenceTranslationPl", value);

    public string? GetSentenceTranslationEn(PrefKey key) => GetString(key, "SentenceTranslationEn");
    public void SetSentenceTranslationEn(PrefKey key, string? value) => SetString(key, "SentenceTranslationEn", value);

    public AudioSample? GetAudioSample(PrefKey key) => GetSerializedJson<AudioSample>(key, "AudioSample");
    public void SetAudioSample(PrefKey key, AudioSample? value) => SetSerializedJson(key, "AudioSample", value);

    public string? GetImageFileName(PrefKey key) => GetString(key, "ImageFileName");
    public void SetImageFileName(PrefKey key, string? value) => SetString(key, "ImageFileName", value);

    public string? GetWordNominativeOriginalGpt4(PrefKey key) => GetString(key, "WordNominativeOriginalGpt4");
    public void SetWordNominativeOriginalGpt4(PrefKey key, string? value) => SetString(key, "WordNominativeOriginalGpt4", value, true);

    public string? GetWordNominativePlGpt4(PrefKey key) => GetString(key, "WordNominativePlGpt4");
    public void SetWordNominativePlGpt4(PrefKey key, string? value) => SetString(key, "WordNominativePlGpt4", value, true);

    public string? GetWordNominativeEnGpt4(PrefKey key) => GetString(key, "WordNominativeEnGpt4");
    public void SetWordNominativeEnGpt4(PrefKey key, string? value) => SetString(key, "WordNominativeEnGpt4", value, true);

    public string? GetWordExplanationPlGpt4(PrefKey key) => GetString(key, "WordExplanationPlGpt4");
    public void SetWordExplanationPlGpt4(PrefKey key, string? value) => SetString(key, "WordExplanationPlGpt4", value, true);

    public string? GetWordExplanationEnGpt4(PrefKey key) => GetString(key, "WordExplanationEnGpt4");
    public void SetWordExplanationEnGpt4(PrefKey key, string? value) => SetString(key, "WordExplanationEnGpt4", value, true);

    public void SetAudioSampleShift(PrefKey prefKey, AudioShift audioShift) => SetSerializedJson(prefKey, "AudioSampleShift", audioShift);
    public AudioShift? GetAudioSampleShift(PrefKey key) => GetSerializedJson<AudioShift>(key, "AudioSampleShift");

    public List<AlreadyRatedCard> GetAlreadyRated()
    {
        using var dbContext = new NoteContext(_databaseFileName);
        var ratedEntities = dbContext.Notes
            .Where(x => x.PropertyName == "Rating" && x.PropertyValue != null)
            .Select(x => new AlreadyRatedCard(new PrefKey(x.Word, x.Sentence), Enum.Parse<Rating>(x.PropertyValue!)))
            .AsNoTracking()
            .ToList();

        return ratedEntities;
    }

    private NotePropertiesEntity GetExistingEntityOrDefault(NoteContext dbContext, PrefKey notePreferencesKey, string propertyName, bool trackInEf)
    {
        var source = trackInEf ? dbContext.Notes : dbContext.Notes.AsNoTracking();

        var existingEntity = source
            .FirstOrDefault(n =>
            n.Word == notePreferencesKey.Word && n.Sentence == notePreferencesKey.SentenceExample && n.PropertyName == propertyName);

        if (existingEntity is not null)
        {
            return existingEntity;
        }

        var newEntity = new NotePropertiesEntity(notePreferencesKey.Word, notePreferencesKey.SentenceExample, propertyName);
        if (trackInEf)
            dbContext.Notes.Add(newEntity);

        return newEntity;
    }

    private TEnum? GetEnum<TEnum>(PrefKey notePreferencesKey, string propertyName) where TEnum : struct
    {
        using var dbContext = new NoteContext(_databaseFileName);
        var entity = GetExistingEntityOrDefault(dbContext, notePreferencesKey, propertyName, false);
        if (Enum.TryParse(entity.PropertyValue, out TEnum result)) return result;
        return null;
    }

    private void SetEnum<TEnum>(PrefKey notePreferencesKey, string propertyName, TEnum? value, bool allowWordScope = false) where TEnum : struct
    {
        _cacheUntilFirstWrite = null;
        if (!allowWordScope && notePreferencesKey.SentenceExample == "*")
            throw new InvalidOperationException($"You cannot use a wildcard sentence scope for the '{propertyName}' property.");

        using var dbContext = new NoteContext(_databaseFileName);
        var entity = GetExistingEntityOrDefault(dbContext, notePreferencesKey, propertyName, true);
        entity.PropertyValue = value?.ToString();
        dbContext.SaveChanges();
    }

    private TObject? GetSerializedJson<TObject>(PrefKey notePreferencesKey, string propertyName) where TObject : class
    {
        using var dbContext = new NoteContext(_databaseFileName);
        var entity = GetExistingEntityOrDefault(dbContext, notePreferencesKey, propertyName, false);
        return entity.PropertyValue is null ? null : JsonSerializer.Deserialize<TObject>(entity.PropertyValue);
    }

    private void SetSerializedJson<TObject>(PrefKey notePreferencesKey, string propertyName, TObject? value, bool allowWordScope = false)
    {
        _cacheUntilFirstWrite = null;
        if (!allowWordScope && notePreferencesKey.SentenceExample == "*")
            throw new InvalidOperationException($"You cannot use a wildcard sentence scope for the '{propertyName}' property.");

        using var dbContext = new NoteContext(_databaseFileName);
        var entity = GetExistingEntityOrDefault(dbContext, notePreferencesKey, propertyName, true);
        entity.PropertyValue = JsonSerializer.Serialize(value);
        dbContext.SaveChanges();
    }

    private string? GetString(PrefKey notePreferencesKey, string propertyName)
    {
        if (_cacheUntilFirstWrite is not null)
            return _cacheUntilFirstWrite[notePreferencesKey].FirstOrDefault(z => z.PropertyName == propertyName)?.PropertyValue;

        using var dbContext = new NoteContext(_databaseFileName);
        return GetExistingEntityOrDefault(dbContext, notePreferencesKey, propertyName, false).PropertyValue;
    }

    private void SetString(PrefKey notePreferencesKey, string propertyName, string? value, bool allowWordScope = false)
    {
        _cacheUntilFirstWrite = null;
        if (!allowWordScope && notePreferencesKey.SentenceExample == "*")
            throw new InvalidOperationException($"You cannot use a wildcard sentence scope for the '{propertyName}' property.");

        using var dbContext = new NoteContext(_databaseFileName);
        var entity = GetExistingEntityOrDefault(dbContext, notePreferencesKey, propertyName, true);
        entity.PropertyValue = value;
        dbContext.SaveChanges();
    }

    internal List<PrefKey> GetAlreadyExplainedByGpt4()
    {
        using var dbContext = new NoteContext(_databaseFileName);
        var alreadyExplained = dbContext.Notes
            .Where(x => x.PropertyName == "WordNominativeOriginalGpt4")
            .Select(x => new PrefKey(x.Word, x.Sentence))
            .ToList();
        return alreadyExplained;
    }


}
