namespace InteractiveGameApi.InteractiveGame.DAL
{
    using InteractiveGame.DAL.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;

    public class InteractiveGameDbContext : DbContext
    {
        public InteractiveGameDbContext(DbContextOptions<InteractiveGameDbContext> options) : base(options) { }

        public DbSet<PotentiometerResult> PotentiometerResults { get; set; }
    }

}
