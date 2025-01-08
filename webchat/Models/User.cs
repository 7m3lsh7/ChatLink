using System.ComponentModel.DataAnnotations;

namespace webchat.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

	    [Required(ErrorMessage = "Please Enter Your Username")]
		[StringLength(255)]
		public string Username { get; set; }

        [Required(ErrorMessage = "Please Enter Your Email")]
        [StringLength(255)]
		public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Your Password")]
        [StringLength(255)]
		public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Please Enter Your ProfilePicture")]
		public string ProfilePicture { get; set; }

	    public DateTime CrearetedAt { get; set; }

		public bool IsOnline { get; set; }
    }
}
