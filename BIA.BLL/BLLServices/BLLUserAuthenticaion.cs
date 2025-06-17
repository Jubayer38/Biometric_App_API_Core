using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using BIA.Entity.ViewModel;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace BIA.BLL.BLLServices
{
    public class BLLUserAuthenticaion
    {
        private readonly DALBiometricRepo _dataManager;
        private readonly IConfiguration _configuration;
        public BLLUserAuthenticaion(DALBiometricRepo dataManager, IConfiguration configuration)
        {
            _dataManager = dataManager;
            _configuration = configuration;
        }
        public async Task<int> GetUserAPIVersion(APIVersionRequest model)
        {
            int result = 0;
            try
            {
                result = await _dataManager.GetUserAPIVersion(new APIVersionRequest()
                {
                    username = model.username,
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }      

        public async Task<APIVersionResponseWithAppUpdateCheck> GetUserAPIVersionWithAppUpdateCheck(APIVersionRequestWithAppUpdateCheck model)
        {
            //APIVersionResponse mainObj = new APIVersionResponse();
            APIVersionResponseWithAppUpdateCheck respObj = new APIVersionResponseWithAppUpdateCheck();
            DataTable resultSet = null;
            try
            {
                resultSet = await _dataManager.GetUserAPIVersionWithAppUpdateCheck(new VMAPIVersionRequestWithAppUpdateCheck()
                {
                    username = model.username,
                    version_code = Convert.ToString(model.appVersion)
                });

                if (resultSet.Rows.Count > 0)
                {
                    if (Convert.ToInt16(resultSet.Rows[0]["CURRENT_APP_VERSION"] == DBNull.Value ? null : resultSet.Rows[0]["CURRENT_APP_VERSION"]) > model.appVersion)
                    {
                        respObj.app_update_info.is_update_exists = true;
                    }
                    else
                    {
                        respObj.app_update_info.is_update_exists = false;
                    }

                    respObj.app_update_info.update_url = Convert.ToString(resultSet.Rows[0]["UPDATE_URL"] == DBNull.Value ? null : resultSet.Rows[0]["UPDATE_URL"]);
                    respObj.app_update_info.is_update_mandatory = Convert.ToInt16(resultSet.Rows[0]["IS_UPDATE_MANDATORY"] == DBNull.Value ? null : resultSet.Rows[0]["IS_UPDATE_MANDATORY"]);
                    respObj.result = true;
                    respObj.message = MessageCollection.Success;
                }
                else
                {
                    respObj.result = false;
                    respObj.message = MessageCollection.NoDataFound;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return respObj;
        }

        public async Task<APIVersionResponseWithAppUpdateCheckRev> GetUserAPIVersionWithAppUpdateCheckV2(APIVersionRequestWithAppUpdateCheck model)
        {
            APIVersionResponseWithAppUpdateCheckRev respObj = new APIVersionResponseWithAppUpdateCheckRev();
            DataTable resultSet = null;
            try
            {
                resultSet = await _dataManager.GetUserAPIVersionWithAppUpdateCheck(new VMAPIVersionRequestWithAppUpdateCheck()
                {
                    username = model.username,
                    version_code = Convert.ToString(model.appVersion)
                });

                if (resultSet.Rows.Count > 0)
                {
                    if (Convert.ToInt16(resultSet.Rows[0]["CURRENT_APP_VERSION"] == DBNull.Value ? null : resultSet.Rows[0]["CURRENT_APP_VERSION"]) > model.appVersion)
                    {
                        respObj.data = new AppUpdateInfoV2()
                        {
                            is_update_exists = true
                        };
                    }
                    else
                    {
                        respObj.data = new AppUpdateInfoV2()
                        {
                            is_update_exists = false
                        };
                    }

                    respObj.data.update_url = Convert.ToString(resultSet.Rows[0]["UPDATE_URL"] == DBNull.Value ? null : resultSet.Rows[0]["UPDATE_URL"]);
                    respObj.data.is_update_mandatory = Convert.ToInt16(resultSet.Rows[0]["IS_UPDATE_MANDATORY"] == DBNull.Value ? null : resultSet.Rows[0]["IS_UPDATE_MANDATORY"]);
                    respObj.isError = false;
                    respObj.message = MessageCollection.Success;
                }
                else
                {
                    respObj.isError = true;
                    respObj.message = MessageCollection.NoDataFound;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return respObj;
        }


        public async Task<LoginUserInfoResponse> ValidateUser(string username, string password)
        {
            LoginUserInfoResponse loginUserInfo = new LoginUserInfoResponse();
            try
            {
                vmUserInfo logInInfo = new vmUserInfo()
                {
                    user_name = username,
                    password = password
                };
                DataTable dataRow = await _dataManager.ValidateUser(logInInfo);
                if (dataRow.Rows.Count > 0)
                {
                    loginUserInfo.user_id = Convert.ToString(dataRow.Rows[0]["USER_ID"] ?? null);
                    loginUserInfo.user_name = Convert.ToString(dataRow.Rows[0]["USER_NAME"] ?? null);
                    loginUserInfo.role_id = Convert.ToString(dataRow.Rows[0]["ROLE_ID"] ?? null);
                    loginUserInfo.role_name = Convert.ToString(dataRow.Rows[0]["ROLE_NAME"] ?? null);
                    loginUserInfo.is_role_active = Convert.ToInt32(dataRow.Rows[0]["IS_ROLE_ACTIVE"] ?? 0);
                    loginUserInfo.channel_id = Convert.ToInt32(dataRow.Rows[0]["CHANNEL_ID"] ?? 0);
                    loginUserInfo.channel_name = Convert.ToString(dataRow.Rows[0]["CHANNEL_NAME"] ?? null);
                    loginUserInfo.is_activedirectory_user = Convert.ToInt32(dataRow.Rows[0]["IS_ACTIVEDIRECTORY_USER"] ?? 0);
                    loginUserInfo.role_access = Convert.ToString(dataRow.Rows[0]["ROLEACCESS"] ?? null);
                    loginUserInfo.distributor_code = Convert.ToString(dataRow.Rows[0]["DIST_CODE"] ?? null);
                    loginUserInfo.inventory_id = Convert.ToInt32(dataRow.Rows[0]["INVENTORY_ID"] ?? null);
                    loginUserInfo.center_code = Convert.ToString(dataRow.Rows[0]["CENTER_CODE"] ?? null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }


        public async Task<ResellerLoginUserInfoResponse> ValidateUser(string username)
        {
            ResellerLoginUserInfoResponse resellerLoginUserInfo = new ResellerLoginUserInfoResponse();
            try
            {
                DataTable dataRow = await _dataManager.ValidateUser(username);
                if (dataRow.Rows.Count > 0)
                {
                    resellerLoginUserInfo.user_id = Convert.ToString(dataRow.Rows[0]["USER_ID"] ?? null);
                    resellerLoginUserInfo.user_name = Convert.ToString(dataRow.Rows[0]["USER_NAME"] ?? null);
                    resellerLoginUserInfo.password = Cryptography.Decrypt(Convert.ToString(dataRow.Rows[0]["PASSWORDHASH"] ?? null), true);
                    resellerLoginUserInfo.role_id = Convert.ToString(dataRow.Rows[0]["ROLE_ID"] ?? null);
                    resellerLoginUserInfo.role_name = Convert.ToString(dataRow.Rows[0]["ROLE_NAME"] ?? null);
                    resellerLoginUserInfo.is_role_active = Convert.ToInt32(dataRow.Rows[0]["IS_ROLE_ACTIVE"] ?? 0);
                    resellerLoginUserInfo.channel_id = Convert.ToInt32(dataRow.Rows[0]["CHANNEL_ID"] ?? 0);
                    resellerLoginUserInfo.channel_name = Convert.ToString(dataRow.Rows[0]["CHANNEL_NAME"] ?? null);
                    resellerLoginUserInfo.is_activedirectory_user = Convert.ToInt32(dataRow.Rows[0]["IS_ACTIVEDIRECTORY_USER"] ?? 0);
                    resellerLoginUserInfo.role_access = Convert.ToString(dataRow.Rows[0]["ROLEACCESS"] ?? null);
                    resellerLoginUserInfo.distributor_code = Convert.ToString(dataRow.Rows[0]["DIST_CODE"] ?? null);
                    resellerLoginUserInfo.inventory_id = Convert.ToInt32(dataRow.Rows[0]["INVENTORY_ID"] ?? null);
                    resellerLoginUserInfo.center_code = Convert.ToString(dataRow.Rows[0]["CENTER_CODE"] ?? null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resellerLoginUserInfo;
        }
         
        public async Task<LoginUserInfoResponseRev> ValidateUserV2(LoginRequestsV2 userModel, string username, string password)
        {
            LoginUserInfoResponseRev loginUserInfo = new LoginUserInfoResponseRev();
            try
            {
                vmUserInfo logInInfo = new vmUserInfo()
                {
                    user_name = username,
                    password = password
                };
                DataTable dataRow = await _dataManager.ValidateUserV2(userModel,logInInfo);
                if (dataRow.Rows.Count > 0)
                {
                    loginUserInfo.user_id = Convert.ToString(dataRow.Rows[0]["USER_ID"] ?? "");
                    loginUserInfo.user_name = Convert.ToString(dataRow.Rows[0]["USER_NAME"] ?? "");
                    loginUserInfo.role_id = Convert.ToString(dataRow.Rows[0]["ROLE_ID"] ?? "");
                    loginUserInfo.role_name = Convert.ToString(dataRow.Rows[0]["ROLE_NAME"] ?? "");
                    loginUserInfo.is_role_active = Convert.ToInt32(dataRow.Rows[0]["IS_ROLE_ACTIVE"] == DBNull.Value ? 0 : dataRow.Rows[0]["IS_ROLE_ACTIVE"]); 
                    loginUserInfo.channel_id = Convert.ToInt32(dataRow.Rows[0]["CHANNEL_ID"] == DBNull.Value ? 0 : dataRow.Rows[0]["CHANNEL_ID"]);
                    loginUserInfo.channel_name = Convert.ToString(dataRow.Rows[0]["CHANNEL_NAME"] ?? "");
                    loginUserInfo.is_activedirectory_user = Convert.ToInt32(dataRow.Rows[0]["IS_ACTIVEDIRECTORY_USER"] == DBNull.Value ? 0 : dataRow.Rows[0]["IS_ACTIVEDIRECTORY_USER"]); 
                    loginUserInfo.role_access = Convert.ToString(dataRow.Rows[0]["ROLEACCESS"] ?? "");
                    loginUserInfo.distributor_code = Convert.ToString(dataRow.Rows[0]["DIST_CODE"] ?? "");
                    loginUserInfo.inventory_id = Convert.ToInt32(dataRow.Rows[0]["INVENTORY_ID"] == DBNull.Value ? 0 : dataRow.Rows[0]["INVENTORY_ID"]);
                    loginUserInfo.center_code = Convert.ToString(dataRow.Rows[0]["CENTER_CODE"] ?? "");
                    loginUserInfo.itopUpNumber = Convert.ToString(dataRow.Rows[0]["MOBILE_NUMBER"] ?? "");
                    loginUserInfo.distributor_code = Convert.ToString(dataRow.Rows[0]["DIST_CODE"] ?? "");
                    loginUserInfo.is_default_Password = Convert.ToInt32(dataRow.Rows[0]["ISDEFAULTPASSWORD"] == DBNull.Value ? 0 : dataRow.Rows[0]["ISDEFAULTPASSWORD"]);
                    loginUserInfo.ExpiredDate = loginUserInfo.is_default_Password == 1 ? Convert.ToString(dataRow.Rows[0]["EXPIRED_DATE"]) : "";
                    loginUserInfo.isValidUser = Convert.ToInt32(dataRow.Rows[0]["IS_USER_VALID"] == DBNull.Value ? 0 : dataRow.Rows[0]["IS_USER_VALID"]); ///Convert.ToInt32(dataRow.Rows[0]["IS_USER_VALID"] ?? 0); 
                    loginUserInfo.message = Convert.ToString(dataRow.Rows[0]["MESSAGE"] ??""); 
                    loginUserInfo.designation = Convert.ToString(dataRow.Rows[0]["DESIGNATION"] ??""); 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }

        public async Task<LoginUserInfoResponseRev> ValidateUserV3(FPValidationReqModel userModel)
        {
            LoginUserInfoResponseRev loginUserInfo = new LoginUserInfoResponseRev();
            try
            {
                DataTable dataRow = await _dataManager.ValidateUserV3(userModel);
                if (dataRow.Rows.Count > 0)
                {
                    loginUserInfo.user_id = Convert.ToString(dataRow.Rows[0]["USER_ID"] ?? "");
                    loginUserInfo.user_name = Convert.ToString(dataRow.Rows[0]["USER_NAME"] ?? "");
                    loginUserInfo.role_id = Convert.ToString(dataRow.Rows[0]["ROLE_ID"] ?? "");
                    loginUserInfo.role_name = Convert.ToString(dataRow.Rows[0]["ROLE_NAME"] ?? "");
                    loginUserInfo.is_role_active = Convert.ToInt32(dataRow.Rows[0]["IS_ROLE_ACTIVE"] == DBNull.Value ? 0 : dataRow.Rows[0]["IS_ROLE_ACTIVE"]);
                    loginUserInfo.channel_id = Convert.ToInt32(dataRow.Rows[0]["CHANNEL_ID"] == DBNull.Value ? 0 : dataRow.Rows[0]["CHANNEL_ID"]);
                    loginUserInfo.channel_name = Convert.ToString(dataRow.Rows[0]["CHANNEL_NAME"] ?? "");
                    loginUserInfo.is_activedirectory_user = Convert.ToInt32(dataRow.Rows[0]["IS_ACTIVEDIRECTORY_USER"] == DBNull.Value ? 0 : dataRow.Rows[0]["IS_ACTIVEDIRECTORY_USER"]);
                    loginUserInfo.role_access = Convert.ToString(dataRow.Rows[0]["ROLEACCESS"] ?? "");
                    loginUserInfo.distributor_code = Convert.ToString(dataRow.Rows[0]["DIST_CODE"] ?? "");
                    loginUserInfo.inventory_id = Convert.ToInt32(dataRow.Rows[0]["INVENTORY_ID"] == DBNull.Value ? 0 : dataRow.Rows[0]["INVENTORY_ID"]);
                    loginUserInfo.center_code = Convert.ToString(dataRow.Rows[0]["CENTER_CODE"] ?? "");
                    loginUserInfo.itopUpNumber = Convert.ToString(dataRow.Rows[0]["MOBILE_NUMBER"] ?? "");
                    loginUserInfo.distributor_code = Convert.ToString(dataRow.Rows[0]["DIST_CODE"] ?? "");
                    loginUserInfo.is_default_Password = Convert.ToInt32(dataRow.Rows[0]["ISDEFAULTPASSWORD"] == DBNull.Value ? 0 : dataRow.Rows[0]["ISDEFAULTPASSWORD"]);
                    loginUserInfo.ExpiredDate = loginUserInfo.is_default_Password == 1 ? Convert.ToString(dataRow.Rows[0]["EXPIRED_DATE"]) : "";
                    loginUserInfo.isValidUser = Convert.ToInt32(dataRow.Rows[0]["IS_USER_VALID"] == DBNull.Value ? 0 : dataRow.Rows[0]["IS_USER_VALID"]); ///Convert.ToInt32(dataRow.Rows[0]["IS_USER_VALID"] ?? 0); 
                    loginUserInfo.message = Convert.ToString(dataRow.Rows[0]["MESSAGE"] ?? "");
                    loginUserInfo.designation = Convert.ToString(dataRow.Rows[0]["DESIGNATION"] ?? "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }

        //public async Task<int> IsUserValid(string userName)
        //{
        //    return await _dua.IsUserValid(userName);
        //}

        public async Task<long> SaveLoginAtmInfo(UserLogInAttempt loginInfo)
        {
            long response = 0;
            try
            {
                response = await _dataManager.SaveLoginAtmInfo(loginInfo);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<long> SaveLoginAtmInfoV2(UserLogInAttemptV2 loginInfo)
        {
            long response = 0;
            try
            {
                response = await _dataManager.SaveLoginAtmInfoV2(loginInfo);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //===========DBSSLogin==============
        public async Task<bool> IsSecurityTokenValidForDBSS(string loginProvider)
        {
            long response = 0;
            try
            {
                response =await _dataManager.IsSecurityTokenValid(loginProvider);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }
        //========================x=====================



        public async Task<bool> IsSecurityTokenValid(string loginProvider, string deviceId)
        {
            long response = 0;
            try
            {
                response =await _dataManager.IsSecurityTokenValid2(loginProvider, deviceId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }

        public async Task<bool> IsSecurityTokenValid2(string loginProvider, string deviceId)
        {
            long response = 0;
            try
            {
                response = await _dataManager.IsSecurityTokenValidV3(loginProvider, deviceId);
                LoginProviderInfo.login_attempt_id = response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response > 0 ? true : false;
        }

        public async Task<bool> IsSecurityTokenValidForBPLogin(string loginProvider, string deviceId)
        {
            long response = 0;
            try
            {
                response = await _dataManager.IsSecurityTokenValidForBPLogin(loginProvider, deviceId);
                LoginProviderInfo.login_attempt_id = response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response > 0 ? true : false;
        }



        public async Task<RACommonResponse> ChangePassword(ChangePasswordRequests model)
        {
            RACommonResponse response = new RACommonResponse();
            int result = 0;
            //var a = Cryptography.Encrypt(model.old_password, true);//test
            try
            {
                VMChangePassword vmCpObj = new VMChangePassword()
                {
                    username = model.user_id,
                    old_password = Cryptography.Encrypt(model.old_password, true),
                    new_password = Cryptography.Encrypt(model.new_password, true)
                };

                result = await _dataManager.ChangePassword(vmCpObj);

                if (result > 0)
                {
                    response.result = true;
                    response.message = "Password Changed Successfully, You will be automatically logged out now. Please login using new password.";
                    return response;
                }
                else if (result == -888/*(int)ChangePasswordEnum.invalidUser*/)
                {
                    response.result = false;
                    response.message = "Invalid user name.";
                    return response;
                }
                else if (result == -777/*(int)ChangePasswordEnum.passwordNotMatched*/)
                {
                    response.result = false;
                    response.message = "Password not matched.";
                    return response;
                }
                else if (result == -999/*(int)ChangePasswordEnum.unableToUpdate*/)
                {
                    response.result = false;
                    response.message = "Unable to update password.";
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = "Something wrong happend.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RACommonResponse> ChangePasswordV2(ChangePasswordRequests model)
        {
            RACommonResponse response = new RACommonResponse();
            int result = 0;
            try
            {
                VMChangePassword vmCpObj = new VMChangePassword();
                int isEligible = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IsEligibleAES"]);
                if (isEligible == 1)
                {
                    bool isEligibleUser = await IsAESEligibleUser(model.user_id);
                    if (isEligibleUser == true)
                    {
                        vmCpObj = new VMChangePassword()
                        {
                            username = model.user_id,
                            old_password = AESCryptography.Encrypt(model.old_password),
                            new_password = AESCryptography.Encrypt(model.new_password)
                        };
                    }
                    else
                    {
                        vmCpObj = new VMChangePassword()
                        {
                            username = model.user_id,
                            old_password = Cryptography.Encrypt(model.old_password, true),
                            new_password = Cryptography.Encrypt(model.new_password, true)
                        };
                    }
                }
                else
                {
                    vmCpObj = new VMChangePassword()
                    {
                        username = model.user_id,
                        old_password = AESCryptography.Encrypt(model.old_password),
                        new_password = AESCryptography.Encrypt(model.new_password)
                    };
                }

                result = await _dataManager.ChangePasswordV2(vmCpObj);

                if (result > 0)
                {
                    response.result = true;
                    response.message = "Password Changed Successfully, You will be automatically logged out now. Please login using new password.";
                    return response;
                }
                else if (result == -888/*(int)ChangePasswordEnum.invalidUser*/)
                {
                    response.result = false;
                    response.message = "Invalid user name.";
                    return response;
                }
                else if (result == -777/*(int)ChangePasswordEnum.passwordNotMatched*/)
                {
                    response.result = false;
                    response.message = "Password not matched.";
                    return response;
                }
                else if (result == -999/*(int)ChangePasswordEnum.unableToUpdate*/)
                {
                    response.result = false;
                    response.message = "Unable to update password.";
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = "Something wrong happend.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<RACommonResponseRevamp> ChangePasswordV3(ChangePasswordRequests model)
        {
            RACommonResponseRevamp response = new RACommonResponseRevamp();
            int result = 0;
            //var a = Cryptography.Encrypt(model.old_password, true);//test
            try
            {
                VMChangePassword vmCpObj = new VMChangePassword();
                int isEligible = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IsEligibleAES"]);
                if (isEligible == 1)
                {
                    bool isEligibleUser = await IsAESEligibleUser(model.user_id);
                    if (isEligibleUser == true)
                    {
                        vmCpObj = new VMChangePassword()
                        {
                            username = model.user_id,
                            old_password = AESCryptography.Encrypt(model.old_password),
                            new_password = AESCryptography.Encrypt(model.new_password)
                        };
                    }
                    else
                    {
                        vmCpObj = new VMChangePassword()
                        {
                            username = model.user_id,
                            old_password = Cryptography.Encrypt(model.old_password, true),
                            new_password = Cryptography.Encrypt(model.new_password, true)
                        };
                    }
                }
                else
                {
                    vmCpObj = new VMChangePassword()
                    {
                        username = model.user_id,
                        old_password = AESCryptography.Encrypt(model.old_password),
                        new_password = AESCryptography.Encrypt(model.new_password)
                    };
                }

                result = await _dataManager.ChangePasswordV3(vmCpObj);

                if (result > 0)
                {
                    response.isError = false;
                    response.message = "Password Changed Successfully, You will be automatically logged out now. Please login using new password.";
                    return response;
                }
                else if (result == -888/*(int)ChangePasswordEnum.invalidUser*/)
                {
                    response.isError = true;
                    response.message = "Invalid user name.";
                    return response;
                }
                else if (result == -777/*(int)ChangePasswordEnum.passwordNotMatched*/)
                {
                    response.isError = true;
                    response.message = "Password not matched.";
                    return response;
                }
                else if (result == -999/*(int)ChangePasswordEnum.unableToUpdate*/)
                {
                    response.isError = true;
                    response.message = "Unable to update password.";
                    return response;
                }
                else
                {
                    response.isError = true;
                    response.message = "Something wrong happend.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RACommonResponseRevamp> ChangePasswordV4(ChangePasswordRequests model)
        {
            RACommonResponseRevamp response = new RACommonResponseRevamp();
            int result = 0;
            int isEligible = 0;
            try
            {
                try
                {
                    isEligible = Convert.ToInt32(_configuration.GetSection("AppSettings:IsEligibleAES").Value);
                }
                catch (Exception)
                {

                    throw;
                }
                VMChangePassword vmCpObj = new VMChangePassword();

                if (isEligible == 1)
                {
                    bool isEligibleUser = await IsAESEligibleUser(model.user_id);
                    if (isEligibleUser == true)
                    {
                        vmCpObj = new VMChangePassword()
                        {
                            username = model.user_id,
                            old_password = AESCryptography.Encrypt(model.old_password),
                            new_password = AESCryptography.Encrypt(model.new_password)
                        };
                    }
                    else
                    {
                        vmCpObj = new VMChangePassword()
                        {
                            username = model.user_id,
                            old_password = Cryptography.Encrypt(model.old_password, true),
                            new_password = Cryptography.Encrypt(model.new_password, true)
                        };
                    }
                }
                else
                {
                    vmCpObj = new VMChangePassword()
                    {
                        username = model.user_id,
                        old_password = AESCryptography.Encrypt(model.old_password),
                        new_password = AESCryptography.Encrypt(model.new_password)
                    };
                }

                result = await _dataManager.ChangePasswordV3(vmCpObj);

                if (result > 0)
                {
                    response.isError = false;
                    response.message = "Password Changed Successfully, You will be automatically logged out now. Please login using new password.";
                    return response;
                }
                else if (result == -888/*(int)ChangePasswordEnum.invalidUser*/)
                {
                    response.isError = true;
                    response.message = "Invalid user name.";
                    return response;
                }
                else if (result == -777/*(int)ChangePasswordEnum.passwordNotMatched*/)
                {
                    response.isError = true;
                    response.message = "Password not matched.";
                    return response;
                }
                else if (result == -999/*(int)ChangePasswordEnum.unableToUpdate*/)
                {
                    response.isError = true;
                    response.message = "Unable to update password.";
                    return response;
                }
                else
                {
                    response.isError = true;
                    response.message = "Something wrong happend.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RAPassLenResponse> GetPasswordLength()
        {
            RAPassLenResponse response = new RAPassLenResponse();
            int passLen = 0;
            try
            {
                var dataRow = await _dataManager.GetPasswordLength();

                if (dataRow.Rows.Count > 0)
                {
                    passLen = Convert.ToInt32(dataRow.Rows[0]["RA_PASSWORD_LENGTH"] == DBNull.Value ? null : dataRow.Rows[0]["RA_PASSWORD_LENGTH"]);
                }

                if (passLen > 0)
                {
                    response.length = passLen;
                    response.result = true;
                    response.message = "Success!";
                    return response;
                }
                else
                {
                    response.length = 0;
                    response.result = false;
                    response.message = "No data found!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RAPassLenResponse> GetPasswordLengthV2()
        {
            RAPassLenResponse response = new RAPassLenResponse();
            int passLen = 0;
            try
            {
                var dataRow = await _dataManager.GetPasswordLengthV2();

                if (dataRow.Rows.Count > 0)
                {
                    passLen = Convert.ToInt32(dataRow.Rows[0]["RA_PASSWORD_LENGTH"] == DBNull.Value ? null : dataRow.Rows[0]["RA_PASSWORD_LENGTH"]);
                }

                if (passLen > 0)
                {
                    response.length = passLen;
                    response.result = true;
                    response.message = "Success!";
                    return response;
                }
                else
                {
                    response.length = 0;
                    response.result = false;
                    response.message = "No data found!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<VMUserMobileNoAndOTP> GetUserMobileNoAndNewPWD(string userName)
        {
            VMUserMobileNoAndOTP respObj = new VMUserMobileNoAndOTP();
            try
            {
                var generatePWDResult = await generatePWD();
                if (generatePWDResult.Item2 == false)
                {
                    respObj.result = false;
                    respObj.message = generatePWDResult.Item3;
                    return respObj;
                }

                var dataRow = await _dataManager.GetUserMobileNoAndOTP(userName);

                if (dataRow.Rows.Count > 0)
                {
                    respObj.user_id = dataRow.Rows[0]["USERID"] == DBNull.Value ? 0 : Convert.ToInt64(dataRow.Rows[0]["USERID"]);
                    respObj.mobile_no = dataRow.Rows[0]["MOBILE_NUMBER"] == DBNull.Value ? null : (string)dataRow.Rows[0]["MOBILE_NUMBER"];
                    respObj.PWD = generatePWDResult.Item1;
                    respObj.result = true;
                    respObj.message = MessageCollection.Success;
                }
                else
                {
                    respObj.result = false;
                    respObj.message = MessageCollection.InvalidUserName;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return respObj;
        }

        public async Task<VMUserMobileNoAndOTP> GetUserMobileNoAndNewPWDV2(string userName)
        {
            VMUserMobileNoAndOTP respObj = new VMUserMobileNoAndOTP();
            try
            {
                var generatePWDResult = await generatePWDV2();
                if (generatePWDResult.Item2 == false)
                {
                    respObj.result = false;
                    respObj.message = generatePWDResult.Item3;
                    return respObj;
                }

                var dataRow = await _dataManager.GetUserMobileNoAndOTPV2(userName);

                if (dataRow.Rows.Count > 0)
                {
                    respObj.user_id = dataRow.Rows[0]["USERID"] == DBNull.Value ? 0 : Convert.ToInt64(dataRow.Rows[0]["USERID"]);
                    respObj.mobile_no = dataRow.Rows[0]["MOBILE_NUMBER"] == DBNull.Value ? null : (string)dataRow.Rows[0]["MOBILE_NUMBER"];
                    respObj.PWD = generatePWDResult.Item1;
                    respObj.result = true;
                    respObj.message = MessageCollection.Success;
                }
                else
                {
                    respObj.result = false;
                    respObj.message = MessageCollection.InvalidUserName;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return respObj;
        }

        /// <summary>
        /// Generate new random PWD in code level.
        /// </summary>
        /// <returns>new random PWD, is success, message</returns>
        private async Task<Tuple<string, bool, string>> generatePWD()
        {
            try
            {
                var globalSettingsData = await getChangePasswordGlobalSettingsData();

                int randomPWD = 0;

                for (int i = 0; i < 10; i++)
                {
                    randomPWD = RandomNumberGenerator.GetInt32(111111, 999999);
                    if (isSameNumberOrCharacterOrTrendScquenceExists(randomPWD.ToString()
                                                                    , globalSettingsData.Item1
                                                                    , globalSettingsData.Item2
                                                                    , globalSettingsData.Item3).Item1 == true)
                        continue;
                    else
                        return Tuple.Create(randomPWD.ToString(), true, "Success");
                }

                return Tuple.Create(randomPWD.ToString(), false, "Please, try to create new password again.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Generate new random PWD in code level.
        /// </summary>
        /// <returns>new random PWD, is success, message</returns>
        private async Task<Tuple<string, bool, string>> generatePWDV2()
        {
            try
            {
                var globalSettingsData = await getChangePasswordGlobalSettingsDataV2();

                int randomPWD = 0;

                for (int i = 0; i < 10; i++)
                {
                    randomPWD = RandomNumberGenerator.GetInt32(111111, 1000000); // 1000000 is exclusive upper bound
                    if (isSameNumberOrCharacterOrTrendScquenceExistsV2(randomPWD.ToString()
                                                                    , globalSettingsData.Item1
                                                                    , globalSettingsData.Item2
                                                                    , globalSettingsData.Item3).Item1 == true)
                        continue;
                    else
                        return Tuple.Create(randomPWD.ToString(), true, "Success");
                }

                return Tuple.Create(randomPWD.ToString(), false, "Please, try to create new password again.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<RACommonResponse> FORGETPWD(VMForgetPWD model)
        {
            RACommonResponse response = new RACommonResponse();
            int result = 0;
            try
            {
                result = await _dataManager.FORGETPWD(model);

                if (result == (int)EnumForgetPWDStatus.Success)
                {
                    response.result = true;
                    response.message = MessageCollection.PWDSentToMobile;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.SMSSendFailed)
                {
                    response.result = false;
                    response.message = MessageCollection.SMSSendFailed;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.UpdateFailed)
                {
                    response.result = false;
                    response.message = MessageCollection.UpdateFailed;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.UserInfoNotFound)
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.SomethingWrongHappend;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<RACommonResponse> FORGETPWDV2(VMForgetPWD model)
        {
            RACommonResponse response = new RACommonResponse();
            int result = 0;
            try
            {
                result = await _dataManager.FORGETPWDV2(model);

                if (result == (int)EnumForgetPWDStatus.Success)
                {
                    response.result = true;
                    response.message = MessageCollection.PWDSentToMobile;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.SMSSendFailed)
                {
                    response.result = false;
                    response.message = MessageCollection.SMSSendFailed;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.UpdateFailed)
                {
                    response.result = false;
                    response.message = MessageCollection.UpdateFailed;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.UserInfoNotFound)
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.SomethingWrongHappend;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RACommonResponseRevamp> FORGETPWDV3(VMForgetPWD model)
        {
            RACommonResponseRevamp response = new RACommonResponseRevamp();
            int result = 0;
            try
            {
                result = await _dataManager.FORGETPWDV3(model);

                if (result == (int)EnumForgetPWDStatus.Success)
                {
                    response.isError = false;
                    response.message = MessageCollection.PWDSentToMobile;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.SMSSendFailed)
                {
                    response.isError = true;
                    response.message = MessageCollection.SMSSendFailed;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.UpdateFailed)
                {
                    response.isError = true;
                    response.message = MessageCollection.UpdateFailed;
                    return response;
                }
                else if (result == (int)EnumForgetPWDStatus.UserInfoNotFound)
                {
                    response.isError = true;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else
                {
                    response.isError = true;
                    response.message = MessageCollection.SomethingWrongHappend;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<string> IsUserCurrentlyLoggedIn(string userId)
        {
            string loginProvider = String.Empty;
            try
            {
                var dataRow = await _dataManager.IsUserCurrentlyLoggedIn(Convert.ToDecimal(userId));

                if (dataRow.Rows.Count > 0)
                {
                    loginProvider = dataRow.Rows[0]["LOGINPROVIDER"] == DBNull.Value ? null : (string)dataRow.Rows[0]["LOGINPROVIDER"];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginProvider;
        }

        public async Task<Tuple<bool, string>> IsPasswordFormatValid(string inputStr)
        {
            if (String.IsNullOrEmpty(inputStr))
                return Tuple.Create(false, "Password cann't be null or empty.");

            //Item1: passLen
            //Item2: maxAcceptableSameNumLen
            //Item3: maxAcceptableTrendSeqLen
            var globalSettingsData = await getChangePasswordGlobalSettingsData();

            if (inputStr.Length < globalSettingsData.Item1)
                return Tuple.Create(false, "Please enter minimum " + globalSettingsData.Item1 + " digit length.");

            var isSpecialCharecterExistsResult = isSpecialCharecterExists(inputStr);

            if (isSpecialCharecterExistsResult == true)
                return Tuple.Create(false, "Special character not allowed on password.");

            var isSameNumberOrTrendScquenceExistsResult = isSameNumberOrCharacterOrTrendScquenceExists(inputStr, globalSettingsData.Item1, globalSettingsData.Item2, globalSettingsData.Item3);

            if (isSameNumberOrTrendScquenceExistsResult.Item1 == true)
                return Tuple.Create(false, isSameNumberOrTrendScquenceExistsResult.Item2 == -123 ? "Sequential number or character not allowed on password."
                                                                                                                    : "Repeated number or character can’t allowed on password.");

            return Tuple.Create(true, "Success!");
        }

        public async Task<Tuple<bool, string>> IsPasswordFormatValidV2(string inputStr)
        {
            if (String.IsNullOrEmpty(inputStr))
                return Tuple.Create(true, "Password cann't be null or empty.");

            //Item1: passLen
            //Item2: maxAcceptableSameNumLen
            //Item3: maxAcceptableTrendSeqLen
            var globalSettingsData = await getChangePasswordGlobalSettingsDataV2();

            if (inputStr.Length < globalSettingsData.Item1)
                return Tuple.Create(true, "Please enter minimum " + globalSettingsData.Item1 + " digit length.");

            var isSpecialCharecterExistsResult = isSpecialCharecterExistsV2(inputStr);

            if (isSpecialCharecterExistsResult == true)
                return Tuple.Create(true, "Special character not allowed on password.");

            var isSameNumberOrTrendScquenceExistsResult = isSameNumberOrCharacterOrTrendScquenceExistsV2(inputStr, globalSettingsData.Item1, globalSettingsData.Item2, globalSettingsData.Item3);

            if (isSameNumberOrTrendScquenceExistsResult.Item1 == true)
                return Tuple.Create(true, isSameNumberOrTrendScquenceExistsResult.Item2 == -123 ? "Sequential number or character not allowed on password."
                                                                                                                    : "Repeated number or character can’t allowed on password.");

            return Tuple.Create(false, "Success!");
        }

        private async Task<Tuple<int, int, int>> getChangePasswordGlobalSettingsData()
        {
            RAPassLenResponse response = new RAPassLenResponse();
            int passLen = 0, maxAcceptableTrendSeqLen = 0, maxAcceptableSameNumLen = 0;

            try
            {
                var dataRow = await _dataManager.GetChangePasswordGlobalSettingsData();
                //var dataRow = _dataManager.IsUserCurrentlyLoggedIn(10001);

                if (dataRow.Rows.Count > 0)
                {
                    passLen = Convert.ToInt16(dataRow.Rows[0]["RA_PASSWORD_LENGTH"] == DBNull.Value ? null : dataRow.Rows[0]["RA_PASSWORD_LENGTH"]);
                    maxAcceptableSameNumLen = Convert.ToInt16(dataRow.Rows[0]["MAX_ACCEPTABLE_SAME_NUM_LEN"] == DBNull.Value ? null : dataRow.Rows[0]["MAX_ACCEPTABLE_SAME_NUM_LEN"]);
                    maxAcceptableTrendSeqLen = Convert.ToInt16(dataRow.Rows[0]["MAX_ACCEPTABLE_TREND_SEQ_LEN"] == DBNull.Value ? null : dataRow.Rows[0]["MAX_ACCEPTABLE_TREND_SEQ_LEN"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Tuple.Create(passLen, maxAcceptableSameNumLen, maxAcceptableTrendSeqLen);
        }

        private async Task<Tuple<int, int, int>> getChangePasswordGlobalSettingsDataV2()
        {
            RAPassLenResponse response = new RAPassLenResponse();
            int passLen = 0, maxAcceptableTrendSeqLen = 0, maxAcceptableSameNumLen = 0;

            try
            {
                var dataRow = await _dataManager.GetChangePasswordGlobalSettingsDataV2();

                if (dataRow.Rows.Count > 0)
                {
                    passLen = Convert.ToInt16(dataRow.Rows[0]["RA_PASSWORD_LENGTH"] == DBNull.Value ? null : dataRow.Rows[0]["RA_PASSWORD_LENGTH"]);
                    maxAcceptableSameNumLen = Convert.ToInt16(dataRow.Rows[0]["MAX_ACCEPTABLE_SAME_NUM_LEN"] == DBNull.Value ? null : dataRow.Rows[0]["MAX_ACCEPTABLE_SAME_NUM_LEN"]);
                    maxAcceptableTrendSeqLen = Convert.ToInt16(dataRow.Rows[0]["MAX_ACCEPTABLE_TREND_SEQ_LEN"] == DBNull.Value ? null : dataRow.Rows[0]["MAX_ACCEPTABLE_TREND_SEQ_LEN"]);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Tuple.Create(passLen, maxAcceptableSameNumLen, maxAcceptableTrendSeqLen);
        }
        private bool isSpecialCharecterExists(string inputStr)
        {
            try
            {
                var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
                return !regexItem.IsMatch(inputStr.Trim());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool isSpecialCharecterExistsV2(string inputStr)
        {
            try
            {
                var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
                return !regexItem.IsMatch(inputStr.Trim());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// password string lenght must be more then 1 digit. 
        /// </summary>
        /// <param name="inputStr"></param>
        /// <param name="maxAcceptableSameNumLength"></param>
        /// <param name="maxAcceptableTrendSeqLength"></param>
        /// <param name="minPassLength"></param>
        /// <returns></returns>
        /// return_code = -111 for same char match, 
        /// return_code = -123 for trend char match, 
        /// return_code = 1 for not match 
        private Tuple<bool, int> isSameNumberOrCharacterOrTrendScquenceExists(string inputStr,
                                                                                 int minPassLength,
                                                                                 int maxAcceptableSameNumLength,
                                                                                 int maxAcceptableTrendSeqLength)
        {
            try
            {
                int trendSequenceLengthCount = 0;
                int sameCharacterLengthCount = 0;
                bool trendSecquencLengthCheckFlag = false;
                bool sameCharacterCheckFlag = false;

                char[] charArray = inputStr.ToCharArray();

                for (int i = 1; i < charArray.Length; i++)
                {
                    //Same Number Check
                    if (charArray[i] - charArray[i - 1] == 0)
                    {
                        // Ex: 123321
                        if (trendSecquencLengthCheckFlag == true && trendSequenceLengthCount == maxAcceptableTrendSeqLength)
                            return Tuple.Create(trendSecquencLengthCheckFlag, -123);

                        ++sameCharacterLengthCount;
                        trendSequenceLengthCount = 0;

                        if (sameCharacterCheckFlag == true && sameCharacterLengthCount > maxAcceptableSameNumLength)
                            return Tuple.Create(sameCharacterCheckFlag, -111);

                        sameCharacterCheckFlag = true;
                        trendSecquencLengthCheckFlag = false;
                        continue;
                    }
                    //Trend Secquence check 
                    else if (charArray[i] - charArray[i - 1] == 1 &&
                             trendSequenceLengthCount <= maxAcceptableTrendSeqLength)
                    {
                        //Ex: 1112223, 112223, 32111321
                        if (sameCharacterCheckFlag == true && sameCharacterLengthCount == maxAcceptableSameNumLength)
                            return Tuple.Create(sameCharacterCheckFlag, -111);

                        sameCharacterCheckFlag = false;
                        sameCharacterLengthCount = 0;
                        trendSecquencLengthCheckFlag = true;
                        ++trendSequenceLengthCount;

                        //Ex: 123321, 321abc123
                        if (trendSequenceLengthCount > maxAcceptableTrendSeqLength)
                            return Tuple.Create(trendSecquencLengthCheckFlag, -123);
                    }
                    else
                    {
                        //Ex: 3211235
                        if ((trendSequenceLengthCount + 1) > maxAcceptableTrendSeqLength &&
                            trendSecquencLengthCheckFlag == true)
                            return Tuple.Create(trendSecquencLengthCheckFlag, -123);

                        //Ex: 1112223
                        if ((sameCharacterLengthCount + 1) > maxAcceptableSameNumLength &&
                            sameCharacterCheckFlag == true)
                            return Tuple.Create(sameCharacterCheckFlag, -111);

                        sameCharacterCheckFlag = false;
                        sameCharacterLengthCount = 0;
                        trendSecquencLengthCheckFlag = false;
                        trendSequenceLengthCount = 0;
                    }
                }

                //Ex: 321456
                if (trendSequenceLengthCount == maxAcceptableTrendSeqLength &&
                        trendSecquencLengthCheckFlag == true)
                    return Tuple.Create(trendSecquencLengthCheckFlag, -123);

                //Ex: 3214111
                if (sameCharacterLengthCount == maxAcceptableSameNumLength &&
                        sameCharacterCheckFlag == true)
                    return Tuple.Create(sameCharacterCheckFlag, -111);

                return Tuple.Create(false, 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// password string lenght must be more then 1 digit. 
        /// </summary>
        /// <param name="inputStr"></param>
        /// <param name="maxAcceptableSameNumLength"></param>
        /// <param name="maxAcceptableTrendSeqLength"></param>
        /// <param name="minPassLength"></param>
        /// <returns></returns>
        /// return_code = -111 for same char match,  
        /// return_code = -123 for trend char match, 
        /// return_code = 1 for not match 
        private Tuple<bool, int> isSameNumberOrCharacterOrTrendScquenceExistsV2(string inputStr,
                                                                                 int minPassLength,
                                                                                 int maxAcceptableSameNumLength,
                                                                                 int maxAcceptableTrendSeqLength)
        {
            try
            {
                int trendSequenceLengthCount = 0;
                int sameCharacterLengthCount = 0;
                bool trendSecquencLengthCheckFlag = false;
                bool sameCharacterCheckFlag = false;

                char[] charArray = inputStr.ToCharArray();

                for (int i = 1; i < charArray.Length; i++)
                {
                    //Same Number Check
                    if (charArray[i] - charArray[i - 1] == 0)
                    {
                        // Ex: 123321
                        if (trendSecquencLengthCheckFlag == true && trendSequenceLengthCount == maxAcceptableTrendSeqLength)
                            return Tuple.Create(trendSecquencLengthCheckFlag, -123);

                        ++sameCharacterLengthCount;
                        trendSequenceLengthCount = 0;

                        if (sameCharacterCheckFlag == true && sameCharacterLengthCount > maxAcceptableSameNumLength)
                            return Tuple.Create(sameCharacterCheckFlag, -111);

                        sameCharacterCheckFlag = true;
                        trendSecquencLengthCheckFlag = false;
                        continue;
                    }
                    //Trend Secquence check 
                    else if (charArray[i] - charArray[i - 1] == 1 &&
                             trendSequenceLengthCount <= maxAcceptableTrendSeqLength)
                    {
                        //Ex: 1112223, 112223, 32111321
                        if (sameCharacterCheckFlag == true && sameCharacterLengthCount == maxAcceptableSameNumLength)
                            return Tuple.Create(sameCharacterCheckFlag, -111);

                        sameCharacterCheckFlag = false;
                        sameCharacterLengthCount = 0;
                        trendSecquencLengthCheckFlag = true;
                        ++trendSequenceLengthCount;

                        //Ex: 123321, 321abc123
                        if (trendSequenceLengthCount > maxAcceptableTrendSeqLength)
                            return Tuple.Create(trendSecquencLengthCheckFlag, -123);
                    }
                    else
                    {
                        //Ex: 3211235
                        if ((trendSequenceLengthCount + 1) > maxAcceptableTrendSeqLength &&
                            trendSecquencLengthCheckFlag == true)
                            return Tuple.Create(trendSecquencLengthCheckFlag, -123);

                        //Ex: 1112223
                        if ((sameCharacterLengthCount + 1) > maxAcceptableSameNumLength &&
                            sameCharacterCheckFlag == true)
                            return Tuple.Create(sameCharacterCheckFlag, -111);

                        sameCharacterCheckFlag = false;
                        sameCharacterLengthCount = 0;
                        trendSecquencLengthCheckFlag = false;
                        trendSequenceLengthCount = 0;
                    }
                }

                //Ex: 321456
                if (trendSequenceLengthCount == maxAcceptableTrendSeqLength &&
                        trendSecquencLengthCheckFlag == true)
                    return Tuple.Create(trendSecquencLengthCheckFlag, -123);

                //Ex: 3214111
                if (sameCharacterLengthCount == maxAcceptableSameNumLength &&
                        sameCharacterCheckFlag == true)
                    return Tuple.Create(sameCharacterCheckFlag, -111);

                return Tuple.Create(false, 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<LoginUserInfoResponse> ValidateDbssUser(string username, string password)
        {
            LoginUserInfoResponse loginUserInfo = new LoginUserInfoResponse();
            try
            {
                vmUserInfo logInInfo = new vmUserInfo()
                {
                    user_name = username,
                    password = password
                };
                DataTable dataRow = await _dataManager.ValidateDbssUser(logInInfo);
                if (dataRow.Rows.Count > 0)
                {
                    loginUserInfo.user_id = Convert.ToString(dataRow.Rows[0]["USER_ID"] ?? null);
                    loginUserInfo.user_name = Convert.ToString(dataRow.Rows[0]["USER_NAME"] ?? null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bp_msisdn"></param>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public async Task<BPUserValidationResponse> ValidateBPUser(string bp_msisdn, string user_name)
        {
            BPUserValidationResponse response = new BPUserValidationResponse();

            try
            {
                DataTable dataRow = await _dataManager.ValidateBPUser(bp_msisdn, user_name);

                if (dataRow.Rows.Count > 0)
                {
                    response.err_msg = dataRow.Rows[0]["err_msg"].ToString();

                    int isValid = Convert.ToInt32(dataRow.Rows[0]["IS_VALID"]);

                    if (isValid == 0)
                    {
                        response.is_valid = false;
                    }
                    else
                    {
                        response.is_valid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.is_valid = false;
                response.err_msg = ex.Message;
            }
            return response;
        }

        public async Task<BPUserValidationResponse> ValidateBPUserV1(string bp_msisdn, string user_name)
        { 
            BPUserValidationResponse response = new BPUserValidationResponse();

            try
            {
                DataTable dataRow = await _dataManager.ValidateBPUserV1(bp_msisdn, user_name);

                if (dataRow.Rows.Count > 0)
                {
                    response.err_msg = dataRow.Rows[0]["err_msg"].ToString();

                    int isValid = Convert.ToInt32(dataRow.Rows[0]["IS_VALID"]);

                    if (isValid == 0)
                    {
                        response.is_valid = false;
                    }
                    else
                    {
                        response.is_valid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.is_valid = false;
                response.err_msg = ex.Message;
            }
            return response;
        }


        public async Task<bool> GenerateBPLoginOTP(string loginProvider)
        {
            long response = 0;
            try
            {
                response = await _dataManager.GenerateBPLoginOTP(loginProvider);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }

        public async Task<bool> GenerateBPLoginOTPV2(string loginProvider)
        {
            long response = 0;
            try
            {
                response = await _dataManager.GenerateBPLoginOTPV2(loginProvider);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login_attempt_id"></param>
        /// <param name="bp_otp"></param>
        /// <param name="retailer_otp"></param>
        /// <returns></returns>
        public async Task<BPOtpValidationRes> ValidateBPOtp(decimal bp_otp, decimal retailer_otp, string sessionToken)
        {
            BPOtpValidationRes response = new BPOtpValidationRes();

            try
            {
                DataTable dataRow =await _dataManager.ValidateBPOtp(bp_otp, retailer_otp, sessionToken);

                if (dataRow.Rows.Count > 0)
                {
                    response.err_msg = dataRow.Rows[0]["err_msg"].ToString();

                    int isValid = Convert.ToInt32(dataRow.Rows[0]["IS_VALID"]);

                    if (isValid == 0)
                    {
                        response.is_otp_valid = false;
                    }
                    else
                    {
                        response.is_otp_valid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.is_otp_valid = false;
                response.err_msg = ex.Message;
            }
            return response;
        }

        public async Task<BPOtpValidationRes> ValidateBPOtpV2(decimal bp_otp, decimal retailer_otp, string sessionToken)
        {
            BPOtpValidationRes response = new BPOtpValidationRes();

            try
            {
                DataTable dataRow = await _dataManager.ValidateBPOtpV2(bp_otp, retailer_otp, sessionToken);

                if (dataRow.Rows.Count > 0)
                {
                    response.err_msg = dataRow.Rows[0]["err_msg"].ToString();

                    int isValid = Convert.ToInt32(dataRow.Rows[0]["IS_VALID"]);

                    if (isValid == 0)
                    {
                        response.is_otp_valid = true;
                    }
                    else
                    {
                        response.is_otp_valid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                response.is_otp_valid = true;
                response.err_msg = ex.Message;
            }
            return response;
        }

        public async Task<bool> ResendBPOTP(string loginProviderId)
        {
            long response = 0;
            try
            {
                response = await _dataManager.ResendBPOTP(loginProviderId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }

        public async Task<bool> ResendBPOTPV2(string loginProviderId)
        {
            long response = 0;
            try
            {
                response = await _dataManager.ResendBPOTPV2(loginProviderId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }

        public async Task<bool> IsAESEligibleUser(string retailer)
        {
            long response = 0;
            try
            {
                response = await _dataManager.IsAESEligibleUser(retailer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response == 1 ? true : false;
        }

        public async Task<long> Logout(string loginProvider)
        {
            long response = 0;
            try
            {
                response = await _dataManager.Logout(loginProvider);

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<DBResponseModel> CheckUser(UserCheckModel userModel)
        {
            DBResponseModel loginUserInfo = new DBResponseModel();
            try
            {                
                DataTable dataRow = await _dataManager.CheckUser(userModel);
                if (dataRow.Rows.Count > 0)
                {
                    loginUserInfo.is_fp_validation_need = Convert.ToInt32(dataRow.Rows[0]["IS_FP_ACTIVE"] ?? "");
                    loginUserInfo.is_registered = Convert.ToInt32(dataRow.Rows[0]["IS_FP_REGISTER"] ?? "");
                    loginUserInfo.msisdn = Convert.ToString(dataRow.Rows[0]["MOBILE_NUMBER"] ?? "");                    
                    loginUserInfo.is_user_valid = Convert.ToInt32(dataRow.Rows[0]["IS_USER_VALID"] ?? "");
                    loginUserInfo.message = Convert.ToString(dataRow.Rows[0]["MESSAGE"] ?? "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }
         
        public async Task<FPDataResponse> FetchFingerPrint(FPValidationReqModel userModel)
        {
            FPDataResponse loginUserInfo = new FPDataResponse();
            try
            {
                DataTable dataRow = await _dataManager.FetchFingerPrint(userModel);
                if (dataRow.Rows.Count > 0)
                {
                    if(!String.IsNullOrEmpty((dataRow.Rows[0]["RIGHT_THUMB"] ?? "").ToString()))
                    {
                        loginUserInfo.right_thumb = Convert.ToString(dataRow.Rows[0]["RIGHT_THUMB"]);
                    }
                    if (!String.IsNullOrEmpty((dataRow.Rows[0]["RIGHT_INDEX"] ?? "").ToString()))
                    {
                        loginUserInfo.right_index = Convert.ToString(dataRow.Rows[0]["RIGHT_INDEX"]);
                    }
                    if (!String.IsNullOrEmpty((dataRow.Rows[0]["LEFT_THUMB"] ?? "").ToString()))
                    {
                        loginUserInfo.left_thumb = Convert.ToString(dataRow.Rows[0]["LEFT_THUMB"]);
                    }
                    if (!String.IsNullOrEmpty((dataRow.Rows[0]["LEFT_INDEX"] ?? "").ToString()))
                    {
                        loginUserInfo.left_index = Convert.ToString(dataRow.Rows[0]["LEFT_INDEX"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }

        public async Task<long?> SaveFingerPrint(FPRegistrationModel userModel)
        {
            long? loginUserInfo = 0;
            try
            {
                loginUserInfo = await _dataManager.SaveFingerPrint(userModel);                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginUserInfo;
        }

        public async Task<FP_Get_Status> FetchFingerPrintResult(double? bi_token)
        {
            FP_Get_Status fp_status = new FP_Get_Status();
            try
            {
                DataTable dataRow = await _dataManager.GetFingerPrintResult(bi_token);
                if (dataRow.Rows.Count > 0)
                {
                    fp_status.status = Convert.ToInt32(dataRow.Rows[0]["STATUS"] ?? "");
                    fp_status.message = Convert.ToString(dataRow.Rows[0]["MESSAGE"] ?? "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return fp_status;
        }

        public async Task<RetailerNIDDOBRespModel> GetRetailerNIDDOB(string username)
        {
            RetailerNIDDOBRespModel retniddob = new RetailerNIDDOBRespModel(); 
            try
            {
                DataTable dataRow = await _dataManager.GetRetailerNIDDOB(username);
                if (dataRow.Rows.Count > 0)
                {
                    retniddob.NID = Convert.ToString(dataRow.Rows[0]["NID"] ?? "");
                    retniddob.DOB = Convert.ToString(dataRow.Rows[0]["DOB"] ?? "");
                    retniddob.ddCode = Convert.ToString(dataRow.Rows[0]["DISTRIBUTOR_CODE"] ?? "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retniddob;
        }

        public async Task<string> GetIsRegistered(string userName)
        {
            string is_registered = string.Empty;
            try 
            {
                DataTable dataRow = await _dataManager.GetIsRegistered(userName);
                if (dataRow.Rows.Count > 0)
                {
                    is_registered = Convert.ToString(dataRow.Rows[0]["USERNAME"] ?? "");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return is_registered;
        }

        public async Task<APPVersionRespModel> GetAppVersion()
        {
            APPVersionRespModel respModel = new APPVersionRespModel();
            try
            {
                DataTable dataRow = await _dataManager.GetUpdateAPKVersion();
                if (dataRow.Rows.Count > 0)
                {
                    respModel.app_version = Convert.ToInt32(dataRow.Rows[0]["CURRENT_APP_VERSION"] ?? 0);
                    respModel.app_url = Convert.ToString(dataRow.Rows[0]["UPDATE_URL"] ?? 0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return respModel;
        }
    }
}
