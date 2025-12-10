using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    /// <summary>
    /// Repræsenterer et element (dør/vindue) der skal pakkes på en palle
    /// </summary>
    public class Element
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference til ordre eller position
        /// </summary>
        [StringLength(100)]
        public string? Reference { get; set; }

        /// <summary>
        /// Type af element: Dør eller Vindue
        /// </summary>
        [StringLength(50)]
        public string? Type { get; set; }

        /// <summary>
        /// Mærke/Brand af element
        /// </summary>
        [StringLength(100)]
        public string? Maerke { get; set; }

        /// <summary>
        /// Serie (produktions batch)
        /// </summary>
        [StringLength(100)]
        public string? Serie { get; set; }

        /// <summary>
        /// Elementets højde i mm
        /// </summary>
        [Required]
        public int Hoejde { get; set; }

        /// <summary>
        /// Elementets bredde i mm
        /// </summary>
        [Required]
        public int Bredde { get; set; }

        /// <summary>
        /// Elementets dybde i mm
        /// </summary>
        [Required]
        public int Dybde { get; set; }

        /// <summary>
        /// Elementets vægt i kg
        /// </summary>
        [Required]
        public decimal Vaegt { get; set; }

        /// <summary>
        /// Om elementet er et specialelement (påvirker sortering)
        /// </summary>
        public bool ErSpecialelement { get; set; } = false;

        /// <summary>
        /// Om elementet er et geometri-element (ikke firkantet)
        /// Der må aldrig stables ovenpå geometri-elementer
        /// </summary>
        public bool ErGeometrielement { get; set; } = false;

        /// <summary>
        /// Rotationsregel: Nej, Ja, Skal
        /// </summary>
        [Required]
        [StringLength(10)]
        public string RotationsRegel { get; set; } = "Ja";

        /// <summary>
        /// Kræver special palle type
        /// </summary>
        [StringLength(50)]
        public string? KraeverPalletype { get; set; }

        /// <summary>
        /// Maks elementer pr palle (bruges f.eks til foldedøre)
        /// </summary>
        public int? MaksElementerPrPalle { get; set; }
    }
}
