using System.ComponentModel.DataAnnotations;

namespace MyProject.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email er påkrævet")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password er påkrævet")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Husk mig")]
        public bool RememberMe { get; set; }
    }
}
