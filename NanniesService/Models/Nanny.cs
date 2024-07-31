using Microsoft.AspNetCore.Identity;

namespace NanniesService.Models
{
    public class Nanny : IdentityUser
    {
        public string? Name { get; set; }
    }
}
