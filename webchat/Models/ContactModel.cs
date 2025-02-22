using System;
using System.ComponentModel.DataAnnotations;

namespace webchat.Models
{
    public class ContactModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^\+?[1-9][0-9]{9,14}$", ErrorMessage = "Please enter a valid mobile number (10-15 digits).")]
        [DataType(DataType.PhoneNumber)]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Additional information is required.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Additional information must be between 10 and 500 characters.")]
        public string AdditionalInfo { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}
