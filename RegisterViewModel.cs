using System.ComponentModel.DataAnnotations;

namespace BTL.DNU.IdeaSpark.Web.Models
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? Department { get; set; }
        
        [Required]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Email.ToLower().EndsWith("@gmail.com"))
            {
                yield return new ValidationResult("Chỉ chấp nhận Gmail.", new[] { nameof(Email) });
            }
        }
    }
}