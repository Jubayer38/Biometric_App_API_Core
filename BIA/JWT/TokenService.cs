using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BIA.Entity.ResponseEntity;

namespace BIA.JWT
{
    public class TokenService
    {
        private readonly string _secretKey;

        public TokenService(string secretKey)
        {
            _secretKey = secretKey;
        }

        public string GenerateToken(LoginUserInfoResponseRev loginUser, string loginprovider)
        {
            var claims = new[]
            {
            new Claim("loginProvider", loginprovider),
            new Claim("channel_name",loginUser.channel_name.ToString()),
            new Claim("user_name", loginUser.user_name.ToString()),
            new Claim("distributor_code",loginUser.distributor_code.ToString()),
            new Claim("center_code",loginUser.center_code.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime currentDateTime = DateTime.Now.Date;
            DateTime expirationTime = currentDateTime.AddHours(24);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expirationTime,  // Set token expiration
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateTokenV2(ResellerLoginUserInfoResponse loginUser, string loginprovider)
        {
            var claims = new[]
            {
            new Claim("loginProvider", loginprovider),
            new Claim("channel_name",loginUser.channel_name.ToString()),
            new Claim("user_name", loginUser.user_name.ToString()),
            new Claim("distributor_code",loginUser.distributor_code.ToString()),
            new Claim("center_code",loginUser.center_code.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime currentDateTime = DateTime.Now.Date;
            DateTime expirationTime = currentDateTime.AddHours(24);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expirationTime,  // Set token expiration
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
         
        public string GenerateTokenV3(string user_name, string loginprovider)
        {
            var claims = new[]
            {
            new Claim("loginProvider", loginprovider),
            new Claim("user_name", user_name.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime currentDateTime = DateTime.Now.Date;
            DateTime expirationTime = currentDateTime.AddHours(24);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expirationTime,  // Set token expiration
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
