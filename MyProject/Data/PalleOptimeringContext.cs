using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    /// <summary>
    /// Database context for palleoptimering systemet
    /// </summary>
    public class PalleOptimeringContext : IdentityDbContext<ApplicationUser>
    {
        public PalleOptimeringContext(DbContextOptions<PalleOptimeringContext> options)
            : base(options)
        {
        }

        public DbSet<Palle> Paller { get; set; } = null!;
        public DbSet<Element> Elementer { get; set; } = null!;
        public DbSet<PalleOptimeringSettings> Settings { get; set; } = null!;
        public DbSet<Pakkeplan> Pakkeplaner { get; set; } = null!;
        public DbSet<PakkeplanPalle> PakkeplanPaller { get; set; } = null!;
        public DbSet<PakkeplanElement> PakkeplanElementer { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfigurer decimal precision
            modelBuilder.Entity<Palle>()
                .Property(p => p.Vaegt)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Palle>()
                .Property(p => p.MaksVaegt)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Element>()
                .Property(e => e.Vaegt)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PalleOptimeringSettings>()
                .Property(s => s.TilladVendeOpTilMaksKg)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PalleOptimeringSettings>()
                .Property(s => s.HoejdeBreddefaktor)
                .HasPrecision(5, 2);

            modelBuilder.Entity<PalleOptimeringSettings>()
                .Property(s => s.TilladStablingOpTilMaksElementVaegt)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PalleOptimeringSettings>()
                .Property(s => s.MaksBalanceVaerdi)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PakkeplanPalle>()
                .Property(p => p.SamletVaegt)
                .HasPrecision(18, 2);

            // Seed data - standard paller
            modelBuilder.Entity<Palle>().HasData(
                new Palle
                {
                    Id = 1,
                    PalleBeskrivelse = "75'er Træpalle",
                    Laengde = 2400,
                    Bredde = 750,
                    Hoejde = 150,
                    Pallegruppe = "75",
                    Palletype = "Trae",
                    Vaegt = 25m,
                    MaksHoejde = 2800,
                    MaksVaegt = 1000m,
                    Overmaal = 50,
                    LuftMellemElementer = 10,
                    Aktiv = true,
                    Sortering = 1
                },
                new Palle
                {
                    Id = 2,
                    PalleBeskrivelse = "80'er Træpalle",
                    Laengde = 2400,
                    Bredde = 800,
                    Hoejde = 150,
                    Pallegruppe = "80",
                    Palletype = "Trae",
                    Vaegt = 27m,
                    MaksHoejde = 2800,
                    MaksVaegt = 1200m,
                    Overmaal = 50,
                    LuftMellemElementer = 10,
                    Aktiv = true,
                    Sortering = 2
                },
                new Palle
                {
                    Id = 3,
                    PalleBeskrivelse = "100'er Træpalle",
                    Laengde = 2400,
                    Bredde = 1000,
                    Hoejde = 150,
                    Pallegruppe = "100",
                    Palletype = "Trae",
                    Vaegt = 30m,
                    MaksHoejde = 2800,
                    MaksVaegt = 1500m,
                    Overmaal = 50,
                    LuftMellemElementer = 10,
                    Aktiv = true,
                    Sortering = 3
                }
            );

            // Seed data - standard settings
            modelBuilder.Entity<PalleOptimeringSettings>().HasData(
                new PalleOptimeringSettings
                {
                    Id = 1,
                    Navn = "Standard",
                    MaksLag = 2,
                    TilladVendeOpTilMaksKg = 50m,
                    HoejdeBreddefaktor = 0.3m,
                    HoejdeBreddefaktorKunForEnkeltElementer = true,
                    TilladStablingOpTilMaksHoejdeInklPalle = 1500,
                    TilladStablingOpTilMaksElementVaegt = 70m,
                    TillaegMonteringAfEndeplade = 20,
                    Aktiv = true,
                    SorteringsPrioritering = "Maerke,Specialelement,Pallestorrelse,Elementstorrelse,Vaegt,Serie",
                    PlacerLaengsteElementerYderst = true,
                    MaksBalanceVaerdi = 100m
                }
            );
        }
    }
}
