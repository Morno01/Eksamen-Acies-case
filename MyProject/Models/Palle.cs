using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class Palle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PalleBeskrivelse { get; set; } = string.Empty;

        [Required]
        public int Laengde { get; set; }

        [Required]
        public int Bredde { get; set; }

        [Required]
        public int Hoejde { get; set; }

        [StringLength(50)]
        public string? Pallegruppe { get; set; }

        [Required]
        [StringLength(50)]
        public string Palletype { get; set; } = "Trae";

        [Required]
        public decimal Vaegt { get; set; }

        [Required]
        public int MaksHoejde { get; set; }

        [Required]
        public decimal MaksVaegt { get; set; }

        public int Overmaal { get; set; } = 0;

        public int LuftMellemElementer { get; set; } = 0;

        public bool Aktiv { get; set; } = true;

        public int Sortering { get; set; } = 0;
    }
}
