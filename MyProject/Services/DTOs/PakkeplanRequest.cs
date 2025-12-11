namespace MyProject.Services.DTOs
{

    public class PakkeplanRequest
    {

        public List<int> ElementIds { get; set; } = new();


        public string? OrdreReference { get; set; }


        public int? SettingsId { get; set; }
    }
}
