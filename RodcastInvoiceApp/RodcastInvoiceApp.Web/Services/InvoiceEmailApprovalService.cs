using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RodcastInvoiceApp.Web.Data;
using RodcastInvoiceApp.Web.Data.Models;
using RodcastInvoiceApp.Web.DataTransferObjects.Invoice;
using RodcastInvoiceApp.Web.Exceptions;
using RodcastInvoiceApp.Web.Interfaces;
using RodcastInvoiceApp.Web.Resources;
using RodcastInvoiceApp.Web.Security;

namespace RodcastInvoiceApp.Web.Services
{
    public class InvoiceEmailApprovalService : IInvoiceEmailApprovalService
    {
        private readonly AppDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceEmailService _invoiceEmailService;
        private readonly ICurrentUserAccessor _currentUser;
        private readonly IStringLocalizer<SharedResource> _loc;

        public InvoiceEmailApprovalService(
            AppDbContext context,
            IInvoiceService invoiceService,
            IInvoiceEmailService invoiceEmailService,
            ICurrentUserAccessor currentUser,
            IStringLocalizer<SharedResource> loc)
        {
            _context = context;
            _invoiceService = invoiceService;
            _invoiceEmailService = invoiceEmailService;
            _currentUser = currentUser;
            _loc = loc;
        }

        public async Task<InvoiceResponseDto> RequestSendAsync(int invoiceId, ApplicationUser requestedBy)
        {
            await _invoiceEmailService.ValidateSendableAsync(invoiceId, requestedBy);

            // El admin manda directo, sin pasar por la cola de aprobacion.
            if (await _currentUser.IsAdminAsync())
                return await _invoiceEmailService.SendAsync(invoiceId, requestedBy);

            var alreadyPending = await _context.InvoiceEmailApprovals
                .AnyAsync(a => a.InvoiceId == invoiceId && a.Status == EmailApprovalStatus.Pending);
            if (alreadyPending)
                throw new ConflictException(_loc["SvcErr_ApprovalAlreadyPending"]);

            _context.InvoiceEmailApprovals.Add(new InvoiceEmailApproval
            {
                InvoiceId = invoiceId,
                RequestedByUserId = requestedBy.Id,
                Status = EmailApprovalStatus.Pending
            });
            await _context.SaveChangesAsync();

            return await _invoiceService.UpdateStatusAsync(invoiceId, InvoiceStatus.PendingApproval);
        }

        public async Task<int> GetPendingCountAsync()
        {
            await _currentUser.EnsureAdminAsync();

            return await _context.InvoiceEmailApprovals
                .CountAsync(a => a.Status == EmailApprovalStatus.Pending);
        }

        public async Task<List<InvoiceEmailApprovalListItemDto>> GetPendingAsync()
        {
            await _currentUser.EnsureAdminAsync();

            return await _context.InvoiceEmailApprovals
                .AsNoTracking()
                .Where(a => a.Status == EmailApprovalStatus.Pending)
                .Include(a => a.Invoice).ThenInclude(i => i.Project).ThenInclude(p => p.Client)
                .Include(a => a.RequestedByUser)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new InvoiceEmailApprovalListItemDto
                {
                    ApprovalId = a.Id,
                    InvoiceId = a.InvoiceId,
                    InvoiceNumber = a.Invoice.InvoiceNumber,
                    ClientName = a.Invoice.Project.Client.Name,
                    RequestedByName = a.RequestedByUser.DisplayName,
                    RequestedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<InvoiceEmailApprovalDetailDto> GetDetailAsync(int approvalId)
        {
            await _currentUser.EnsureAdminAsync();

            var approval = await LoadPendingApprovalAsync(approvalId);
            var preview = await _invoiceEmailService.BuildPreviewAsync(approval.InvoiceId, approval.RequestedByUser);

            return new InvoiceEmailApprovalDetailDto
            {
                ApprovalId = approval.Id,
                InvoiceId = approval.InvoiceId,
                InvoiceNumber = approval.Invoice.InvoiceNumber,
                ClientName = approval.Invoice.Project.Client.Name,
                RequestedByName = approval.RequestedByUser.DisplayName,
                RequestedAt = approval.CreatedAt,
                Preview = preview
            };
        }

        public async Task<InvoiceResponseDto> ApproveAsync(int approvalId, ApplicationUser reviewer)
        {
            await _currentUser.EnsureAdminAsync();

            var approval = await LoadPendingApprovalAsync(approvalId);

            // Se manda con las credenciales y la firma de quien lo pidio, no las del admin.
            var result = await _invoiceEmailService.SendAsync(approval.InvoiceId, approval.RequestedByUser);

            approval.Status = EmailApprovalStatus.Approved;
            approval.ReviewedByUserId = reviewer.Id;
            approval.ReviewedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<InvoiceResponseDto> RejectAsync(int approvalId, ApplicationUser reviewer, string comment)
        {
            await _currentUser.EnsureAdminAsync();

            if (string.IsNullOrWhiteSpace(comment))
                throw new BadRequestException(_loc["SvcErr_RejectionCommentRequired"]);

            var approval = await LoadPendingApprovalAsync(approvalId);

            approval.Status = EmailApprovalStatus.Rejected;
            approval.RejectionComment = comment.Trim();
            approval.ReviewedByUserId = reviewer.Id;
            approval.ReviewedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await _invoiceService.UpdateStatusAsync(approval.InvoiceId, InvoiceStatus.Draft);
        }

        private async Task<InvoiceEmailApproval> LoadPendingApprovalAsync(int approvalId)
        {
            var approval = await _context.InvoiceEmailApprovals
                .Include(a => a.Invoice).ThenInclude(i => i.Project).ThenInclude(p => p.Client)
                .Include(a => a.RequestedByUser)
                .FirstOrDefaultAsync(a => a.Id == approvalId)
                ?? throw new NotFoundException(_loc["SvcErr_ApprovalNotFound"]);

            if (approval.Status != EmailApprovalStatus.Pending)
                throw new ConflictException(_loc["SvcErr_ApprovalAlreadyReviewed"]);

            return approval;
        }
    }
}
