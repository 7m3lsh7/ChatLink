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

        [Required(ErrorMessage = "Please Enter Your Password")]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Please Enter Your Profile Picture")]
        public string ProfilePicture { get; set; }

        public bool IsAdmin { get; set; }

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

        [Required(ErrorMessage = "Please Enter Your Email")]
        [StringLength(255)]
        public string Language { get; set; }

        [Required(ErrorMessage = "Please Enter Your Email")]
        [StringLength(255)]
        public string TimeZone { get; set; }


        public string VerificationCode { get; set; }

        public DateTime? VerificationCodeExpiry { get; set; }

        public bool IsVerified { get; set; }

        public string ResetPasswordToken { get; set; } 

        public DateTime? ResetPasswordExpiry { get; set; } 
    }
}
