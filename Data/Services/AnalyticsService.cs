using JournalPersonalApp.Data.Abstractions;
using JournalPersonalApp.Data.Models;
using JournalPersonalApp.Data.Utils;
using Microsoft.EntityFrameworkCore;

namespace JournalPersonalApp.Data.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly DBcontext _context;

    public AnalyticsService(DBcontext context)
    {
        _context = context;
    }

    // Streak Analytics
    public async Task<int> GetCurrentStreakAsync()
    {
        try
        {
            var entries = await _context.JournalEntries
                .OrderByDescending(j => j.EntryDate)
                .Select(j => j.EntryDate.Date)
                .ToListAsync();

            if (!entries.Any())
                return 0;

            int streak = 1;
            var today = DateTime.UtcNow.Date;

            if (entries.First() != today)
                return 0;

            for (int i = 0; i < entries.Count - 1; i++)
            {
                if (entries[i].AddDays(-1) == entries[i + 1])
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating current streak.", ex);
        }
    }

    public async Task<int> GetLongestStreakAsync()
    {
        try
        {
            var entries = await _context.JournalEntries
                .OrderBy(j => j.EntryDate)
                .Select(j => j.EntryDate.Date)
                .ToListAsync();

            if (!entries.Any())
                return 0;

            int longestStreak = 1;
            int currentStreak = 1;

            for (int i = 0; i < entries.Count - 1; i++)
            {
                if (entries[i].AddDays(1) == entries[i + 1])
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return longestStreak;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating longest streak.", ex);
        }
    }

    public async Task<List<DateTime>> GetMissedDaysAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date;

            var entryDates = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate <= end)
                .Select(j => j.EntryDate.Date)
                .Distinct()
                .ToListAsync();

            var missedDays = new List<DateTime>();
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                if (!entryDates.Contains(date))
                {
                    missedDays.Add(date);
                }
            }

            return missedDays;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating missed days.", ex);
        }
    }

    // Mood Analytics
    public async Task<Dictionary<string, int>> GetMoodDistributionAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var entries = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .Select(j => new { j.PrimaryMood, j.SecondaryMood1, j.SecondaryMood2 })
                .ToListAsync();

            var distribution = new Dictionary<string, int>
            {
                { "Positive", 0 },
                { "Neutral", 0 },
                { "Negative", 0 }
            };

            foreach (var entry in entries)
            {
                distribution[GetMoodCategory(entry.PrimaryMood)]++;

                if (entry.SecondaryMood1.HasValue)
                    distribution[GetMoodCategory(entry.SecondaryMood1.Value)]++;

                if (entry.SecondaryMood2.HasValue)
                    distribution[GetMoodCategory(entry.SecondaryMood2.Value)]++;
            }

            return distribution;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating mood distribution.", ex);
        }
    }

    public async Task<MoodCategory> GetMostFrequentMoodAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var moodCounts = new Dictionary<MoodCategory, int>();

            var entries = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .Select(j => new { j.PrimaryMood, j.SecondaryMood1, j.SecondaryMood2 })
                .ToListAsync();

            foreach (var entry in entries)
            {
                IncrementMoodCount(moodCounts, entry.PrimaryMood);

                if (entry.SecondaryMood1.HasValue)
                    IncrementMoodCount(moodCounts, entry.SecondaryMood1.Value);

                if (entry.SecondaryMood2.HasValue)
                    IncrementMoodCount(moodCounts, entry.SecondaryMood2.Value);
            }

            return moodCounts.Any() ? moodCounts.OrderByDescending(x => x.Value).First().Key : MoodCategory.Calm;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error getting most frequent mood.", ex);
        }
    }

    public async Task<Dictionary<MoodCategory, double>> GetMoodPercentagesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var moodCounts = new Dictionary<MoodCategory, int>();

            var entries = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .Select(j => new { j.PrimaryMood, j.SecondaryMood1, j.SecondaryMood2 })
                .ToListAsync();

            int totalMoods = 0;

            foreach (var entry in entries)
            {
                IncrementMoodCount(moodCounts, entry.PrimaryMood);
                totalMoods++;

                if (entry.SecondaryMood1.HasValue)
                {
                    IncrementMoodCount(moodCounts, entry.SecondaryMood1.Value);
                    totalMoods++;
                }

                if (entry.SecondaryMood2.HasValue)
                {
                    IncrementMoodCount(moodCounts, entry.SecondaryMood2.Value);
                    totalMoods++;
                }
            }

            var percentages = new Dictionary<MoodCategory, double>();

            if (totalMoods > 0)
            {
                foreach (var mood in moodCounts)
                {
                    percentages[mood.Key] = Math.Round((double)mood.Value / totalMoods * 100, 2);
                }
            }

            return percentages;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating mood percentages.", ex);
        }
    }

    // Tag Analytics
    public async Task<Dictionary<string, int>> GetMostUsedTagsAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var tagCounts = await _context.JournalEntryTags
                .Where(jet => jet.JournalEntry.EntryDate >= start && jet.JournalEntry.EntryDate < end)
                .GroupBy(jet => jet.Tag.Name)
                .Select(g => new { TagName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToDictionaryAsync(x => x.TagName, x => x.Count);

            return tagCounts;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error getting most used tags.", ex);
        }
    }

    public async Task<Dictionary<string, double>> GetTagPercentagesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var totalEntries = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .CountAsync();

            if (totalEntries == 0)
                return new Dictionary<string, double>();

            var tagCounts = await _context.JournalEntryTags
                .Where(jet => jet.JournalEntry.EntryDate >= start && jet.JournalEntry.EntryDate < end)
                .GroupBy(jet => jet.Tag.Name)
                .Select(g => new { TagName = g.Key, Count = g.Count() })
                .ToListAsync();

            var percentages = tagCounts
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionary(
                    x => x.TagName,
                    x => Math.Round((double)x.Count / totalEntries * 100, 2)
                );

            return percentages;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating tag percentages.", ex);
        }
    }

    // Entry Analytics
    public async Task<int> GetTotalEntriesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            return await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error getting total entries.", ex);
        }
    }

    public async Task<double> GetAverageWordCountAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var entries = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .Select(j => j.Description)
                .ToListAsync();

            if (!entries.Any())
                return 0;

            var wordCounts = entries.Select(desc => CountWords(desc));
            return Math.Round(wordCounts.Average(), 2);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating average word count.", ex);
        }
    }

    public async Task<Dictionary<DateTime, double>> GetWordCountTrendsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var entries = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .OrderBy(j => j.EntryDate)
                .Select(j => new { j.EntryDate, j.Description })
                .ToListAsync();

            var trends = entries
                .GroupBy(e => e.EntryDate.Date)
                .ToDictionary(
                    g => g.Key,
                    g => Math.Round(g.Average(e => CountWords(e.Description)), 2)
                );

            return trends;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating word count trends.", ex);
        }
    }

    public async Task<Dictionary<DateTime, int>> GetEntriesPerDayAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var start = startDate.Date;
            var end = endDate.Date.AddDays(1);

            var entriesPerDay = await _context.JournalEntries
                .Where(j => j.EntryDate >= start && j.EntryDate < end)
                .GroupBy(j => j.EntryDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            return entriesPerDay;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error getting entries per day.", ex);
        }
    }

    // Helper Methods
    private string GetMoodCategory(MoodCategory mood)
    {
        return mood switch
        {
            MoodCategory.Happy or MoodCategory.Excited or MoodCategory.Relaxed
                or MoodCategory.Grateful or MoodCategory.Confident => "Positive",
            MoodCategory.Sad or MoodCategory.Angry or MoodCategory.Stressed
                or MoodCategory.Lonely or MoodCategory.Anxious => "Negative",
            _ => "Neutral"
        };
    }

    private void IncrementMoodCount(Dictionary<MoodCategory, int> counts, MoodCategory mood)
    {
        if (counts.ContainsKey(mood))
            counts[mood]++;
        else
            counts[mood] = 1;
    }

    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        // Strip HTML tags
        var plainText = System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);

        // Count words
        return plainText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
