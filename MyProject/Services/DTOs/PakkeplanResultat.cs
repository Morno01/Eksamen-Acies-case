namespace MyProject.Services.DTOs
{
    /// <summary>
    /// Resultat af pakkeplan generering
    /// </summary>
    public class PakkeplanResultat
    {
        public int PakkeplanId { get; set; }
        public int AntalPaller { get; set; }
        public int AntalElementer { get; set; }
        public List<PalleResultat> Paller { get; set; } = new();
        public string Status { get; set; } = "Success";
        public List<string> Meddelelser { get; set; } = new();
    }

    public class PalleResultat
    {
        public int PalleNummer { get; set; }
        public string PalleBeskrivelse { get; set; } = string.Empty;
        public int AntalLag { get; set; }
        public decimal SamletVaegt { get; set; }
        public int SamletHoejde { get; set; }
        public List<ElementPlacering> Elementer { get; set; } = new();
    }

    public class ElementPlacering
    {
        public int ElementId { get; set; }
        public string? Reference { get; set; }
        public int Lag { get; set; }
        public int Plads { get; set; }
        public bool ErRoteret { get; set; }
        public int Hoejde { get; set; }
        public int Bredde { get; set; }
        public decimal Vaegt { get; set; }
    }
}
