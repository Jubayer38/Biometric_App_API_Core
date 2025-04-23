using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Entity.ENUM;
using BIA.Entity.Utility;

namespace BIA.Common
{
    public class ApiManager
    {
        private readonly BLLCommon _bLLCommon;
        private readonly BLLUserAuthenticaion _bua;
        private readonly BllOrderBssService _bssService;

        public ApiManager(BLLCommon bLLCommon, BLLUserAuthenticaion bua, BllOrderBssService bssService)
        {
            _bLLCommon = bLLCommon;
            _bua = bua;
            _bssService = bssService;
        }
        internal async Task<bool> ValidUserBySecurityToken(string securityToken)
        {
            bool result = false;
            try
            {
                if (!_bLLCommon.CheckSecurityTokenFormat(securityToken))
                {
                    return false;
                }
                string decriptedSecurityToken = Cryptography.Decrypt(securityToken, true);
                string loginProvider = _bLLCommon.GetDataFromSecurityToken(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValid(loginProvider, _bLLCommon.GetDataFromSecurityToken(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.deviceId));

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal async Task<bool> ValidUserBySecurityTokenV2(string securityToken)
        {
            bool result = false;
            try
            {
                string decriptedSecurityToken = string.Empty;
                string decriptedSecurityTokenMD5 = string.Empty;
                try
                {
                    decriptedSecurityToken = AESCryptography.Decrypt(securityToken);
                    if (decriptedSecurityToken.Equals("InvalidSessionToken"))
                    {
                        decriptedSecurityToken = string.Empty;
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(securityToken, true);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(securityToken, true);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(decriptedSecurityTokenMD5))
                {
                    if (!_bLLCommon.CheckSecurityTokenFormatV3(decriptedSecurityTokenMD5))
                    {
                        return false;
                    }
                    result = await validateSequrityTokenForMD5(decriptedSecurityTokenMD5);
                }
                else
                {
                    if (!_bLLCommon.CheckSecurityTokenFormatV2(decriptedSecurityToken))
                    {
                        return false;
                    }
                    result = await validateSequrityTokenForAES(decriptedSecurityToken);
                }

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal async Task<bool> validateSequrityTokenForAES(string decriptedSecurityToken)
        {
            bool result = false;
            try
            {
                string loginProvider = _bLLCommon.GetDataFromSecurityTokenV2(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValid2(loginProvider, _bLLCommon.GetDataFromSecurityTokenV2(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.deviceId));
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal async Task<bool> validateSequrityTokenForMD5(string decriptedSecurityTokenMD5)
        {
            bool result = false;
            try
            {
                string loginProvider = _bLLCommon.GetDataFromSecurityTokenV3(decriptedSecurityTokenMD5, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValid2(loginProvider, _bLLCommon.GetDataFromSecurityTokenV3(decriptedSecurityTokenMD5, (int)EnumSecurityTokenPropertyIndex.deviceId));
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        internal async Task<bool> validateSequrityTokenForAESForBP(string decriptedSecurityToken)
        {
            bool result = false;
            try
            {
                string loginProvider = _bLLCommon.GetDataFromSecurityTokenV2(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValidForBPLogin(loginProvider, _bLLCommon.GetDataFromSecurityTokenV2(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.deviceId));

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        internal async Task<bool> validateSequrityTokenForMD5ForBP(string decriptedSecurityTokenMD5)
        {
            bool result = false;
            try
            {
                string loginProvider = _bLLCommon.GetDataFromSecurityTokenV3(decriptedSecurityTokenMD5, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValidForBPLogin(loginProvider, _bLLCommon.GetDataFromSecurityTokenV3(decriptedSecurityTokenMD5, (int)EnumSecurityTokenPropertyIndex.deviceId));
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal async Task<bool> ValidUserBySecurityTokenForBPLogin(string securityToken)
        {
            bool result = false;
            try
            {
                string decriptedSecurityToken = string.Empty;
                string decriptedSecurityTokenMD5 = string.Empty;
                try
                {
                    decriptedSecurityToken = AESCryptography.Decrypt(securityToken);
                    if (decriptedSecurityToken.Equals("InvalidSessionToken"))
                    {
                        decriptedSecurityToken = string.Empty;
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(securityToken, true);
                    }

                }
                catch (Exception)
                {
                    try
                    {
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(securityToken, true);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(decriptedSecurityTokenMD5))
                {
                    if (!_bLLCommon.CheckSecurityTokenFormatV3(decriptedSecurityTokenMD5))
                    {
                        return false;
                    }
                    result = await validateSequrityTokenForMD5ForBP(decriptedSecurityTokenMD5);
                }
                else
                {
                    if (!_bLLCommon.CheckSecurityTokenFormatV2(decriptedSecurityToken))
                    {
                        return false;
                    }
                    result = await validateSequrityTokenForAESForBP(decriptedSecurityToken);
                }

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //===============DBSSLogin================
        internal async Task<bool> ValidUserBySecurityTokenForDBSS(string securityToken)
        {
            bool result = false;
            try
            {
                string loginProvider = _bLLCommon.GetDataFromSecurityToken(Cryptography.Decrypt(securityToken, true), (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValidForDBSS(loginProvider);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal async Task<bool> ValidUserBySecurityTokenForDBSSV2(string securityToken)
        {
            bool result = false;
            string loginProvider = string.Empty;
            try
            {
                string decriptedSecurityToken = string.Empty;
                string decriptedSecurityTokenMD5 = string.Empty;
                try
                {
                    decriptedSecurityToken = AESCryptography.Decrypt(securityToken);
                    if (decriptedSecurityToken.Equals("InvalidSessionToken"))
                    {
                        decriptedSecurityToken = string.Empty;
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(securityToken, true);
                    }

                }
                catch (Exception)
                {
                    try
                    {
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(securityToken, true);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                if (!String.IsNullOrEmpty(decriptedSecurityTokenMD5))
                {
                    loginProvider = _bLLCommon.GetDataFromSecurityToken(decriptedSecurityTokenMD5, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                    result = await _bua.IsSecurityTokenValidForDBSS(loginProvider);
                    return result;
                }
                else
                {
                    loginProvider = _bLLCommon.GetDataFromSecurityToken(decriptedSecurityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                    result = await _bua.IsSecurityTokenValidForDBSS(loginProvider);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //=========x=================

        private static string getLoginProvider(string token)
        {
            return token.Substring(0, token.IndexOf(",uid:"));
        }




        internal async Task<bool> ValidUserBySecurityToken_Test(string securityToken)
        {
            bool result = false;
            try
            {
                if (!_bLLCommon.CheckSecurityTokenFormat(securityToken))
                {
                    return false;
                }

                string loginProvider = _bLLCommon.GetDataFromSecurityToken(securityToken, (int)EnumSecurityTokenPropertyIndex.LoginProvider);
                result = await _bua.IsSecurityTokenValid2(loginProvider, _bLLCommon.GetDataFromSecurityToken(securityToken, (int)EnumSecurityTokenPropertyIndex.deviceId));

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal async Task<string> GetBtsInfoByLacCid(int lac, int cid)
        {
            string BtsCode = string.Empty;
            try
            {
                BtsCode = await _bssService.GetBTSInfoByLacCid(lac, cid);
            }
            catch (Exception ex)
            {
                throw;
            }
            return BtsCode;
        }
    }

}
