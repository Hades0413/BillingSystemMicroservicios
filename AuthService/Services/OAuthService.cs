using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AuthService.Services
{
    public class OAuthService
    {
        private readonly IConfiguration _configuration;

        public OAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> ValidateOAuthToken(string token)
        {
            return await Task.FromResult(true);
        }
    }
}