using JournalPersonalApp.Data.Abstractions;
using JournalPersonalApp.Data.Models;
using JournalPersonalApp.Data.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JournalPersonalApp.Data.Services
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly DBcontext _context;

        public JournalEntryService(DBcontext context)
        {
            _context = context;
        }
        /// operation to create a new journal entry
        public async Task<Journalentry> CreateAsync(
           string title,
           string description,
           MoodCategory primaryMood,
           string category,
           MoodCategory? secondaryMood1 = null,
           MoodCategory? secondaryMood2 = null,
           List<string>? tagNames = null,
           DateTime? entryDate = null)
        {
            try
            {
                var date = (entryDate ?? DateTime.Now).Date;
                var now = DateTime.Now;

                var existingEntry = await _context.JournalEntries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(j => j.EntryDate == date);

                if (existingEntry != null)
                    throw new InvalidOperationException($"An entry already exists for date {date:yyyy-MM-dd}.");

                var secondary = new[] { secondaryMood1, secondaryMood2 }
                    .Where(m => m.HasValue)
                    .Select(m => m!.Value)
                    .Distinct()
                    .Take(2)
                    .ToList();

                var normalizedTags = (tagNames ?? new List<string>())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var entry = new Journalentry
                {
                    Id = Guid.NewGuid(),
                    Title = title ?? string.Empty,
                    Description = description ?? string.Empty,
                    EntryDate = date,
                    PrimaryMood = primaryMood,
                    SecondaryMood1 = secondary.ElementAtOrDefault(0),
                    SecondaryMood2 = secondary.ElementAtOrDefault(1),
                    Category = category,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.JournalEntries.Add(entry);
                await _context.SaveChangesAsync();

                if (normalizedTags.Any())
                {
                    await AddTagsToEntryAsync(entry.Id, normalizedTags);
                }

                return entry;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating journal entry.", ex);
            }
        }

        /// Id for the journal entry
        public async Task<Journalentry?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .FirstOrDefaultAsync(j => j.Id == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving journal entry.", ex);
            }
        }

        /// journal entry for today
        public async Task<Journalentry?> GetTodayEntryAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .FirstOrDefaultAsync(j => j.EntryDate == today);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving today's entry.", ex);
            }
        }

        /// Entry for specific date
        public async Task<Journalentry?> GetEntryByDateAsync(DateTime date)
        {
            try
            {
                var targetDate = date.Date;
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .FirstOrDefaultAsync(j => j.EntryDate == targetDate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entry by date.", ex);
            }
        }

        ///List opeartion done here 
        public async Task<List<Journalentry>> GetAllAsync()
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving journal entries.", ex);
            }
        }

        ///Update operation
        public async Task<Journalentry?> UpdateAsync(
            Guid id,
            string title,
            string description,
            MoodCategory primaryMood,
            string category,
            MoodCategory? secondaryMood1 = null,
            MoodCategory? secondaryMood2 = null,
            List<string>? tagNames = null)
        {
            try
            {
                var entry = await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .FirstOrDefaultAsync(j => j.Id == id);

                if (entry == null)
                    return null;

                entry.Title = title;
                entry.Description = description;
                entry.PrimaryMood = primaryMood;
                entry.SecondaryMood1 = secondaryMood1;
                entry.SecondaryMood2 = secondaryMood2;
                entry.Category = category;
                entry.UpdatedAt = DateTime.UtcNow;

                _context.JournalEntries.Update(entry);
                await _context.SaveChangesAsync();

                // Update tags if provided
                if (tagNames != null)
                {
                    await RemoveAllTagsFromEntryAsync(entry.Id);
                    if (tagNames.Any())
                    {
                        await AddTagsToEntryAsync(entry.Id, tagNames);
                    }
                }

                return entry;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating journal entry.", ex);
            }
        }

        /// Delete operation for journal entry
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var entry = await _context.JournalEntries.FirstOrDefaultAsync(j => j.Id == id);

                if (entry == null)
                    return false;

                _context.JournalEntries.Remove(entry);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error deleting journal entry.", ex);
            }
        }

        /// Searches journal entries by content
        public async Task<List<Journalentry>> SearchByContentAsync(string searchTerm)
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => j.Title.Contains(searchTerm) || j.Description.Contains(searchTerm))
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error searching journal entries.", ex);
            }
        }

        /// Searches journal entries by date range
        public async Task<List<Journalentry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var start = startDate.Date;
                var end = endDate.Date.AddDays(1);

                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => j.EntryDate >= start && j.EntryDate < end)
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entries by date range.", ex);
            }
        }

        /// Filters entries by mood
        public async Task<List<Journalentry>> GetEntriesByMoodAsync(MoodCategory mood)
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => j.PrimaryMood == mood || j.SecondaryMood1 == mood || j.SecondaryMood2 == mood)
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entries by mood.", ex);
            }
        }

        /// filters entries by mood and date range
        public async Task<List<Journalentry>> GetEntriesByMoodAndDateAsync(MoodCategory mood, DateTime startDate, DateTime endDate)
        {
            try
            {
                var start = startDate.Date;
                var end = endDate.Date.AddDays(1);

                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => (j.PrimaryMood == mood || j.SecondaryMood1 == mood || j.SecondaryMood2 == mood)
                        && j.EntryDate >= start && j.EntryDate < end)
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entries by mood and date.", ex);
            }
        }

        /// Filter by tag 
        public async Task<List<Journalentry>> GetEntriesByTagAsync(string tagName)
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => j.JournalEntryTags.Any(et => et.Tag.Name == tagName))
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entries by tag.", ex);
            }
        }

        /// filter by multiple tags
        public async Task<List<Journalentry>> GetEntriesByMultipleTagsAsync(List<string> tagNames)
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => tagNames.All(t => j.JournalEntryTags.Any(et => et.Tag.Name == t)))
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entries by tags.", ex);
            }
        }

        /// Filter by category
        public async Task<List<Journalentry>> GetEntriesByCategoryAsync(string category)
        {
            try
            {
                return await _context.JournalEntries
                    .Include(j => j.JournalEntryTags)
                    .ThenInclude(et => et.Tag)
                    .Where(j => j.Category == category)
                    .OrderByDescending(j => j.EntryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving entries by category.", ex);
            }
        }

        /// prebuilt tags retrieval
        public async Task<List<Tag>> GetPrebuiltTagsAsync()
        {
            try
            {
                return await _context.Tags
                    .Where(t => t.IsPrebuilt)
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving prebuilt tags.", ex);
            }
        }

        /// custom tags retrieval
        public async Task<List<Tag>> GetCustomTagsAsync()
        {
            try
            {
                return await _context.Tags
                    .Where(t => !t.IsPrebuilt)
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving custom tags.", ex);
            }
        }

        /// Creating new tag 
        public async Task<Tag> CreateCustomTagAsync(string tagName)
        {
            try
            {
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());

                if (existingTag != null)
                    throw new InvalidOperationException("Tag already exists.");

                var tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = tagName,
                    IsPrebuilt = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                return tag;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating custom tag.", ex);
            }
        }

        /// Adding tag to journal entry
        private async Task AddTagsToEntryAsync(Guid entryId, List<string> tagNames)
        {
            try
            {
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name == tagName);

                    if (tag == null)
                    {
                        tag = await CreateCustomTagAsync(tagName);
                    }

                    var entryTag = new JournalEntryTag
                    {
                        JournalEntryId = entryId,
                        TagId = tag.Id
                    };

                    _context.JournalEntryTags.Add(entryTag);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error adding tags to entry.", ex);
            }
        }

        /// Removing all tags from journal entry
        private async Task RemoveAllTagsFromEntryAsync(Guid entryId)
        {
            try
            {
                var entryTags = await _context.JournalEntryTags
                    .Where(et => et.JournalEntryId == entryId)
                    .ToListAsync();

                _context.JournalEntryTags.RemoveRange(entryTags);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error removing tags from entry.", ex);
            }
        }

        /// get daily streak
        public async Task<int> GetDailyStreakAsync()
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

                // Check if today has an entry
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
                throw new InvalidOperationException("Error calculating daily streak.", ex);
            }
        }

        /// mood analytics over date range
        public async Task<Dictionary<MoodCategory, int>> GetMoodAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var start = startDate.Date;
                var end = endDate.Date.AddDays(1);

                var entries = await _context.JournalEntries
                    .Where(j => j.EntryDate >= start && j.EntryDate < end)
                    .Select(j => new { j.PrimaryMood, j.SecondaryMood1, j.SecondaryMood2 })
                    .ToListAsync();

                var moodCounts = new Dictionary<MoodCategory, int>();

                foreach (var entry in entries)
                {
                    if (moodCounts.ContainsKey(entry.PrimaryMood))
                        moodCounts[entry.PrimaryMood]++;
                    else
                        moodCounts[entry.PrimaryMood] = 1;

                    if (entry.SecondaryMood1.HasValue)
                    {
                        if (moodCounts.ContainsKey(entry.SecondaryMood1.Value))
                            moodCounts[entry.SecondaryMood1.Value]++;
                        else
                            moodCounts[entry.SecondaryMood1.Value] = 1;
                    }

                    if (entry.SecondaryMood2.HasValue)
                    {
                        if (moodCounts.ContainsKey(entry.SecondaryMood2.Value))
                            moodCounts[entry.SecondaryMood2.Value]++;
                        else
                            moodCounts[entry.SecondaryMood2.Value] = 1;
                    }
                }

                return moodCounts;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving mood analytics.", ex);
            }
        }
    }

}
