using System.ComponentModel.DataAnnotations;
namespace webchat.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Username { get; set; }

        [Required(ErrorMessage = "Please Enter Your Email")]
        [StringLength(255)]
        public string Email { get; set; }

        [MinLength(9, ErrorMessage = "Password must be at least 9 characters long.")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{9,}$", ErrorMessage = "Password must include both letters and numbers.")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Please Enter Your Profile Picture")]
        public string ProfilePicture { get; set; }

        public string IsAdmin { get; set; }

        public DateTime CrearetedAt { get; set; }

        [Required(ErrorMessage = "Please Enter Your NickName")]
        [StringLength(255)]
        public string NickName { get; set; }

        [Required(ErrorMessage = "Please Enter Your Gender")]
        [StringLength(255)]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please Enter Your Country")]
        [StringLength(255)]
        public string Country { get; set; }

        [Required(ErrorMessage = "Please Enter Your Language")]
        [StringLength(255)]
        public string Language { get; set; }

        [Required(ErrorMessage = "Please Enter Your TimeZone")]
        [StringLength(255)]
        public string TimeZone { get; set; }


        public string VerificationCode { get; set; }

        public DateTime? VerificationCodeExpiry { get; set; }

        public bool IsVerified { get; set; }

        public string ResetPasswordToken { get; set; } 

        public DateTime? ResetPasswordExpiry { get; set; }

        public bool IsOnline { get; set; }  // Tracks whether the user is online or offline

        public bool IsPasswordChanged { get; set; }

        public DateTime? LastPasswordChangeDate { get; set; }

    }
}
