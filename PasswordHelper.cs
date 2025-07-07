using System.Security.Cryptography;
using System.Text;

namespace BTL.DNU.IdeaSpark.Web.Helpers
{
    public static class PasswordHelper
    {
        // Hàm băm mật khẩu bằng SHA256
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}