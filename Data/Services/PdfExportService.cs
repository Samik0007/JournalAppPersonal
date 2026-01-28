namespace JournalPersonalApp.Data.Services;

using JournalPersonalApp.Data.Abstractions;
using JournalPersonalApp.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

public sealed class PdfExportService : IPdfExportService
{
    private const string AssetName = "quest.pdf";

    public PdfExportService()
    {
        // Set QuestPDF license (Community license is free for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> ExportQuestPdfAsync(CancellationToken ct = default)
    {
        try
        {
            // Get the Downloads folder path
            string downloadsPath;
            
#if WINDOWS
            downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                "Downloads");
#elif ANDROID
            downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
                Android.OS.Environment.DirectoryDownloads)?.AbsolutePath 
                ?? Path.Combine(FileSystem.AppDataDirectory, "Downloads");
#elif IOS || MACCATALYST
            downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "..", "Downloads");
#else
            // Fallback to AppDataDirectory for other platforms
            downloadsPath = Path.Combine(FileSystem.AppDataDirectory, "Downloads");
#endif

            // Ensure the Downloads directory exists
            if (!Directory.Exists(downloadsPath))
            {
                Directory.CreateDirectory(downloadsPath);
            }

            // Create a unique filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"JournalExport_{timestamp}.pdf";
            var destinationPath = Path.Combine(downloadsPath, fileName);

            await using var src = await FileSystem.OpenAppPackageFileAsync(AssetName);
            await using var dst = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            await src.CopyToAsync(dst, ct);

            return destinationPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to export PDF to Downloads folder.", ex);
        }
    }

    public async Task<string> ExportJournalEntryAsync(Journalentry entry, CancellationToken ct = default)
    {
        try
        {
            // Get the Downloads folder path
            string downloadsPath = GetDownloadsPath();

            // Sanitize the title for use as filename
            var sanitizedTitle = SanitizeFileName(entry.Title);
            var fileName = $"{sanitizedTitle}.pdf";
            var destinationPath = Path.Combine(downloadsPath, fileName);

            // If file already exists, add timestamp
            if (File.Exists(destinationPath))
            {
                var timestamp = DateTime.Now.ToString("_yyyyMMdd_HHmmss");
                fileName = $"{sanitizedTitle}{timestamp}.pdf";
                destinationPath = Path.Combine(downloadsPath, fileName);
            }

            // Generate PDF
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                        page.Header()
                            .Text($"Journal Entry - {entry.EntryDate:MMMM dd, yyyy}")
                            .SemiBold().FontSize(10).FontColor(Colors.Grey.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(10);

                                // Title
                                x.Item().Text(entry.Title).SemiBold().FontSize(24);

                                // Date and Mood
                                x.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"Date: {entry.EntryDate:MMMM dd, yyyy}");
                                    row.RelativeItem().AlignRight().Text($"Mood: {GetMoodDisplayName(entry.PrimaryMood)}");
                                });

                                x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                                // Category
                                if (!string.IsNullOrEmpty(entry.Category))
                                {
                                    x.Item().Text($"Category: {entry.Category}").FontSize(11).Italic();
                                }

                                // Tags
                                if (entry.JournalEntryTags != null && entry.JournalEntryTags.Any())
                                {
                                    x.Item().Row(row =>
                                    {
                                        row.AutoItem().Text("Tags: ").FontSize(11);
                                        row.RelativeItem().Text(string.Join(", ", entry.JournalEntryTags.Select(t => t.Tag?.Name)))
                                            .FontSize(11).FontColor(Colors.Blue.Medium);
                                    });
                                }

                                x.Item().PaddingTop(10);

                                // Content
                                var plainText = StripHtml(entry.Description);
                                x.Item().Text(plainText).FontSize(12).LineHeight(1.5f);

                                // Secondary Moods
                                if (entry.SecondaryMood1.HasValue || entry.SecondaryMood2.HasValue)
                                {
                                    x.Item().PaddingTop(15);
                                    var secondaryMoods = new List<string>();
                                    if (entry.SecondaryMood1.HasValue)
                                        secondaryMoods.Add(entry.SecondaryMood1.Value.ToString());
                                    if (entry.SecondaryMood2.HasValue)
                                        secondaryMoods.Add(entry.SecondaryMood2.Value.ToString());
                                    
                                    x.Item().Text($"Secondary Moods: {string.Join(", ", secondaryMoods)}")
                                        .FontSize(10).FontColor(Colors.Grey.Medium);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Generated on ").FontSize(9).FontColor(Colors.Grey.Medium);
                                text.Span(DateTime.Now.ToString("MMMM dd, yyyy HH:mm")).SemiBold().FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                    });
                })
                .GeneratePdf(destinationPath);
            }, ct);

            return destinationPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to export journal entry '{entry.Title}' to PDF.", ex);
        }
    }

    public async Task<string> ExportAllJournalEntriesAsync(List<Journalentry> entries, CancellationToken ct = default)
    {
        try
        {
            // Get the Downloads folder path
            string downloadsPath = GetDownloadsPath();

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"AllJournalEntries_{timestamp}.pdf";
            var destinationPath = Path.Combine(downloadsPath, fileName);

            // Generate PDF
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                        page.Header()
                            .Text("My Journal Entries")
                            .SemiBold().FontSize(14).FontColor(Colors.Grey.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                foreach (var entry in entries.OrderByDescending(e => e.EntryDate))
                                {
                                    x.Item().Element(c => RenderJournalEntry(c, entry));
                                    x.Item().PaddingTop(20);
                                    x.Item().LineHorizontal(2).LineColor(Colors.Grey.Lighten1);
                                    x.Item().PaddingTop(20);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                                text.Span(" / ").FontSize(9).FontColor(Colors.Grey.Medium);
                                text.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                    });
                })
                .GeneratePdf(destinationPath);
            }, ct);

            return destinationPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to export all journal entries to PDF.", ex);
        }
    }

    private void RenderJournalEntry(IContainer container, Journalentry entry)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            // Title
            column.Item().Text(entry.Title).SemiBold().FontSize(18);

            // Date and Mood
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Date: {entry.EntryDate:MMMM dd, yyyy}").FontSize(10);
                row.RelativeItem().AlignRight().Text($"Mood: {GetMoodDisplayName(entry.PrimaryMood)}")
                    .FontSize(10).FontColor(GetMoodColor(entry.PrimaryMood));
            });

            // Category and Tags
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Category: {entry.Category}").FontSize(10).Italic();
                
                if (entry.JournalEntryTags != null && entry.JournalEntryTags.Any())
                {
                    row.RelativeItem().AlignRight().Text($"Tags: {string.Join(", ", entry.JournalEntryTags.Select(t => t.Tag?.Name))}")
                        .FontSize(10).FontColor(Colors.Blue.Medium);
                }
            });

            column.Item().PaddingTop(5);

            // Content
            var plainText = StripHtml(entry.Description);
            column.Item().Text(plainText).FontSize(11).LineHeight(1.4f);
        });
    }

    private string GetDownloadsPath()
    {
        string downloadsPath;
        
#if WINDOWS
        downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            "Downloads");
