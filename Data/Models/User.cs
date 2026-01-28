using System;
using System.Collections.Generic;
using System.Text;

namespace JournalPersonalApp.Data.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public int pin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
