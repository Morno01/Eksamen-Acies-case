namespace MyProject.Services.DTOs
{
    /// <summary>
    /// Request for at generere en pakkeplan
    /// </summary>
    public class PakkeplanRequest
    {
        /// <summary>
        /// Liste af element IDs der skal pakkes
        /// </summary>
        public List<int> ElementIds { get; set; } = new();

        /// <summary>
        /// Valgfri reference til ordren
        /// </summary>
        public string? OrdreReference { get; set; }

        /// <summary>
        /// Valgfri settings ID - hvis ikke angivet bruges aktiv settings
        /// </summary>
        public int? SettingsId { get; set; }
    }
}
