using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{

    public class PalleOptimeringSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Navn { get; set; } = "Standard";


        [Required]
        [Range(1, 10)]
        public int MaksLag { get; set; } = 2;


        [Required]
        public decimal TilladVendeOpTilMaksKg { get; set; } = 50;


        [Required]
        public decimal HoejdeBreddefaktor { get; set; } = 0.3m;


        public bool HoejdeBreddefaktorKunForEnkeltElementer { get; set; } = true;


        public int? TilladStablingOpTilMaksHoejdeInklPalle { get; set; }


        public decimal? TilladStablingOpTilMaksElementVaegt { get; set; }


        public int TillaegMonteringAfEndeplade { get; set; } = 0;


        public bool Aktiv { get; set; } = true;


        [StringLength(500)]
        public string SorteringsPrioritering { get; set; } = "Type,Specialelement,Pallestorrelse,Elementstorrelse,Vaegt,Serie";


        public bool PlacerLaengsteElementerYderst { get; set; } = true;


        public decimal? MaksBalanceVaerdi { get; set; }
    }
}
