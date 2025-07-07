namespace BTL.DNU.IdeaSpark.Web.Models;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Role { get; set; } = "User";
    public string? AvatarUrl { get; set; }
    public string PasswordHash { get; set; }
    public string? Department { get; set; }
    public string? FullName { get; set; }     // Tên người dùng
       // Khoa / Đơn vị
    public int Points { get; set; }           // Điểm
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }


}