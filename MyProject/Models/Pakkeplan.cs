using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{

    public class Pakkeplan
    {
        [Key]
        public int Id { get; set; }


        [StringLength(100)]
        public string? OrdreReference { get; set; }


        [Required]
        public DateTime Oprettet { get; set; } = DateTime.Now;


        public int? SettingsId { get; set; }

        [ForeignKey("SettingsId")]
        public PalleOptimeringSettings? Settings { get; set; }


        public int AntalPaller { get; set; }


        public int AntalElementer { get; set; }


        public ICollection<PakkeplanPalle> Paller { get; set; } = new List<PakkeplanPalle>();
    }


    public class PakkeplanPalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PakkeplanId { get; set; }

        [ForeignKey("PakkeplanId")]
        public Pakkeplan Pakkeplan { get; set; } = null!;


        [Required]
        public int PalleNummer { get; set; }

        [Required]
        public int PalleId { get; set; }

        [ForeignKey("PalleId")]
        public Palle Palle { get; set; } = null!;


        public int SamletHoejde { get; set; }


        public decimal SamletVaegt { get; set; }


        public int AntalLag { get; set; }


        public ICollection<PakkeplanElement> Elementer { get; set; } = new List<PakkeplanElement>();
    }


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


        [Required]
        public int Lag { get; set; }


        [Required]
        public int Plads { get; set; }


        public bool ErRoteret { get; set; }


        public int Sortering { get; set; }
    }
}
