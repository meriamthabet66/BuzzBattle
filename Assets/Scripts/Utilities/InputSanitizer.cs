using System.Text.RegularExpressions;

namespace Utilities
{
    public static class InputSanitizer
    {
        public static string CleanEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return "";

            email = email.Trim();

            // Remove invisible direction marks
            email = email.Replace("\u200F", "");
            email = email.Replace("\u200E", "");
            email = email.Replace("\u202A", "");
            email = email.Replace("\u202B", "");
            email = email.Replace("\u202C", "");

            return email;
        }

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
            );
        }
    }
}