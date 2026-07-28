using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.DataTransferObjects.Audit;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserAccessor _currentUser;

        public AuditService(AppDbContext context, ICurrentUserAccessor currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<InvoiceCreationLogDto>> GetInvoiceCreationLogAsync()
        {
            await _currentUser.EnsureAdminAsync();

            return await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Project).ThenInclude(p => p.Client)
                .Include(i => i.CreatedByUser)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InvoiceCreationLogDto
                {
                    InvoiceId = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    ClientName = i.Project.Client.Name,
                    CreatedByName = i.CreatedByUser != null ? i.CreatedByUser.DisplayName : null,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<InvoiceEmailLogDto>> GetEmailSendLogAsync()
        {
            await _currentUser.EnsureAdminAsync();

            return await _context.InvoiceEmailLogs
                .AsNoTracking()
                .Include(l => l.Invoice).ThenInclude(i => i.Project).ThenInclude(p => p.Client)
                .Include(l => l.SentByUser)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new InvoiceEmailLogDto
                {
                    InvoiceId = l.InvoiceId,
                    InvoiceNumber = l.Invoice.InvoiceNumber,
                    ClientName = l.Invoice.Project.Client.Name,
                    SentByName = l.SentByUser.DisplayName,
                    ToEmail = l.ToEmail,
                    SentAt = l.CreatedAt,
                    SavedToSentFolder = l.SavedToSentFolder,
                    IsRetry = l.IsRetry
                })
                .ToListAsync();
        }
    }
}
