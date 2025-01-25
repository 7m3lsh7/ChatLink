using System.ComponentModel.DataAnnotations;

namespace webchat.Models
{
    public class ContactModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Please enter a valid mobile number.")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Additional information is required.")]
        [StringLength(500, ErrorMessage = "Additional information cannot be longer than 500 characters.")]
        public string AdditionalInfo { get; set; }

        public DateTime? SubmissionDate { get; set; }

    }
}
