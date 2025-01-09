using System.ComponentModel.DataAnnotations;

namespace webchat.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

	   

        [Required(ErrorMessage = "Please Enter Your Email")]
        [StringLength(255)]
		public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Your Password")]
        [StringLength(255)]
		public string PasswordHash { get; set; }

		public bool IsEmailVerified { get; set; } = false;

    public string VerificationToken { get; set; }

        [Required(ErrorMessage = "Please Enter Your ProfilePicture")]
		

	    public DateTime CrearetedAt { get; set; }

		
    }
}
