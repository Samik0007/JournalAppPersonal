using System;
using System.Collections.Generic;
using System.Text;

namespace JournalPersonalApp.Data.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPrebuilt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<JournalEntryTag> JournalEntryTags { get; set; } = new List<JournalEntryTag>();

    }
}
