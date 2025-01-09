using System.ComponentModel.DataAnnotations;

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

    public DateTime CrearetedAt { get; set; }
}
