using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NanniesService.Models;

namespace NanniesService.Data
{
    public class AppDbContext(DbContextOptions options) : IdentityDbContext<Nanny>(options)
    {
        public DbSet<Nanny> Nannies { get; set; }
    }
}
