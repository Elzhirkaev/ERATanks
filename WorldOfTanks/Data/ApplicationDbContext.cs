using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.Register;

namespace WorldOfTanks.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<ApplicationUser>? ApplicationUser { get; set; }
        public DbSet<PassiveMapElement>? PassiveMapElement { get; set; }
        public DbSet<Map>? Map { get; set; }
        public DbSet<Weapon>? Weapon { get; set; }
        public DbSet<Tank>? Tank { get; set; }

    }
}
