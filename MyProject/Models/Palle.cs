using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    /// <summary>
    /// Repræsenterer en palle som elementer kan pakkes på
    /// </summary>
    public class Palle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PalleBeskrivelse { get; set; } = string.Empty;

        /// <summary>
        /// Pallens længde i mm
        /// </summary>
        [Required]
        public int Laengde { get; set; }

        /// <summary>
        /// Pallens bredde i mm
        /// </summary>
        [Required]
        public int Bredde { get; set; }

        /// <summary>
        /// Pallens egen højde i mm
        /// </summary>
        [Required]
        public int Hoejde { get; set; }

        /// <summary>
        /// Om det f.eks er 75'er paller eller 80'er paller
        /// </summary>
        [StringLength(50)]
        public string? Pallegruppe { get; set; }

        /// <summary>
        /// Om det f.eks. Er træ, alu eller glasstelspalle
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Palletype { get; set; } = "Trae";

        /// <summary>
        /// Pallens egen vægt i kg
        /// </summary>
        [Required]
        public decimal Vaegt { get; set; }

        /// <summary>
        /// Makshøjde for palle + elementer i mm
        /// </summary>
        [Required]
        public int MaksHoejde { get; set; }

        /// <summary>
        /// Maks. vægt for pallen (elementer og palle) i kg
        /// </summary>
        [Required]
        public decimal MaksVaegt { get; set; }

        /// <summary>
        /// Overmål i mm - hvor meget må elementer rage ud over pallen
        /// </summary>
        public int Overmaal { get; set; } = 0;

        /// <summary>
        /// Hvor meget luft der skal være mellem elementer på pallen i mm
        /// </summary>
        public int LuftMellemElementer { get; set; } = 0;

        /// <summary>
        /// Om en palle er aktiv i palleoptimeringen
        /// </summary>
        public bool Aktiv { get; set; } = true;

        /// <summary>
        /// Sorteringsorden for at finde mindste palle
        /// </summary>
        public int Sortering { get; set; } = 0;
    }
}
