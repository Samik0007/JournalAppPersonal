using System;
using System.Collections.Generic;
using System.Text;

namespace JournalPersonalApp.Data.Models
{
    public class Journalentry
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public MoodCategory PrimaryMood { get; set; }
        public MoodCategory? SecondaryMood1 { get; set; }
        public MoodCategory? SecondaryMood2 { get; set; }

        // Category
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<JournalEntryTag> JournalEntryTags { get; set; } = new List<JournalEntryTag>();


    }
}
