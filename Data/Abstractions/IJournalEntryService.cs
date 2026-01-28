using JournalPersonalApp.Data.Models;

namespace JournalPersonalApp.Data.Abstractions;

public interface IJournalEntryService
{
    Task<Journalentry> CreateAsync(
        string title,
        string description,
        MoodCategory primaryMood,
        string category,
        MoodCategory? secondaryMood1 = null,
        MoodCategory? secondaryMood2 = null,
        List<string>? tagNames = null,
        DateTime? entryDate = null);

    Task<Journalentry?> GetByIdAsync(Guid id);
    Task<Journalentry?> GetTodayEntryAsync();
    Task<Journalentry?> GetEntryByDateAsync(DateTime date);
    Task<List<Journalentry>> GetAllAsync();

    Task<Journalentry?> UpdateAsync(
        Guid id,
        string title,
        string description,
        MoodCategory primaryMood,
        string category,
        MoodCategory? secondaryMood1 = null,
        MoodCategory? secondaryMood2 = null,
        List<string>? tagNames = null);

    Task<bool> DeleteAsync(Guid id);

    Task<List<Journalentry>> SearchByContentAsync(string searchTerm);
    Task<List<Journalentry>> SearchByTitleAsync(string searchTerm);
    Task<List<Journalentry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Journalentry>> GetEntriesByMoodAsync(MoodCategory mood);
    Task<List<Journalentry>> GetEntriesByMoodAndDateAsync(MoodCategory mood, DateTime startDate, DateTime endDate);
    Task<List<Journalentry>> GetEntriesByTagAsync(string tagName);
    Task<List<Journalentry>> GetEntriesByMultipleTagsAsync(List<string> tagNames);
    Task<List<Journalentry>> GetEntriesByCategoryAsync(string category);

    Task<List<Tag>> GetPrebuiltTagsAsync();
    Task<List<Tag>> GetCustomTagsAsync();
    Task<Tag> CreateCustomTagAsync(string tagName);

    Task<int> GetDailyStreakAsync();
    Task<Dictionary<MoodCategory, int>> GetMoodAnalyticsAsync(DateTime startDate, DateTime endDate);
}
