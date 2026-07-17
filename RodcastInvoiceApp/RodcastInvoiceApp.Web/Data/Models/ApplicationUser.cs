using Microsoft.AspNetCore.Identity;

namespace RodcastInvoiceApp.Web.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
    }
}
