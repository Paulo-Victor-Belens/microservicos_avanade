using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Model
{
    public class ApplicationUser : IdentityUser<long>
    {
        public string FullName { get; set; } = null!;
        public DateTime MemberSince { get; set; }
    }
}