using JournalPersonalApp.Data.Models;

namespace JournalPersonalApp.Data.Abstractions;

public interface IAnalyticsService
{
    // Streak Analytics
    Task<int> GetCurrentStreakAsync();
    Task<int> GetLongestStreakAsync();
    Task<List<DateTime>> GetMissedDaysAsync(DateTime startDate, DateTime endDate);
    
    // Mood Analytics
    Task<Dictionary<string, int>> GetMoodDistributionAsync(DateTime startDate, DateTime endDate);
    Task<MoodCategory> GetMostFrequentMoodAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<MoodCategory, double>> GetMoodPercentagesAsync(DateTime startDate, DateTime endDate);
    
    // Tag Analytics
    Task<Dictionary<string, int>> GetMostUsedTagsAsync(DateTime startDate, DateTime endDate, int topCount = 10);
    Task<Dictionary<string, double>> GetTagPercentagesAsync(DateTime startDate, DateTime endDate);
    
    // Entry Analytics
    Task<int> GetTotalEntriesAsync(DateTime startDate, DateTime endDate);
    Task<double> GetAverageWordCountAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<DateTime, double>> GetWordCountTrendsAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<DateTime, int>> GetEntriesPerDayAsync(DateTime startDate, DateTime endDate);
}
