using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MediaCloud.WebApp
{
    public class AuthData
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
    }
}
