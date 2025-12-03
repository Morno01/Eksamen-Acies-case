using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Fulde navn er påkrævet")]
        [Display(Name = "Fulde Navn")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email er påkrævet")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password er påkrævet")]
        [StringLength(100, ErrorMessage = "Password skal være mindst {2} tegn langt.", MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Bekræft password")]
        [Compare("Password", ErrorMessage = "Password og bekræftelse matcher ikke.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