#elif ANDROID
        downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDownloads)?.AbsolutePath 
            ?? Path.Combine(FileSystem.AppDataDirectory, "Downloads");
#elif IOS || MACCATALYST
        downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "..", "Downloads");
#else
        downloadsPath = Path.Combine(FileSystem.AppDataDirectory, "Downloads");
#endif

        // Ensure the Downloads directory exists
        if (!Directory.Exists(downloadsPath))
        {
            Directory.CreateDirectory(downloadsPath);
        }

        return downloadsPath;
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid filename characters
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        
        // Limit length to 50 characters
        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "journal_entry" : sanitized;
    }

    private string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        // Replace common HTML tags with appropriate formatting
        var text = html;
        
        // Replace paragraph tags with newlines
        text = Regex.Replace(text, @"<p[^>]*>", "");
        text = Regex.Replace(text, @"</p>", "\n\n");
        
        // Replace br tags with newlines
        text = Regex.Replace(text, @"<br[^>]*>", "\n");
        
        // Remove all other HTML tags
        text = Regex.Replace(text, @"<[^>]+>", "");
        
        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
        
        // Clean up multiple newlines
        text = Regex.Replace(text, @"\n\s*\n\s*\n+", "\n\n");
        
        return text.Trim();
    }

    private string GetMoodDisplayName(MoodCategory mood)
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

    private string GetMoodColor(MoodCategory mood)
    {
        return mood switch
        {
            MoodCategory.Happy or MoodCategory.Excited or MoodCategory.Relaxed 
                or MoodCategory.Grateful or MoodCategory.Confident => Colors.Green.Medium,
            MoodCategory.Sad or MoodCategory.Angry or MoodCategory.Stressed 
                or MoodCategory.Lonely or MoodCategory.Anxious => Colors.Red.Medium,
            _ => Colors.Grey.Medium
        };
    }
}
