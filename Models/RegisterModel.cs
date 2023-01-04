using System.ComponentModel.DataAnnotations;

namespace NovaTime.Models
{
    public class RegisterModel
    {
       [Required(ErrorMessage = "UserName is required ")]
        public string UserName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email is required ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required ")]
        public string Password { get; set; }
        public string Matricule { get; set; } 
       
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

    }
}
