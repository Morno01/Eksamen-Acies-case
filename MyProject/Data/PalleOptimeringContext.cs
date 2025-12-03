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

            // Seed data - sample elementer med realistiske mål
            modelBuilder.Entity<Element>().HasData(
                // Døre - Serie A
                new Element
                {
                    Id = 1,
                    Reference = "DØR-001",
                    Type = "Dør",
                    Serie = "Serie-A",
                    Hoejde = 2100,  // Standard dør højde
                    Bredde = 900,   // Standard dør bredde
                    Dybde = 120,    // Dør tykkelse med karm
                    Vaegt = 45m,    // Typisk vægt for træ/glas dør
                    RotationsRegel = "Ja",
                    ErSpecialelement = false,
                    ErGeometrielement = false
                },
                new Element
                {
                    Id = 2,
                    Reference = "DØR-002",
                    Type = "Dør",
                    Serie = "Serie-A",
                    Hoejde = 2100,
                    Bredde = 800,   // Smallere dør
                    Dybde = 120,
                    Vaegt = 42m,
                    RotationsRegel = "Ja",
                    ErSpecialelement = false,
                    ErGeometrielement = false
                },
                new Element
                {
                    Id = 3,
                    Reference = "DØR-003",
                    Type = "Dør",
                    Serie = "Serie-A",
                    Hoejde = 2100,
                    Bredde = 1200,  // Dobbelt dør
                    Dybde = 120,
                    Vaegt = 65m,
                    RotationsRegel = "Nej",  // Tung dør må ikke roteres
                    ErSpecialelement = false,
                    ErGeometrielement = false
                },
                // Vinduer - Serie B
                new Element
                {
                    Id = 4,
                    Reference = "VIND-001",
                    Type = "Vindue",
                    Serie = "Serie-B",
                    Hoejde = 1200,
                    Bredde = 1000,
                    Dybde = 150,    // Med karm
                    Vaegt = 28m,
                    RotationsRegel = "Ja",
                    ErSpecialelement = false,
                    ErGeometrielement = false
                },
                new Element
                {
                    Id = 5,
                    Reference = "VIND-002",
                    Type = "Vindue",
                    Serie = "Serie-B",
                    Hoejde = 1400,
                    Bredde = 800,
                    Dybde = 150,
                    Vaegt = 25m,
                    RotationsRegel = "Ja",
                    ErSpecialelement = false,
                    ErGeometrielement = false
                },
                new Element
                {
                    Id = 6,
                    Reference = "VIND-003",
                    Type = "Vindue",
                    Serie = "Serie-B",
                    Hoejde = 1800,  // Stort vindue
                    Bredde = 1500,
                    Dybde = 150,
                    Vaegt = 52m,
                    RotationsRegel = "Nej",
                    ErSpecialelement = false,
                    ErGeometrielement = false
                },
                // Special vinduer - Serie C
                new Element
                {
                    Id = 7,
                    Reference = "SPEC-001",
                    Type = "Vindue",
                    Serie = "Serie-C",
                    Hoejde = 2200,  // Panorama vindue
                    Bredde = 2400,
                    Dybde = 180,
                    Vaegt = 95m,
                    RotationsRegel = "Nej",
                    ErSpecialelement = true,
                    ErGeometrielement = false,
                    MaksElementerPrPalle = 2  // Max 2 store panorama vinduer pr palle
                },
                new Element
                {
                    Id = 8,
                    Reference = "GEOM-001",
                    Type = "Vindue",
                    Serie = "Serie-C",
                    Hoejde = 1600,  // Buet vindue
                    Bredde = 1200,
                    Dybde = 200,
                    Vaegt = 48m,
                    RotationsRegel = "Skal",  // Skal roteres pga. form
                    ErSpecialelement = true,
                    ErGeometrielement = true  // Må ikke stables ovenpå
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
                    SorteringsPrioritering = "Type,Specialelement,Pallestorrelse,Elementstorrelse,Vaegt,Serie",
                    PlacerLaengsteElementerYderst = true,
                    MaksBalanceVaerdi = 100m
                }
            );
        }
    }
}
