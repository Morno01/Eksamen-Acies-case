using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Element
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        [StringLength(50)]
        public string? Type { get; set; }

        [StringLength(100)]
        public string? Serie { get; set; }

        [Required]
        public int Hoejde { get; set; }

        [Required]
        public int Bredde { get; set; }

        [Required]
        public int Dybde { get; set; }

        [Required]
        public decimal Vaegt { get; set; }

        public bool ErSpecialelement { get; set; } = false;

        public bool ErGeometrielement { get; set; } = false;

        [Required]
        [StringLength(10)]
        public string RotationsRegel { get; set; } = "Ja";

        [StringLength(50)]
        public string? KraeverPalletype { get; set; }

        public int? MaksElementerPrPalle { get; set; }
    }
}
