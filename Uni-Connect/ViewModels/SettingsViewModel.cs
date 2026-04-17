namespace Uni_Connect.ViewModels
{
    public class SettingsViewModel
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string YearOfStudy { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
    }
}
