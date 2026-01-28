using System;
using System.Collections.Generic;
using System.Text;

namespace JournalPersonalApp.Data.Models
{
    public class JournalEntryTag
    {
        public Guid JournalEntryId { get; set; }
        public Guid TagId { get; set; }

        // Navigation properties
        public Journalentry JournalEntry { get; set; } = null!;
        public Tag Tag { get; set; } = null!;

    }
}
