using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    /// <summary>
    /// Generelle indstillinger for palleoptimering algoritmen
    /// </summary>
    public class PalleOptimeringSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Navn { get; set; } = "Standard";

        /// <summary>
        /// Angiver hvor mange lag man må have på en palle
        /// </summary>
        [Required]
        [Range(1, 10)]
        public int MaksLag { get; set; } = 2;

        /// <summary>
        /// Hvor tunge må elementerne være før de som udgangspunkt ikke længere må vendes (kg)
        /// </summary>
        [Required]
        public decimal TilladVendeOpTilMaksKg { get; set; } = 50;

        /// <summary>
        /// Mindste af elementets højde/bredde og bredde/højde
        /// Elementer der er meget lange på det ene led lægges ned på pallen for at pallen ikke tipper
        /// </summary>
        [Required]
        public decimal HoejdeBreddefaktor { get; set; } = 0.3m;

        /// <summary>
        /// Om reglen kun skal gælde for enkelt elementer
        /// </summary>
        public bool HoejdeBreddefaktorKunForEnkeltElementer { get; set; } = true;

        /// <summary>
        /// Elementerne skal tages ned fra pallerne igen på byggepladsen.
        /// For at sikre at tømrerne kan fjerne elementer over lag 1 sikkert (mm)
        /// </summary>
        public int? TilladStablingOpTilMaksHoejdeInklPalle { get; set; }

        /// <summary>
        /// Max element vægt for stabling (kg)
        /// </summary>
        public decimal? TilladStablingOpTilMaksElementVaegt { get; set; }

        /// <summary>
        /// Tillæg montering af endeplade (mm)
        /// For at udregne den korrekte pallelængde
        /// </summary>
        public int TillaegMonteringAfEndeplade { get; set; } = 0;

        /// <summary>
        /// Om denne settings profil er aktiv
        /// </summary>
        public bool Aktiv { get; set; } = true;

        /// <summary>
        /// Sorteringskriterier prioritering (kommasepareret)
        /// F.eks: "Type,Specialelement,Pallestorrelse,Elementstorrelse,Vaegt,Serie"
        /// </summary>
        [StringLength(500)]
        public string SorteringsPrioritering { get; set; } = "Type,Specialelement,Pallestorrelse,Elementstorrelse,Vaegt,Serie";

        /// <summary>
        /// Om længste elementer skal placeres yderst (alternativt: vægtfordeling)
        /// </summary>
        public bool PlacerLaengsteElementerYderst { get; set; } = true;

        /// <summary>
        /// Max balance værdi hvis PlacerLaengsteElementerYderst er true
        /// Hvis balancen overskrider dette tal, rykkes der alligevel rundt på rækkerne
        /// </summary>
        public decimal? MaksBalanceVaerdi { get; set; }
    }
}
