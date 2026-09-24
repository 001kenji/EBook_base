using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EBook.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
    public DbSet<EBook.Models.Payment> Payment { get; set; } = default!;
    public DbSet<EBook.Models.Library> Library { get; set; } = default!;
    }
}
