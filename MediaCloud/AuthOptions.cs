using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MediaCloud.WebApp
{
    public class AuthOptions
    {
        public const string ISSUER = "MediaCloud WebApp";
        public const string AUDIENCE = "MediaCloud WebApp - Client";
        const string KEY = "pjCqvgzSmdQUiPtHpjCqvgzSmdQUiPtH";
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
