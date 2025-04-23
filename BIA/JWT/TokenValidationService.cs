using BIA.Entity.ResponseEntity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BIA.JWT
{
    public class TokenValidationService
    {
        private readonly string _secretKey;

        public TokenValidationService(string secretKey)
        {
            _secretKey = secretKey;
        }

        public ValidTokenResponse ValidateToken(string token)
        {
            ValidTokenResponse response = new ValidTokenResponse();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,  // Set to true if issuer validation is required
                ValidateAudience = false,  // Set to true if audience validation is required
                ClockSkew = TimeSpan.Zero  // No tolerance for expiration time
            };

            try
            {
                SecurityToken validatedToken;
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                var logindClaim = claimsPrincipal.FindFirst("loginProvider");
                var channelNameClaim = claimsPrincipal.FindFirst("channel_name");
                var usernameClaim = claimsPrincipal.FindFirst("user_name");
                var ddcodeClaim = claimsPrincipal.FindFirst("distributor_code");
                var centerCodeClaim = claimsPrincipal.FindFirst("center_code");
                
                if (logindClaim != null)
                {
                    response.LoginProviderId = logindClaim.Value;
                }
                if (channelNameClaim != null)
                {
                    response.ChannelName = channelNameClaim.Value;
                }
                if (usernameClaim != null)
                {
                    response.UserName = usernameClaim.Value;
                }
                if (ddcodeClaim != null)
                {
                    response.DistributorCode = ddcodeClaim.Value;
                }
                if (centerCodeClaim != null)
                {
                    response.CenterCode = centerCodeClaim.Value;
                }
                response.IsVallid = true;
                return response;
            }
            catch (Exception ex)
            {
                if(ex.Message.ToString().Contains("expired"))
                {
                    response.Message = "The session token is expired!";
                }
                else
                {
                    response.Message = ex.Message.ToString();
                }
                response.IsVallid = false;
                return response;
            }
        }
         
        public ValidTokenResponse ValidateTokenV2(string token)
        {
            ValidTokenResponse response = new ValidTokenResponse();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,  // Set to true if issuer validation is required
                ValidateAudience = false,  // Set to true if audience validation is required
                ClockSkew = TimeSpan.Zero  // No tolerance for expiration time
            };

            try
            {
                SecurityToken validatedToken;
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                var logindClaim = claimsPrincipal.FindFirst("loginProvider");
                var usernameClaim = claimsPrincipal.FindFirst("user_name");

                if (logindClaim != null)
                {
                    response.LoginProviderId = logindClaim.Value;
                }                
                if (usernameClaim != null)
                {
                    response.UserName = usernameClaim.Value;
                }                
                response.IsVallid = true;
                return response;
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("expired"))
                {
                    response.Message = "The session token is expired!";
                }
                else
                {
                    response.Message = ex.Message.ToString();
                }
                response.IsVallid = false;
                return response;
            }
        }
    }
}
