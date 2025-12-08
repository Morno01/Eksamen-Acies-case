using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    /// <summary>
    /// Repræsenterer en genereret pakkeplan
    /// </summary>
    public class Pakkeplan
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference til ordren
        /// </summary>
        [StringLength(100)]
        public string? OrdreReference { get; set; }

        /// <summary>
        /// Hvornår planen blev genereret
        /// </summary>
        [Required]
        public DateTime Oprettet { get; set; } = DateTime.Now;

        /// <summary>
        /// Hvilket settings blev brugt
        /// </summary>
        public int? SettingsId { get; set; }

        [ForeignKey("SettingsId")]
        public PalleOptimeringSettings? Settings { get; set; }

        /// <summary>
        /// Samlet antal paller i planen
        /// </summary>
        public int AntalPaller { get; set; }

        /// <summary>
        /// Samlet antal elementer
        /// </summary>
        public int AntalElementer { get; set; }

        /// <summary>
        /// Navigation property til paller i denne plan
        /// </summary>
        public ICollection<PakkeplanPalle> Paller { get; set; } = new List<PakkeplanPalle>();
    }

    /// <summary>
    /// Repræsenterer en palle i en pakkeplan
    /// </summary>
    public class PakkeplanPalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PakkeplanId { get; set; }

        [ForeignKey("PakkeplanId")]
        public Pakkeplan Pakkeplan { get; set; } = null!;

        /// <summary>
        /// Palle nummer i planen (1, 2, 3, osv.)
        /// </summary>
        [Required]
        public int PalleNummer { get; set; }

        [Required]
        public int PalleId { get; set; }

        [ForeignKey("PalleId")]
        public Palle Palle { get; set; } = null!;

        /// <summary>
        /// Samlet højde af elementer på denne palle (mm)
        /// </summary>
        public int SamletHoejde { get; set; }

        /// <summary>
        /// Samlet vægt af elementer på denne palle (kg)
        /// </summary>
        public decimal SamletVaegt { get; set; }

        /// <summary>
        /// Antal lag på denne palle
        /// </summary>
        public int AntalLag { get; set; }

        /// <summary>
        /// Navigation property til elementer på denne palle
        /// </summary>
        public ICollection<PakkeplanElement> Elementer { get; set; } = new List<PakkeplanElement>();
    }

    /// <summary>
    /// Repræsenterer et element placeret på en palle i en pakkeplan
    /// </summary>
    public class PakkeplanElement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PakkeplanPalleId { get; set; }

        [ForeignKey("PakkeplanPalleId")]
        public PakkeplanPalle PakkeplanPalle { get; set; } = null!;

        [Required]
        public int ElementId { get; set; }

        [ForeignKey("ElementId")]
        public Element Element { get; set; } = null!;

        /// <summary>
        /// Hvilket lag elementet er placeret i (1, 2, 3, osv.)
        /// </summary>
        [Required]
        public int Lag { get; set; }

        /// <summary>
        /// Position/plads på pallen (1-5)
        /// </summary>
        [Required]
        public int Plads { get; set; }

        /// <summary>
        /// Om elementet blev roteret/vendt
        /// </summary>
        public bool ErRoteret { get; set; }

        /// <summary>
        /// Sorteringsorden inden for pallen og laget
        /// </summary>
        public int Sortering { get; set; }
    }
}
