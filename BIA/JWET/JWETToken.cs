using BIA.Entity.Collections;
using BIA.Entity.ViewModel;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BIA.JWET
{
    public class JWETToken
    {
        public string GenerateJWETToken(string itopUpNumber, string retailerCode, string deviceId, string loginProvider, string userId, int timeAddSubstract)
        {
            string tokenSignK = "pKbotujPftemhsd7ummFE4iYg6VxIvEUKBn65qVB3DFMytF1tLo29sKZZvjfgegTu2dDuAJ3zK3KPdJuYDYEjzjtkpOxoM5kQYURRFDmDP0EqKObvFDmQuUwx4sjTT75";
            string tokenEncK = "OfOPD1UGH4mtisyv8I25QI4ZCIFPKsqJ";
            string issuer = "banglalink.net";
            string audience = "bl_retailers";
            var jweToken = string.Empty;

            var tokenModel = new JwtTokenModel
            {
                ITopUpNumber = itopUpNumber,
                RetailerCode = retailerCode,
                DeviceId = deviceId,
                LoginProvider = loginProvider,
                UserId = userId
            };

            try
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSignK));
                var encryptingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenEncK));

                var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                var encryptingCredentials = new EncryptingCredentials(encryptingKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);

                if (SettingsValues.GetIsRetailerAPICore() == 1)
                {
                    var claims = new[]
                    {
                        new Claim("retailerCode", tokenModel.RetailerCode),
                        new Claim("dvicId", tokenModel.DeviceId),
                        new Claim("userId", tokenModel.UserId),
                        new Claim(JwtRegisteredClaimNames.Jti, tokenModel.LoginProvider)
                    };

                    DateTime currentDateTime = DateTime.Now;
                    DateTime expirationTime = currentDateTime.Date.AddHours(24);

                    DateTime StartTime = currentDateTime.Date.AddMinutes(timeAddSubstract);

                    var jwtTokenHandler = new JwtSecurityTokenHandler();
                    jweToken = jwtTokenHandler.CreateEncodedJwt(new SecurityTokenDescriptor
                    {
                        Issuer = issuer,
                        Audience = audience,
                        EncryptingCredentials = encryptingCredentials,
                        SigningCredentials = credentials,
                        Subject = new ClaimsIdentity(claims),
                        Expires = expirationTime,
                        NotBefore = StartTime,
                    });
                }
                else
                {
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.UniqueName, tokenModel.RetailerCode),
                        new Claim("dvicId", tokenModel.DeviceId),
                        new Claim("userId", tokenModel.UserId),
                        new Claim(JwtRegisteredClaimNames.Jti, tokenModel.LoginProvider)
                    };

                    DateTime currentDateTime = DateTime.Now;
                    DateTime expirationTime = currentDateTime.Date.AddHours(24);

                    DateTime StartTime = currentDateTime.Date.AddMinutes(timeAddSubstract);

                    var jwtTokenHandler = new JwtSecurityTokenHandler();
                    jweToken = jwtTokenHandler.CreateEncodedJwt(new SecurityTokenDescriptor
                    {
                        Issuer = issuer,
                        Audience = audience,
                        EncryptingCredentials = encryptingCredentials,
                        SigningCredentials = credentials,
                        Subject = new ClaimsIdentity(claims),
                        Expires = expirationTime,
                        NotBefore = StartTime,
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return jweToken;
        }
    }
}
