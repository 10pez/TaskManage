using Microsoft.AspNetCore.Identity;

namespace TaskManage.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public int? GroupId { get; set; }
    }
}
