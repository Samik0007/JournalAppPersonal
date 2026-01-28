using JournalPersonalApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace JournalPersonalApp.Data.Utils
{
    public class DBcontext: DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Journalentry> JournalEntries { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<JournalEntryTag> JournalEntryTags { get; set; } = null!;


        public DBcontext(DbContextOptions<DBcontext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.pin).IsRequired().HasMaxLength(4);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
            modelBuilder.Entity<Journalentry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.EntryDate).IsRequired();
                entity.Property(e => e.PrimaryMood).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                // Index for daily entry lookup
                entity.HasIndex(e => e.EntryDate).IsUnique();

                // Navigation to tags
                entity.HasMany(e => e.JournalEntryTags)
                    .WithOne(et => et.JournalEntry)
                    .HasForeignKey(et => et.JournalEntryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsPrebuilt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Seed prebuilt tags
                var prebuiltTags = new[]
                {
                    "Work", "Career", "Studies", "Family", "Friends", "Relationships",
                    "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies", "Travel",
                    "Nature", "Finance", "Spirituality", "Birthday", "Holiday", "Vacation",
                    "Celebration", "Exercise", "Reading", "Writing", "Cooking", "Meditation",
                    "Yoga", "Music", "Shopping", "Parenting", "Projects", "Planning", "Reflection"
                };

                var tagList = new List<Tag>();
                foreach (var tagName in prebuiltTags)
                {
                    tagList.Add(new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = tagName,
                        IsPrebuilt = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                entity.HasData(tagList);
            });

            modelBuilder.Entity<JournalEntryTag>(entity =>
            {
                entity.HasKey(e => new { e.JournalEntryId, e.TagId });

                entity.HasOne(e => e.JournalEntry)
                    .WithMany(j => j.JournalEntryTags)
                    .HasForeignKey(e => e.JournalEntryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.JournalEntryTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
