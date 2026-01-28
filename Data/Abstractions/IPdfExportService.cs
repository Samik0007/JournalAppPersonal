using JournalPersonalApp.Data.Models;

namespace JournalPersonalApp.Data.Abstractions;

public interface IPdfExportService
{
    Task<string> ExportQuestPdfAsync(CancellationToken ct = default);
    Task<string> ExportJournalEntryAsync(Journalentry entry, CancellationToken ct = default);
    Task<string> ExportAllJournalEntriesAsync(List<Journalentry> entries, CancellationToken ct = default);
}
