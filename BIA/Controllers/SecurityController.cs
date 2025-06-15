using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using BIA.Entity.ViewModel;
using BIA.Helper;
using BIA.JWET;
using BIA.JWT;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BIA.Controllers
{
    [Route("api/Security")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly BLLUserAuthenticaion _bLLUserAuthenticaion;
        private readonly BLLLog _bllLog;
        private readonly BaseController _bio;
        private readonly ApiManager _apiManager;

        public SecurityController(BLLUserAuthenticaion auth,BLLLog bllLog, BaseController bio, ApiManager apiManager)
        {
            _bLLUserAuthenticaion = auth;
            _bllLog = bllLog;
            _bio = bio;
            _apiManager = apiManager;
        }
        // POST: api/Security/Login
        /// <summary>
        /// Authentication API for external user. ***Single user login.***
        /// </summary>
        /// <param name="loginInfo">Requesting parameter with username and password</param>
        /// <returns>Return the authentication information of requesting user</returns>        
        [ValidateModel]
        [Route("LoginV1")]
        public async Task<IActionResult> LoginAsyncV1([FromBody] LoginRequests login)
        {
            string encriptedPwd = Cryptography.Encrypt(login.Password, true);
            LoginUserInfoResponse user =await _bLLUserAuthenticaion.ValidateUser(login.UserName, encriptedPwd);

            if (user == null || user.user_name == null)
            {
                return Ok(new LogInResponse()
                {
                    ISAuthenticate = false,
                    AuthenticationMessage = MessageCollection.InvalidUserCridential,
                    HasUpdate = false,
                });
            }

            string loginProvider = Guid.NewGuid().ToString();


            UserLogInAttempt loginAtmInfo = new UserLogInAttempt()
            {
                userid = user.user_id,
                is_success = 1,
                ip_address = GetIP(),
                loginprovider = loginProvider,
                deviceid = login.DeviceId,
                lan = login.Lan,
                versioncode = login.VersionCode,
                versionname = login.VersionName,
                osversion = login.OSVersion,
                kernelversion = login.KernelVersion,
                fermwarevirsion = login.FermwareVersion,
                //installapps = login.InstalledApps
            };

            _bLLUserAuthenticaion.SaveLoginAtmInfo(loginAtmInfo);

            return Ok(new LogInResponse()
            {
                SessionToken = GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                ISAuthenticate = true,
                AuthenticationMessage = MessageCollection.UserValidted,
                UserName = login.UserName,
                Password = login.Password,
                DeviceId = login.DeviceId,
                HasUpdate = false,
                MinimumScore = SettingsValues.GetFPDefaultScore(),
                OptionalMinimumScore = "30",
                MaximumRetry = "2",
                RoleAccess = user.role_access,
                ChannelId = user.channel_id,
                ChannelName = user.channel_name
            });
        }
        private string GetIP()
        {
            //IHttpContextAccessor httpContextAccessor = null;
            //string? userIpAddress = httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var feature = HttpContext.Features.Get<IHttpConnectionFeature>();
            string LocalIPAddr = feature?.LocalIpAddress?.ToString();

            if (!String.IsNullOrEmpty(LocalIPAddr))
            {
                return LocalIPAddr;
            }
            else
            {
                return "";
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Retreive server/local IP address
            var feature = HttpContext.Features.Get<IHttpConnectionFeature>();
            string LocalIPAddr = feature?.LocalIpAddress?.ToString();

            return Ok(LocalIPAddr);
        }

        //=====================Multiple User Login==================
        /// <summary>
        /// Verify user and generates security token.
        /// 1. If the user logged in for first time new security token will generate.  
        /// 2. One user can login from diferrent device. 
        /// 3. If the user is already logged in, then no new security token will generate. Last time generated token will resend.  
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        //[ResponseType(typeof(LogInResponse))]
        //[GzipCompression]
        [ValidateModel]
        [Route("Login")]
        public async Task<IActionResult> LoginAsyncV2([FromBody] LoginRequests login)
        {
            try
            {
                string encriptedPwd = Cryptography.Encrypt(login.Password, true);
                LoginUserInfoResponse user = await _bLLUserAuthenticaion.ValidateUser(login.UserName, encriptedPwd);

                if (user.user_name == null)
                {
                    return Ok(new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }



                #region Password Policy Checking

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValid(login.Password);

                if (validationResult.Item1 == false)
                {
                    return Ok(new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = validationResult.Item2,
                        HasUpdate = false,
                    });
                }

                #endregion

                string loginProviderId = await _bLLUserAuthenticaion.IsUserCurrentlyLoggedIn(user.user_id);

                UserLogInAttempt loginAtmInfo;
                string loginProvider = Guid.NewGuid().ToString();

                if (String.IsNullOrEmpty(loginProviderId))
                {

                    loginAtmInfo = new UserLogInAttempt()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion
                    };
                }
                else
                {
                    loginProvider = loginProviderId;

                    loginAtmInfo = new UserLogInAttempt()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion
                    };
                }

                Thread logThread = new Thread(() => _bLLUserAuthenticaion.SaveLoginAtmInfo(loginAtmInfo));
                logThread.Start();

                return Ok(new LogInResponse()
                {
                    SessionToken = GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                    ISAuthenticate = true,
                    AuthenticationMessage = MessageCollection.UserValidted,
                    UserName = login.UserName,
                    Password = login.Password,
                    DeviceId = login.DeviceId,
                    HasUpdate = false,
                    MinimumScore = SettingsValues.GetFPDefaultScore(),
                    OptionalMinimumScore = "30",
                    MaximumRetry = "2",
                    RoleAccess = user.role_access,
                    ChannelId = user.channel_id,
                    ChannelName = user.channel_name,
                    InventoryId = user.inventory_id,
                    CenterCode = user.center_code
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        //=====================Multiple User Login including BP==================
        /// <summary>
        /// Verify user and generates security token.
        /// 1. If the user logged in for first time new security token will generate.  
        /// 2. One user can login from diferrent device. 
        /// 3. If the user is already logged in, then no new security token will generate. Last time generated token will resend.  
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        //[ResponseType(typeof(LogInResponse))]
        //[GzipCompression]
        [ValidateModel]
        [Route("LoginV3")]
        public async Task<IActionResult> LoginAsyncV3([FromBody] LoginRequestsV2 login)
        {
            LogInResponse response = new LogInResponse();
            string encriptedPwd = string.Empty;
            try
            {
                int isEligible = 0;
                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    isEligible = Convert.ToInt32(configuration.GetSection("AppSettings:IsEligibleAES").Value);
                }
                catch { }

                if (isEligible == 1)
                {
                    bool isEligibleUser = await _bLLUserAuthenticaion.IsAESEligibleUser(login.UserName);
                    if (isEligibleUser)
                    {
                        encriptedPwd = AESCryptography.Encrypt(login.Password);
                        response = await LoginByAESEncription(login, encriptedPwd);
                        return Ok(response);
                    }
                    else
                    {
                        response = new LogInResponse();
                        encriptedPwd = Cryptography.Encrypt(login.Password, true);
                        response = await LoginByMD5Encription(login, encriptedPwd);
                        return Ok(response);
                    }
                }
                else
                {
                    response = new LogInResponse();
                    encriptedPwd = AESCryptography.Encrypt(login.Password);
                    response = await LoginByAESEncription(login, encriptedPwd);
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        private async Task<LogInResponse> LoginByAESEncription(LoginRequestsV2 login, string encPwd)
        {
            try
            {
                LoginUserInfoResponse user =await _bLLUserAuthenticaion.ValidateUser(login.UserName, encPwd);

                if (user.user_name == null)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }

                #region Password Policy Checking

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValid(login.Password);

                if (validationResult.Item1 == false)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = validationResult.Item2,
                        HasUpdate = false,
                    });
                }

                #endregion

                if (string.IsNullOrEmpty(login.BPMSISDN))
                {
                    UserLogInAttemptV2 loginAtmInfo;
                    string loginProvider = Guid.NewGuid().ToString();

                    loginAtmInfo = new UserLogInAttemptV2()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion,
                        latitude = login.latitude,
                        longitude = login.longitude,
                        lac = login.lac,
                        cid = login.cid,
                        is_bp = 0,
                        bp_msisdn = login.BPMSISDN,
                        device_model = login.DeviceModel
                    };

                    Thread logThread = new Thread(() => _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo));

                    logThread.Start();

                    return (new LogInResponse()
                    {
                        SessionToken = GetEncriptedSecurityTokenV2(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                        ISAuthenticate = true,
                        AuthenticationMessage = MessageCollection.UserValidted,
                        UserName = login.UserName,
                        Password = login.Password,
                        DeviceId = login.DeviceId,
                        HasUpdate = false,
                        MinimumScore = SettingsValues.GetFPDefaultScore(),
                        OptionalMinimumScore = "30",
                        MaximumRetry = "2",
                        RoleAccess = user.role_access,
                        ChannelId = user.channel_id,
                        ChannelName = user.channel_name,
                        InventoryId = user.inventory_id,
                        CenterCode = user.center_code
                    });
                }
                else
                {
                    string bp_msisdn = ConverterHelper.MSISDNCountryCodeAddition(login.BPMSISDN, FixedValueCollection.MSISDNCountryCode);

                    BPUserValidationResponse bPUserValidationResponse = await _bLLUserAuthenticaion.ValidateBPUser(bp_msisdn, login.UserName);

                    if (!bPUserValidationResponse.is_valid)
                    {
                        return (new LogInResponse()
                        {
                            ISAuthenticate = bPUserValidationResponse.is_valid,
                            AuthenticationMessage = bPUserValidationResponse.err_msg,
                            HasUpdate = false,
                        });
                    }
                    else
                    {
                        UserLogInAttemptV2 loginAtmInfo;
                        string loginProvider = Guid.NewGuid().ToString();

                        loginAtmInfo = new UserLogInAttemptV2()
                        {
                            userid = user.user_id,
                            is_success = 1,
                            ip_address = GetIP(),
                            loginprovider = loginProvider,
                            deviceid = login.DeviceId,
                            lan = login.Lan,
                            versioncode = login.VersionCode,
                            versionname = login.VersionName,
                            osversion = login.OSVersion,
                            kernelversion = login.KernelVersion,
                            fermwarevirsion = login.FermwareVersion,
                            latitude = login.latitude,
                            longitude = login.longitude,
                            lac = login.lac,
                            cid = login.cid,
                            is_bp = 1,
                            bp_msisdn = login.BPMSISDN,
                            device_model = login.DeviceModel
                        };

                        _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo);


                        Thread logThread = new Thread(() => _bLLUserAuthenticaion.GenerateBPLoginOTP(loginProvider));

                        logThread.Start();

                        return (new LogInResponse()
                        {
                            SessionToken = GetEncriptedSecurityTokenV2(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                            ISAuthenticate = true,
                            AuthenticationMessage = MessageCollection.UserValidted,
                            UserName = login.UserName,
                            Password = login.Password,
                            DeviceId = login.DeviceId,
                            HasUpdate = false,
                            MinimumScore = SettingsValues.GetFPDefaultScore(),
                            OptionalMinimumScore = "30",
                            MaximumRetry = "2",
                            RoleAccess = user.role_access,
                            ChannelId = user.channel_id,
                            ChannelName = user.channel_name,
                            InventoryId = user.inventory_id,
                            CenterCode = user.center_code
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return (new LogInResponse()
                {
                    ISAuthenticate = false,
                    AuthenticationMessage = ex.Message.ToString(),
                    HasUpdate = false,
                });
            }
        }
        private async Task<LogInResponse> LoginByMD5Encription(LoginRequestsV2 login, string encPwd)
        {
            try
            {
                LoginUserInfoResponse user = await _bLLUserAuthenticaion.ValidateUser(login.UserName, encPwd);

                if (user.user_name == null)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }

                #region Password Policy Checking

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValid(login.Password);

                if (validationResult.Item1 == false)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = validationResult.Item2,
                        HasUpdate = false,
                    });
                }

                #endregion

                if (string.IsNullOrEmpty(login.BPMSISDN))
                {
                    UserLogInAttemptV2 loginAtmInfo;
                    string loginProvider = Guid.NewGuid().ToString();

                    loginAtmInfo = new UserLogInAttemptV2()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion,
                        latitude = login.latitude,
                        longitude = login.longitude,
                        lac = login.lac,
                        cid = login.cid,
                        is_bp = 0,
                        bp_msisdn = login.BPMSISDN,
                        device_model = login.DeviceModel
                    };

                    Thread logThread = new Thread(() => _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo));

                    logThread.Start();

                    return (new LogInResponse()
                    {
                        SessionToken = GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                        ISAuthenticate = true,
                        AuthenticationMessage = MessageCollection.UserValidted,
                        UserName = login.UserName,
                        Password = login.Password,
                        DeviceId = login.DeviceId,
                        HasUpdate = false,
                        MinimumScore = SettingsValues.GetFPDefaultScore(),
                        OptionalMinimumScore = "30",
                        MaximumRetry = "2",
                        RoleAccess = user.role_access,
                        ChannelId = user.channel_id,
                        ChannelName = user.channel_name,
                        InventoryId = user.inventory_id,
                        CenterCode = user.center_code
                    });
                }
                else
                {
                    string bp_msisdn = ConverterHelper.MSISDNCountryCodeAddition(login.BPMSISDN, FixedValueCollection.MSISDNCountryCode);

                    BPUserValidationResponse bPUserValidationResponse = await _bLLUserAuthenticaion.ValidateBPUser(bp_msisdn, login.UserName);

                    if (!bPUserValidationResponse.is_valid)
                    {
                        return (new LogInResponse()
                        {
                            ISAuthenticate = bPUserValidationResponse.is_valid,
                            AuthenticationMessage = bPUserValidationResponse.err_msg,
                            HasUpdate = false,
                        });
                    }
                    else
                    {
                        UserLogInAttemptV2 loginAtmInfo;
                        string loginProvider = Guid.NewGuid().ToString();

                        loginAtmInfo = new UserLogInAttemptV2()
                        {
                            userid = user.user_id,
                            is_success = 1,
                            ip_address = GetIP(),
                            loginprovider = loginProvider,
                            deviceid = login.DeviceId,
                            lan = login.Lan,
                            versioncode = login.VersionCode,
                            versionname = login.VersionName,
                            osversion = login.OSVersion,
                            kernelversion = login.KernelVersion,
                            fermwarevirsion = login.FermwareVersion,
                            latitude = login.latitude,
                            longitude = login.longitude,
                            lac = login.lac,
                            cid = login.cid,
                            is_bp = 1,
                            bp_msisdn = login.BPMSISDN,
                            device_model = login.DeviceModel
                        };

                        _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo);

                        Thread logThread = new Thread(() => _bLLUserAuthenticaion.GenerateBPLoginOTP(loginProvider));

                        logThread.Start();

                        return (new LogInResponse()
                        {
                            SessionToken = GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                            ISAuthenticate = true,
                            AuthenticationMessage = MessageCollection.UserValidted,
                            UserName = login.UserName,
                            Password = login.Password,
                            DeviceId = login.DeviceId,
                            HasUpdate = false,
                            MinimumScore = SettingsValues.GetFPDefaultScore(),
                            OptionalMinimumScore = "30",
                            MaximumRetry = "2",
                            RoleAccess = user.role_access,
                            ChannelId = user.channel_id,
                            ChannelName = user.channel_name,
                            InventoryId = user.inventory_id,
                            CenterCode = user.center_code
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return (new LogInResponse()
                {
                    ISAuthenticate = false,
                    AuthenticationMessage = ex.Message.ToString(),
                    HasUpdate = false,
                });
            }
        }
        //=================x===================================

        #region Revamp Login
        [ValidateModel]
        [Route("LoginV4")]
        public async Task<IActionResult> LoginAsyncV4([FromBody] LoginRequestsV2 login)
        {
            LogInResponse response = new LogInResponse();
            string encriptedPwd = string.Empty;
            try
            {
                int isEligible = 0;
                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    isEligible = Convert.ToInt32(configuration.GetSection("AppSettings:IsEligibleAES").Value);
                }
                catch { }

                if (isEligible == 1)
                {
                    bool isEligibleUser = await _bLLUserAuthenticaion.IsAESEligibleUser(login.UserName);
                    if (isEligibleUser)
                    {
                        encriptedPwd = AESCryptography.Encrypt(login.Password);
                        response = await LoginByAESEncriptionV1(login, encriptedPwd);
                        return Ok(response);
                    }
                    else
                    {
                        response = new LogInResponse();
                        encriptedPwd = Cryptography.Encrypt(login.Password, true);
                        response = await LoginByMD5EncriptionV1(login, encriptedPwd);
                        return Ok(response);
                    }
                }
                else
                {
                    response = new LogInResponse();
                    encriptedPwd = AESCryptography.Encrypt(login.Password);
                    response = await LoginByAESEncriptionV1(login, encriptedPwd);
                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        private async Task<LogInResponse> LoginByAESEncriptionV1(LoginRequestsV2 login, string encPwd)
        {
            JWETToken jWETToken = new JWETToken();
            string secreteKey = string.Empty;

            secreteKey = SettingsValues.GetJWTSequrityKey();

            TokenService token = new TokenService(secreteKey);
            try
            {
                LoginUserInfoResponseRev user = await _bLLUserAuthenticaion.ValidateUserV2(login, login.UserName, encPwd);

                if (user.user_name == null && user.isValidUser == 2)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = user.message,
                        HasUpdate = false,
                    });
                }
                else if (user.user_name == null)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }

                #region Password Policy Checking

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValid(login.Password);

                if (validationResult.Item1 == false)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = validationResult.Item2,
                        HasUpdate = false,
                    });
                }

                #endregion

                if (string.IsNullOrEmpty(login.BPMSISDN))
                {
                    UserLogInAttemptV2 loginAtmInfo;
                    string loginProvider = Guid.NewGuid().ToString();

                    loginAtmInfo = new UserLogInAttemptV2()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion,
                        latitude = login.latitude,
                        longitude = login.longitude,
                        lac = login.lac,
                        cid = login.cid,
                        is_bp = 0,
                        bp_msisdn = login.BPMSISDN,
                        device_model = login.DeviceModel,


                    };

                    if (!String.IsNullOrEmpty(user.user_id))
                    {
                        Thread logThread = new Thread(() => _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo));

                        logThread.Start();
                    }
                    return (new LogInResponse()
                    {
                        SessionToken = user.isValidUser == 1 ? token.GenerateToken(user, loginProvider) : "",//  GetEncriptedSecurityTokenV2(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                        ISAuthenticate = user.isValidUser == 1 ? true : false,
                        AuthenticationMessage = user.message,
                        UserName = login.UserName,
                        Password = login.Password,
                        DeviceId = login.DeviceId,
                        HasUpdate = false,
                        MinimumScore = SettingsValues.GetFPDefaultScore(),
                        OptionalMinimumScore = "30",
                        MaximumRetry = "2",
                        RoleAccess = user.role_access,
                        ChannelId = user.channel_id,
                        ChannelName = user.channel_name,
                        InventoryId = user.inventory_id,
                        CenterCode = user.center_code,
                        itopUpNumber = user.itopUpNumber,
                        is_default_Password = user.is_default_Password,
                        ExpiredDate = user.ExpiredDate,
                        Designation = user.designation,
                        is_etsaf_validation_need = SettingsValues.GetETSAFValidationValue()

                    });
                }
                else
                {
                    string bp_msisdn = ConverterHelper.MSISDNCountryCodeAddition(login.BPMSISDN, FixedValueCollection.MSISDNCountryCode);

                    BPUserValidationResponse bPUserValidationResponse = await _bLLUserAuthenticaion.ValidateBPUserV1(bp_msisdn, login.UserName);

                    if (!bPUserValidationResponse.is_valid)
                    {
                        return (new LogInResponse()
                        {
                            ISAuthenticate = bPUserValidationResponse.is_valid,
                            AuthenticationMessage = bPUserValidationResponse.err_msg,
                            HasUpdate = false,
                        });
                    }
                    else
                    {
                        UserLogInAttemptV2 loginAtmInfo;
                        string loginProvider = Guid.NewGuid().ToString();

                        loginAtmInfo = new UserLogInAttemptV2()
                        {
                            userid = user.user_id,
                            is_success = 1,
                            ip_address = GetIP(),
                            loginprovider = loginProvider,
                            deviceid = login.DeviceId,
                            lan = login.Lan,
                            versioncode = login.VersionCode,
                            versionname = login.VersionName,
                            osversion = login.OSVersion,
                            kernelversion = login.KernelVersion,
                            fermwarevirsion = login.FermwareVersion,
                            latitude = login.latitude,
                            longitude = login.longitude,
                            lac = login.lac,
                            cid = login.cid,
                            is_bp = 1,
                            bp_msisdn = login.BPMSISDN,
                            device_model = login.DeviceModel
                        };

                        if (!String.IsNullOrEmpty(user.user_id))
                        {
                            _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo);
                        }

                        Thread logThread = new Thread(() => _bLLUserAuthenticaion.GenerateBPLoginOTPV2(loginProvider));

                        logThread.Start();

                        return (new LogInResponse()
                        {
                            SessionToken = user.isValidUser == 1 ? token.GenerateToken(user, loginProvider) : "", ///GetEncriptedSecurityTokenV2(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                            ISAuthenticate = user.isValidUser == 1 ? true : false,
                            AuthenticationMessage = user.message,
                            UserName = login.UserName,
                            Password = "",
                            DeviceId = login.DeviceId,
                            HasUpdate = false,
                            MinimumScore = SettingsValues.GetFPDefaultScore(),
                            OptionalMinimumScore = "30",
                            MaximumRetry = "2",
                            RoleAccess = user.role_access,
                            ChannelId = user.channel_id,
                            ChannelName = user.channel_name,
                            InventoryId = user.inventory_id,
                            CenterCode = user.center_code,
                            itopUpNumber = user.itopUpNumber,
                            is_default_Password = user.is_default_Password,
                            ExpiredDate = user.ExpiredDate,
                            Designation = user.designation,
                            is_etsaf_validation_need = SettingsValues.GetETSAFValidationValue()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return (new LogInResponse()
                {
                    ISAuthenticate = false,
                    AuthenticationMessage = ex.Message.ToString(),
                    HasUpdate = false,
                });
            }
        }
        private async Task<LogInResponse> LoginByMD5EncriptionV1(LoginRequestsV2 login, string encPwd)
        {
            string secreteKey = string.Empty;

            secreteKey = SettingsValues.GetJWTSequrityKey();

            TokenService token = new TokenService(secreteKey);

            try
            {
                LoginUserInfoResponseRev user = await _bLLUserAuthenticaion.ValidateUserV2(login, login.UserName, encPwd);

                if (user.user_name == null && user.isValidUser == 2)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = user.message,
                        HasUpdate = false,
                    });
                }
                else if (user.user_name == null)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }                     

                #region Password Policy Checking

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValid(login.Password);

                if (validationResult.Item1 == false)
                {
                    return (new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = validationResult.Item2,
                        HasUpdate = false,
                    });
                }

                #endregion

                if (string.IsNullOrEmpty(login.BPMSISDN))
                {
                    UserLogInAttemptV2 loginAtmInfo;
                    string loginProvider = Guid.NewGuid().ToString();

                    loginAtmInfo = new UserLogInAttemptV2()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion,
                        latitude = login.latitude,
                        longitude = login.longitude,
                        lac = login.lac,
                        cid = login.cid,
                        is_bp = 0,
                        bp_msisdn = login.BPMSISDN,
                        device_model = login.DeviceModel
                    };

                    if (!String.IsNullOrEmpty(user.user_id))
                    {
                        Thread logThread = new Thread(async () => await _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo));

                        logThread.Start();
                    }

                    return (new LogInResponse()
                    {
                        SessionToken = user.isValidUser == 1 ? token.GenerateToken(user, loginProvider) : "", ///GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                        ISAuthenticate = user.isValidUser == 1 ? true : false,
                        AuthenticationMessage = user.message,
                        UserName = login.UserName,
                        Password = "",
                        DeviceId = login.DeviceId,
                        HasUpdate = false,
                        MinimumScore = SettingsValues.GetFPDefaultScore(),
                        OptionalMinimumScore = "30",
                        MaximumRetry = "2",
                        RoleAccess = user.role_access,
                        ChannelId = user.channel_id,
                        ChannelName = user.channel_name,
                        InventoryId = user.inventory_id,
                        CenterCode = user.center_code,
                        itopUpNumber = user.itopUpNumber,
                        is_default_Password = user.is_default_Password,
                        ExpiredDate = user.ExpiredDate,
                        Designation = user.designation,
                        is_etsaf_validation_need = SettingsValues.GetETSAFValidationValue()
                    });
                }
                else
                {
                    string bp_msisdn = ConverterHelper.MSISDNCountryCodeAddition(login.BPMSISDN, FixedValueCollection.MSISDNCountryCode);

                    BPUserValidationResponse bPUserValidationResponse = await _bLLUserAuthenticaion.ValidateBPUserV1(bp_msisdn, login.UserName);

                    if (!bPUserValidationResponse.is_valid)
                    {
                        return (new LogInResponse()
                        {
                            ISAuthenticate = bPUserValidationResponse.is_valid,
                            AuthenticationMessage = bPUserValidationResponse.err_msg,
                            HasUpdate = false,
                        });
                    }
                    else
                    {
                        UserLogInAttemptV2 loginAtmInfo;
                        string loginProvider = Guid.NewGuid().ToString();

                        loginAtmInfo = new UserLogInAttemptV2()
                        {
                            userid = user.user_id,
                            is_success = 1,
                            ip_address = GetIP(),
                            loginprovider = loginProvider,
                            deviceid = login.DeviceId,
                            lan = login.Lan,
                            versioncode = login.VersionCode,
                            versionname = login.VersionName,
                            osversion = login.OSVersion,
                            kernelversion = login.KernelVersion,
                            fermwarevirsion = login.FermwareVersion,
                            latitude = login.latitude,
                            longitude = login.longitude,
                            lac = login.lac,
                            cid = login.cid,
                            is_bp = 1,
                            bp_msisdn = login.BPMSISDN,
                            device_model = login.DeviceModel
                        };

                        if (!String.IsNullOrEmpty(user.user_id))
                        {
                            _bLLUserAuthenticaion.SaveLoginAtmInfoV2(loginAtmInfo);
                        }

                        Thread logThread = new Thread(() => _bLLUserAuthenticaion.GenerateBPLoginOTPV2(loginProvider));

                        logThread.Start();

                        return (new LogInResponse()
                        {
                            SessionToken = user.isValidUser == 1 ? token.GenerateToken(user, loginProvider) : "", ///GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                            ISAuthenticate = user.isValidUser == 1 ? true : false,
                            AuthenticationMessage = user.message,
                            UserName = login.UserName,
                            Password = login.Password,
                            DeviceId = login.DeviceId,
                            HasUpdate = false,
                            MinimumScore = SettingsValues.GetFPDefaultScore(),
                            OptionalMinimumScore = "30",
                            MaximumRetry = "2",
                            RoleAccess = user.role_access,
                            ChannelId = user.channel_id,
                            ChannelName = user.channel_name,
                            InventoryId = user.inventory_id,
                            CenterCode = user.center_code,
                            itopUpNumber = user.itopUpNumber,
                            is_default_Password = user.is_default_Password,
                            ExpiredDate = user.ExpiredDate,
                            Designation = user.designation,
                            is_etsaf_validation_need = SettingsValues.GetETSAFValidationValue()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return (new LogInResponse()
                {
                    ISAuthenticate = false,
                    AuthenticationMessage = ex.Message.ToString(),
                    HasUpdate = false,
                });
            }
        }

        #endregion


        // POST: api/Security/Login
        /// <summary>
        /// Authentication API for external user(DBSS)
        /// </summary>
        /// <param name="loginInfo">Requesting parameter with username and password</param>
        /// <returns>Return the authentication information of requesting user</returns>  
        //[ResponseType(typeof(DBSSLogInResponse))]
        [ValidateModel]
        [Route("DBSSLogin")]
        public async Task<IActionResult> DBSSLoginAsync([FromBody] DBSSLoginRequests login)
        {
            BIAToDBSSLog biaLogObj = new BIAToDBSSLog();
            BL_Json bllJson = new BL_Json();
            DBSSLogInResponse response = new DBSSLogInResponse();
            string txtReq = string.Empty, txtResp = string.Empty;
            try
            {
                biaLogObj.req_blob = bllJson.GetGenericJsonData(login);
                biaLogObj.req_time = DateTime.Now;
                txtReq = JsonConvert.SerializeObject(login);

                LoginUserInfoResponse user = await _bLLUserAuthenticaion.ValidateDbssUser(login.UserName, login.Password);

                if (user.user_name == null)
                {
                    return Ok(new DBSSLogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential
                    });
                }
                string loginProvider = Guid.NewGuid().ToString();

                UserLogInAttempt loginAtmInfo = new UserLogInAttempt()
                {
                    userid = user.user_id,
                    is_success = 1,
                    ip_address = GetIP(),
                    loginprovider = loginProvider
                };

                Thread logThread = new Thread(() => _bLLUserAuthenticaion.SaveLoginAtmInfo(loginAtmInfo));
                logThread.Start();

                int isEligible = 0;
                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    isEligible = Convert.ToInt32(configuration.GetSection("AppSettings:IsEligibleAES").Value);
                }
                catch { }

                if (isEligible == 0)
                {
                    response = new DBSSLogInResponse()
                    {
                        SessionToken = GetEncriptedSecurityTokenV2(loginProvider, user.user_id, user.user_name, user.distributor_code,0),
                        ISAuthenticate = true,
                        AuthenticationMessage = MessageCollection.UserValidted
                    };
                }
                else
                {
                    response = new DBSSLogInResponse()
                    {
                        SessionToken = GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, 0),
                        ISAuthenticate = true,
                        AuthenticationMessage = MessageCollection.UserValidted
                    };
                }





                biaLogObj.res_blob = bllJson.GetGenericJsonData(response);
                biaLogObj.res_time = DateTime.Now;
                txtResp = JsonConvert.SerializeObject(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                biaLogObj.res_blob = bllJson.GetGenericJsonData(ex.Message);
                biaLogObj.res_time = DateTime.Now;
                biaLogObj.is_success = 0;
                ErrorDescription error = new ErrorDescription();
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    biaLogObj.error_code = error.error_code ?? String.Empty;
                    biaLogObj.error_source = error.error_source ?? String.Empty;
                    biaLogObj.message = error.error_custom_msg ?? String.Empty;

                    return Ok(new DBSSLogInResponse()
                    {
                        SessionToken = null,
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.Failed
                    });
                }
                catch (Exception)
                {
                    return Ok(new DBSSLogInResponse()
                    {
                        SessionToken = null,
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.Failed
                    });
                }
            }
            finally
            {
                biaLogObj.method_name = "DBSSLoginAsync";
                biaLogObj.error_source = "BIA";
                biaLogObj.user_id = login.UserName;
                biaLogObj.remarks = "";
                biaLogObj.integration_point_from = Convert.ToDecimal(IntegrationPoints.BSS);
                biaLogObj.integration_point_to = Convert.ToDecimal(IntegrationPoints.RA);

                await _bllLog.RAToDBSSLog(biaLogObj, txtReq, txtResp);
            }
        }



        /// <summary>
        /// Get Reseller app info. 
        /// This api is called by reseller app before login api call.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        //[ResponseType(typeof(APIVersionResponseWithAppUpdateCheck))]
        [Route("GetAPIServer")]
        public async Task<IActionResult> GetAPIServerV2([FromBody] APIVersionRequestWithAppUpdateCheck model)
        {
            APIVersionResponse apiVersionRespObj = new APIVersionResponse();
            int apiVersion = 0;
            try
            {
                apiVersion = await _bLLUserAuthenticaion.GetUserAPIVersion(new APIVersionRequest { username = model.username });

                if (apiVersion == 0)
                {
                    return Ok(new APIVersionResponseWithAppUpdateCheck()
                    {
                        api_version = 0,
                        message = MessageCollection.UserNotFound,
                        result = false,
                        app_update_info = new AppUpdateInfo
                        {
                            is_update_exists = false,
                            is_update_mandatory = 0,
                            update_url = null
                        }

                    });
                }

                if (model.appVersion.HasValue)
                {
                    var apiUpdateData = await _bLLUserAuthenticaion.GetUserAPIVersionWithAppUpdateCheck(new APIVersionRequestWithAppUpdateCheck
                    {
                        username = model.username,
                        appVersion = model.appVersion
                    });

                    apiUpdateData.api_version = apiVersion;
                    apiUpdateData.message = apiVersion == 1 ? "Old version." : "New version.";
                    return Ok(apiUpdateData);
                }
                //=========for change pwd=========
                if (apiVersion == 1)
                {
                    apiVersionRespObj.result = true;
                    apiVersionRespObj.message = "Old version.";
                    apiVersionRespObj.api_version = apiVersion;
                }
                else
                {
                    apiVersionRespObj.result = true;
                    apiVersionRespObj.message = "New version.";
                    apiVersionRespObj.api_version = apiVersion;
                }
                return Ok(apiVersionRespObj);
                //=========x==============
            }
            catch (Exception ex)
            {
                apiVersionRespObj.result = false;
                apiVersionRespObj.api_version = 0;

                try
                {
                    var error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    apiVersionRespObj.message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description;

                    return Ok(apiVersionRespObj);
                }
                catch (Exception)
                {
                    apiVersionRespObj.message = MessageCollection.Failed;
                    return Ok(apiVersionRespObj);
                }
            }
        }

        /// <summary>
        /// Get Reseller app info. 
        /// This api is called by reseller app before login api call.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[GzipCompression]
        [HttpPost]
        //[ResponseType(typeof(APIVersionResponseWithAppUpdateCheck))]
        [Route("GetAPIServerV2")]
        public async Task<IActionResult> GetAPIServerV3([FromBody] APIVersionRequestWithAppUpdateCheck model)
        {
            APIVersionResponseRev apiVersionRespObj = new APIVersionResponseRev();
            int apiVersion = 0;
            try
            {
                apiVersion = await _bLLUserAuthenticaion.GetUserAPIVersion(new APIVersionRequest { username = model.username });

                if (apiVersion == 0)
                {
                    return Ok(new APIVersionResponseWithAppUpdateCheckRev()
                    {
                        message = MessageCollection.UserNotFound,
                        isError = true,
                        data = new AppUpdateInfoV2
                        {
                            is_update_exists = false,
                            is_update_mandatory = 0,
                            api_version = 0,
                            update_url = null
                        }

                    });
                }

                if (model.appVersion.HasValue)
                {
                    var apiUpdateData = await _bLLUserAuthenticaion.GetUserAPIVersionWithAppUpdateCheckV2(new APIVersionRequestWithAppUpdateCheck
                    {
                        username = model.username,
                        appVersion = model.appVersion
                    });

                    apiUpdateData.data.api_version = apiVersion;
                    apiUpdateData.message = apiVersion == 1 ? "Old version." : "New version.";
                    return Ok(apiUpdateData);
                }
                //=========for change pwd=========
                if (apiVersion == 1)
                {
                    apiVersionRespObj.isError = false;
                    apiVersionRespObj.message = "Old version.";
                    apiVersionRespObj.data = new APIVersionData()
                    {
                        api_version = apiVersion
                    };
                }
                else
                {
                    apiVersionRespObj.isError = false;
                    apiVersionRespObj.message = "New version.";
                    apiVersionRespObj.data = new APIVersionData()
                    {
                        api_version = apiVersion
                    };
                }
                return Ok(apiVersionRespObj);
                //=========x==============
            }
            catch (Exception ex)
            {
                apiVersionRespObj.isError = true;
                apiVersionRespObj.data.api_version = 0;

                try
                {
                    var error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    apiVersionRespObj.message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description;

                    return Ok(apiVersionRespObj);
                }
                catch (Exception)
                {
                    apiVersionRespObj.message = MessageCollection.Failed;
                    return Ok(apiVersionRespObj);
                }
            }
        }

        //[GzipCompression]
        [HttpPost]
        //[ResponseType(typeof(APIVersionResponseWithAppUpdateCheck))]
        [Route("GetAPIServerV3")]
        public async Task<IActionResult> GetAPIServerV4([FromBody] APIVersionRequestWithAppUpdateCheckForGotPass model)
        {
            APIVersionResponseRev apiVersionRespObj = new APIVersionResponseRev();
            int apiVersion = 0;
            try
            {
                apiVersion = await _bLLUserAuthenticaion.GetUserAPIVersion(new APIVersionRequest { username = model.username });

                if (apiVersion == 0)
                {
                    return Ok(new APIVersionResponseWithAppUpdateCheckRev()
                    {
                        message = MessageCollection.UserNotFound,
                        isError = true,
                        data = new AppUpdateInfoV2
                        {
                            is_update_exists = false,
                            is_update_mandatory = 0,
                            api_version = 0,
                            update_url = null
                        }

                    });
                }
                //=========for change pwd=========
                if (apiVersion == 1)
                {
                    apiVersionRespObj.isError = false;
                    apiVersionRespObj.message = "Old version.";
                    apiVersionRespObj.data = new APIVersionData()
                    {
                        api_version = apiVersion
                    };
                }
                else
                {
                    apiVersionRespObj.isError = false;
                    apiVersionRespObj.message = "New version.";
                    apiVersionRespObj.data = new APIVersionData()
                    {
                        api_version = apiVersion
                    };
                }
                return Ok(apiVersionRespObj);
                //=========x==============
            }
            catch (Exception ex)
            {
                apiVersionRespObj.isError = true;
                apiVersionRespObj.data.api_version = 0;

                try
                {
                    var error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");
                    apiVersionRespObj.message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description;

                    return Ok(apiVersionRespObj);
                }
                catch (Exception)
                {
                    apiVersionRespObj.message = MessageCollection.Failed;
                    return Ok(apiVersionRespObj);
                }
            }
        }


        /// <summary>
        /// API for change password
        /// </summary>
        /// <param name="changePassword">Requesting parameter with old password and new password</param>
        /// <returns>Return reuslt of logout request</returns>
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequests changePasswordReq)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(changePasswordReq.session_token))
                    throw new Exception("Invalid Session Token.");

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValid(changePasswordReq.new_password);

                if (validationResult.Item1 == false)
                    return Ok(new RACommonResponse
                    {
                        result = validationResult.Item1,
                        message = validationResult.Item2
                    });
                else
                    return Ok(_bLLUserAuthenticaion.ChangePassword(changePasswordReq));
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// API for change password
        /// </summary>
        /// <param name="changePassword">Requesting parameter with old password and new password</param>
        /// <returns>Return reuslt of logout request</returns>
        [HttpPost]
        [Route("ChangePasswordV2")]
        public async Task<IActionResult> ChangePasswordV2([FromBody] ChangePasswordRequests changePasswordReq)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(changePasswordReq.session_token))
                    throw new Exception("Invalid Session Token.");

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValidV2(changePasswordReq.new_password);

                if (validationResult.Item1 == false)
                    return Ok(new RACommonResponse
                    {
                        result = validationResult.Item1,
                        message = validationResult.Item2
                    });
                else
                    return Ok(_bLLUserAuthenticaion.ChangePasswordV2(changePasswordReq));
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// API for change password
        /// </summary>
        /// <param name="changePassword">Requesting parameter with old password and new password</param>
        /// <returns>Return reuslt of logout request</returns>
        [HttpPost]
        [Route("ChangePasswordV3")]
        public async Task<IActionResult> ChangePasswordV3([FromBody] ChangePasswordRequests changePasswordReq)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(changePasswordReq.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                var validationResult = await _bLLUserAuthenticaion.IsPasswordFormatValidV2(changePasswordReq.new_password);

                if (validationResult.Item1 == true)
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = validationResult.Item1,
                        message = validationResult.Item2
                    });
                else
                    return Ok(_bLLUserAuthenticaion.ChangePasswordV4(changePasswordReq));
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// Get-Password-Length
        /// </summary>
        /// <param name="changePassword"></param>
        /// <returns>Return reuslt of logout request</returns>
        [HttpPost]
        [Route("GetPasswordLength")]
        public async Task<IActionResult> GetPasswordLength([FromBody] RACommonRequest raRequest)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(raRequest.session_token))
                    throw new Exception("Invalid Session Token.");

                return Ok(_bLLUserAuthenticaion.GetPasswordLength());
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }


        /// <summary>
        /// Get-Password-Length
        /// </summary>
        /// <param name="changePassword"></param>
        /// <returns>Return reuslt of logout request</returns>
        [HttpPost]
        [Route("GetPasswordLengthV2")]
        public async Task<IActionResult> GetPasswordLengthV2([FromBody] RACommonRequest radReq)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(radReq.session_token))
                    throw new Exception("Invalid Session Token.");

                return Ok(_bLLUserAuthenticaion.GetPasswordLengthV2());
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// Get-Password-Length 
        /// </summary>
        /// <param name="changePassword"></param>
        /// <returns>Return reuslt of logout request</returns>
        [HttpPost]
        [Route("GetPasswordLengthV3")]
        public async Task<IActionResult> GetPasswordLengthV3([FromBody] RACommonRequest radReq)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            RAPassLenResponse rAPassLenResponse = new RAPassLenResponse();
            RAPassLenResponseV2 rAPass = new RAPassLenResponseV2();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(radReq.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                rAPassLenResponse = await _bLLUserAuthenticaion.GetPasswordLengthV2();

                return Ok(new RAPassLenResponseV2()
                {
                    data = new PasswordLenthData()
                    {
                        length = rAPassLenResponse.length,
                    },
                    isError = rAPassLenResponse.result == true ? false : true,
                    message = rAPassLenResponse.message
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }


        [HttpPost]
        [Route("ForgetPwd")]
        public async Task<IActionResult> ForgetPwd(VMUserInfoForForgetPWD model)
        {
            RACommonResponse raResp = new RACommonResponse();
            try
            {
                var userInfo = await _bLLUserAuthenticaion.GetUserMobileNoAndNewPWD(model.user_name);

                if (userInfo.user_id > 0)
                {
                    raResp = await _bLLUserAuthenticaion.FORGETPWD(new VMForgetPWD()
                    {
                        user_id = userInfo.user_id,
                        mobile_no = userInfo.mobile_no,
                        new_pwd = userInfo.PWD,
                        new_hashed_pwd = Cryptography.Encrypt(userInfo.PWD, true)
                    });

                    return Ok(raResp);
                }
                else
                {
                    raResp.result = false;
                    raResp.message = userInfo.message;
                    return Ok(raResp);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }

        [HttpPost]
        [Route("ForgetPwdV2")]
        public async Task<IActionResult> ForgetPwdV2(VMUserInfoForForgetPWD model)
        {
            RACommonResponse raResp = new RACommonResponse();
            try
            {
                var userInfo = await _bLLUserAuthenticaion.GetUserMobileNoAndNewPWDV2(model.user_name);

                if (userInfo.user_id > 0)
                {
                    int isEligible = 0;
                    try
                    {
                        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                        isEligible = Convert.ToInt32(configuration.GetSection("AppSettings:IsEligibleAES").Value);
                    }
                    catch { }

                    if (isEligible == 1)
                    {
                        bool isEligibleUser = await _bLLUserAuthenticaion.IsAESEligibleUser(model.user_name);
                        if (isEligibleUser == true)
                        {
                            raResp = await _bLLUserAuthenticaion.FORGETPWDV2(new VMForgetPWD()
                            {
                                user_id = userInfo.user_id,
                                mobile_no = userInfo.mobile_no,
                                new_pwd = userInfo.PWD,
                                new_hashed_pwd = AESCryptography.Encrypt(userInfo.PWD)
                            });
                        }
                        else
                        {
                            raResp = await _bLLUserAuthenticaion.FORGETPWDV2(new VMForgetPWD()
                            {
                                user_id = userInfo.user_id,
                                mobile_no = userInfo.mobile_no,
                                new_pwd = userInfo.PWD,
                                new_hashed_pwd = Cryptography.Encrypt(userInfo.PWD, true)
                            });
                        }
                    }
                    else
                    {
                        raResp =await _bLLUserAuthenticaion.FORGETPWDV2(new VMForgetPWD()
                        {
                            user_id = userInfo.user_id,
                            mobile_no = userInfo.mobile_no,
                            new_pwd = userInfo.PWD,
                            new_hashed_pwd = AESCryptography.Encrypt(userInfo.PWD)
                        });
                    }

                    return Ok(raResp);
                }
                else
                {
                    raResp.result = false;
                    raResp.message = userInfo.message;
                    return Ok(raResp);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }

        [HttpPost]
        [Route("ForgetPwdV3")]
        public async Task<IActionResult> ForgetPwdV3(VMUserInfoForForgetPWD model)
        {
            RACommonResponseRevamp raResp = new RACommonResponseRevamp();
            try
            {
                var userInfo = await _bLLUserAuthenticaion.GetUserMobileNoAndNewPWDV2(model.user_name);

                if (userInfo.user_id > 0)
                {
                    int isEligible = 0;
                    try
                    {
                        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                        isEligible = Convert.ToInt32(configuration.GetSection("AppSettings:IsEligibleAES").Value);
                    }
                    catch { }

                    if (isEligible == 1)
                    {
                        bool isEligibleUser = await _bLLUserAuthenticaion.IsAESEligibleUser(model.user_name);
                        if (isEligibleUser == true)
                        {
                            raResp = await _bLLUserAuthenticaion.FORGETPWDV3(new VMForgetPWD()
                            {
                                user_id = userInfo.user_id,
                                mobile_no = userInfo.mobile_no,
                                new_pwd = userInfo.PWD,
                                new_hashed_pwd = AESCryptography.Encrypt(userInfo.PWD)
                            });
                        }
                        else
                        {
                            raResp = await _bLLUserAuthenticaion.FORGETPWDV3(new VMForgetPWD()
                            {
                                user_id = userInfo.user_id,
                                mobile_no = userInfo.mobile_no,
                                new_pwd = userInfo.PWD,
                                new_hashed_pwd = Cryptography.Encrypt(userInfo.PWD, true)
                            });
                        }
                    }
                    else
                    {
                        raResp = await _bLLUserAuthenticaion.FORGETPWDV3(new VMForgetPWD()
                        {
                            user_id = userInfo.user_id,
                            mobile_no = userInfo.mobile_no,
                            new_pwd = userInfo.PWD,
                            new_hashed_pwd = AESCryptography.Encrypt(userInfo.PWD)
                        });
                    }

                    return Ok(raResp);
                }
                else
                {
                    raResp.isError = true;
                    raResp.message = userInfo.message;
                    return Ok(raResp);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription error;
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = "Failed! " + ex.Message
                    });
                }
            }
        }


        private string GetEncriptedSecurityToken(string loginProvider, string userId, string userName, string distributorCode, object? deviceId)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(loginProvider))
                    throw new ArgumentNullException(nameof(loginProvider), "Login provider cannot be null or empty.");
                if (string.IsNullOrEmpty(userId))
                    throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentNullException(nameof(userName), "User name cannot be null or empty.");
                if (string.IsNullOrEmpty(distributorCode))
                    throw new ArgumentNullException(nameof(distributorCode), "Distributor code cannot be null or empty.");
                if (string.IsNullOrEmpty(StringFormatCollection.AccessTokenFormat))
                    throw new ArgumentNullException(nameof(StringFormatCollection.AccessTokenFormat), "Format string cannot be null or empty.");

                // Safely handle deviceId
                string deviceIdValue = deviceId?.ToString() ?? string.Empty;

                // Generate formatted string
                string formattedToken = String.Format(StringFormatCollection.AccessTokenFormat, loginProvider, userId, userName, distributorCode, deviceIdValue);

                // Ensure formatted string is not null before encryption
                if (formattedToken == null)
                    throw new InvalidOperationException("Formatted token string is null.");

                // Encrypt the formatted string
                return Cryptography.Encrypt(formattedToken, true);
            }
            catch (Exception ex)
            {
                // Wrap exception with context for better debugging
                throw new InvalidOperationException("Failed to generate encrypted security token.", ex);
            }
        }

        private string GetEncriptedSecurityTokenV2(string loginProvider, string userId, string userName, string distributorCode, object? deviceId)
        {
            if (string.IsNullOrEmpty(loginProvider) ||
        string.IsNullOrEmpty(userId) ||
        string.IsNullOrEmpty(userName) ||
        string.IsNullOrEmpty(distributorCode))
            {
                throw new ArgumentException("Required parameters cannot be null or empty.");
            }

            try
            {
                string formattedToken = String.Format(
                    StringFormatCollection.AccessTokenFormatV2,
                    loginProvider,
                    userId,
                    userName,
                    distributorCode,
                    deviceId,
                    Guid.NewGuid()
                );

                if (string.IsNullOrEmpty(formattedToken))
                {
                    throw new InvalidOperationException("Formatted token string is null or empty.");
                }

                string encryptedToken = AESCryptography.Encrypt(formattedToken);

                if (string.IsNullOrEmpty(encryptedToken))
                {
                    throw new InvalidOperationException("Encryption returned a null or empty value.");
                }

                return encryptedToken;
            }
            catch (Exception) // Avoid `throw ex;` to preserve stack trace
            {
                throw;
            }
        }

        /// <summary>
        /// Reseller Login [without password] 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        //[ResponseType(typeof(LogInResponse))]
        [GzipCompression]
        [ValidateModel]
        [Route("ResellerLogin")]
        public async Task<IActionResult> ResellerLogin([FromBody] ResellerLoginRequests login)
        {
            try
            {
                ResellerLoginUserInfoResponse user = await _bLLUserAuthenticaion.ValidateUser(login.UserName);

                if (user.user_name == null)
                {
                    return Ok(new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }

                string loginProviderId = await _bLLUserAuthenticaion.IsUserCurrentlyLoggedIn(user.user_id);

                UserLogInAttempt loginAtmInfo;
                string loginProvider = Guid.NewGuid().ToString();

                if (String.IsNullOrEmpty(loginProviderId))
                {

                    loginAtmInfo = new UserLogInAttempt()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion
                    };
                }
                else
                {
                    loginProvider = loginProviderId;

                    loginAtmInfo = new UserLogInAttempt()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion
                    };
                }

                Thread logThread = new Thread(() => _bLLUserAuthenticaion.SaveLoginAtmInfo(loginAtmInfo));
                logThread.Start();

                return Ok(new LogInResponse()
                {
                    SessionToken = GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId),
                    ISAuthenticate = true,
                    AuthenticationMessage = MessageCollection.UserValidted,
                    UserName = login.UserName,
                    Password = user.password,
                    DeviceId = login.DeviceId,
                    HasUpdate = false,
                    MinimumScore = SettingsValues.GetFPDefaultScore(),
                    OptionalMinimumScore = "30",
                    MaximumRetry = "2",
                    RoleAccess = user.role_access,
                    ChannelId = user.channel_id,
                    ChannelName = user.channel_name,
                    InventoryId = user.inventory_id,
                    CenterCode = user.center_code,
                    //Designation = user.designation,
                    is_etsaf_validation_need = SettingsValues.GetETSAFValidationValue()
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }

        /// <summary>
        /// Reseller Login [without password] 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        //[ResponseType(typeof(LogInResponse))]
        [GzipCompression]
        [ValidateModel]
        [Route("ResellerLoginV2")]
        public async Task<IActionResult> ResellerLoginV2([FromBody] ResellerLoginRequests login)
        {
            string secreteKey = string.Empty;

            secreteKey = SettingsValues.GetJWTSequrityKey();

            TokenService token = new TokenService(secreteKey);
            try
            {
                ResellerLoginUserInfoResponse user = await _bLLUserAuthenticaion.ValidateUser(login.UserName);

                if (user.user_name == null)
                {
                    return Ok(new LogInResponse()
                    {
                        ISAuthenticate = false,
                        AuthenticationMessage = MessageCollection.InvalidUserCridential,
                        HasUpdate = false,
                    });
                }

                string loginProviderId = await _bLLUserAuthenticaion.IsUserCurrentlyLoggedIn(user.user_id);

                UserLogInAttempt loginAtmInfo;
                string loginProvider = Guid.NewGuid().ToString();

                if (String.IsNullOrEmpty(loginProviderId))
                {

                    loginAtmInfo = new UserLogInAttempt()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion
                    };
                }
                else
                {
                    loginProvider = loginProviderId;

                    loginAtmInfo = new UserLogInAttempt()
                    {
                        userid = user.user_id,
                        is_success = 1,
                        ip_address = GetIP(),
                        loginprovider = loginProvider,
                        deviceid = login.DeviceId,
                        lan = login.Lan,
                        versioncode = login.VersionCode,
                        versionname = login.VersionName,
                        osversion = login.OSVersion,
                        kernelversion = login.KernelVersion,
                        fermwarevirsion = login.FermwareVersion
                    };
                }

                Thread logThread = new Thread(async () => await _bLLUserAuthenticaion.SaveLoginAtmInfo(loginAtmInfo));
                logThread.Start();

                return Ok(new LogInResponse()
                {
                    SessionToken =token.GenerateTokenV2(user, loginProvider), // GetEncriptedSecurityToken(loginProvider, user.user_id, user.user_name, user.distributor_code, login.DeviceId), 
                    ISAuthenticate = true,
                    AuthenticationMessage = MessageCollection.UserValidted,
                    UserName = login.UserName,
                    Password = user.password,
                    DeviceId = login.DeviceId,
                    HasUpdate = false,
                    MinimumScore = SettingsValues.GetFPDefaultScore(),
                    OptionalMinimumScore = "30",
                    MaximumRetry = "2",
                    RoleAccess = user.role_access,
                    ChannelId = user.channel_id,
                    ChannelName = user.channel_name,
                    InventoryId = user.inventory_id,
                    CenterCode = user.center_code,
                    is_etsaf_validation_need = SettingsValues.GetETSAFValidationValue()
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponse
                    {
                        result = false,
                        message = ex.Message
                    });
                }
            }
        }


        /// <summary>
        /// DBSS OTP Validation API for Reseller App
        /// </summary>
        /// <param name="otpValidationReq"></param>
        /// <returns></returns>
        //[ResponseType(typeof(OTPResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateDBSSOTP")]
        public async Task<IActionResult> ValidateDBSSOTPV1([FromBody] DBSSOTPValidationReq otpValidationReq)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityToken(otpValidationReq.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                OTPResponse otpResponse = await _bio.ValidateOTP(new DBSSOTPValidationRequest()
                {
                    otp = otpValidationReq.otp,
                    poc_msisdn = ConverterHelper.MSISDNCountryCodeAddition(otpValidationReq.src_msisdn, FixedValueCollection.MSISDNCountryCode),
                    auth_msisdn = ConverterHelper.MSISDNCountryCodeAddition(otpValidationReq.dest_msisdn, FixedValueCollection.MSISDNCountryCode),
                    purpose = Convert.ToInt16(otpValidationReq.purpose_number)
                }, otpValidationReq.retailer_id);

                return Ok(otpResponse);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        //[ResponseType(typeof(OTPResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateDBSSOTPV2")]
        public async Task<IActionResult> ValidateDBSSOTPV2([FromBody] DBSSOTPValidationReq otpValidationReq)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenV2(otpValidationReq.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                OTPResponse otpResp = await _bio.ValidateOTP(new DBSSOTPValidationRequest()
                {
                    otp = otpValidationReq.otp,
                    poc_msisdn = ConverterHelper.MSISDNCountryCodeAddition(otpValidationReq.src_msisdn, FixedValueCollection.MSISDNCountryCode),
                    auth_msisdn = ConverterHelper.MSISDNCountryCodeAddition(otpValidationReq.dest_msisdn, FixedValueCollection.MSISDNCountryCode),
                    purpose = Convert.ToInt16(otpValidationReq.purpose_number)
                }, otpValidationReq.retailer_id);

                return Ok(otpResp);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        //[ResponseType(typeof(OTPResponse))]
        [HttpPost]
        [ValidateModel]
        [Route("ValidateDBSSOTPV3")]
        public async Task<IActionResult> ValidateDBSSOTPV3([FromBody] DBSSOTPValidationReq otpValidationReq)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(otpValidationReq.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                OTPResponseRev otpResp = await _bio.ValidateOTPV2(new DBSSOTPValidationRequest()
                {
                    otp = otpValidationReq.otp,
                    poc_msisdn = ConverterHelper.MSISDNCountryCodeAddition(otpValidationReq.src_msisdn, FixedValueCollection.MSISDNCountryCode),
                    auth_msisdn = ConverterHelper.MSISDNCountryCodeAddition(otpValidationReq.dest_msisdn, FixedValueCollection.MSISDNCountryCode),
                    purpose = Convert.ToInt16(otpValidationReq.purpose_number)
                }, otpValidationReq.retailer_id);

                return Ok(otpResp);
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("ValidateBPOTP")]
        public async Task<IActionResult> ValidateBPOTP([FromBody] BPOtpValidationReq bPOtpValidationReq)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenForBPLogin(bPOtpValidationReq.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string loginProviderId = _bio.GetDecryptedSecurityToken(bPOtpValidationReq.session_token);

                if (loginProviderId.Equals("Fail"))
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "Invalid Security Token"
                    });
                }

                BPOtpValidationRes otpResp = new BPOtpValidationRes();

                otpResp =await _bLLUserAuthenticaion.ValidateBPOtp(Convert.ToDecimal(bPOtpValidationReq.bp_otp), Convert.ToDecimal(bPOtpValidationReq.retailer_otp), loginProviderId);

                return Ok(new RACommonResponse()
                {
                    result = otpResp.is_otp_valid,
                    message = otpResp.err_msg
                });
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }


        [HttpPost]
        [ValidateModel]
        [Route("ValidateBPOTPV2")]
        public async Task<IActionResult> ValidateBPOTPV2([FromBody] BPOtpValidationReq bPOtpValidationReq)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(bPOtpValidationReq.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                BPOtpValidationRes otpResp = new BPOtpValidationRes();

                otpResp = await _bLLUserAuthenticaion.ValidateBPOtpV2(Convert.ToDecimal(bPOtpValidationReq.bp_otp), Convert.ToDecimal(bPOtpValidationReq.retailer_otp), loginProviderId);

                return Ok(new RACommonResponseRevamp()
                {
                    isError = otpResp.is_otp_valid,
                    message = otpResp.err_msg
                });
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponseRevamp()
                {
                    isError = true,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("ResendBPOTP")]
        public async Task<IActionResult> ResendBPOTP([FromBody] RACommonRequest model)
        {
            try
            {
                if (!await _apiManager.ValidUserBySecurityTokenForBPLogin(model.session_token))
                    throw new Exception(MessageCollection.InvalidSecurityToken);

                string loginProviderId = _bio.GetDecryptedSecurityToken(model.session_token);

                if (loginProviderId.Equals("Fail"))
                {
                    return Ok(new RACommonResponse()
                    {
                        result = false,
                        message = "Invalid Security Token"
                    });
                }

                bool is_success = await _bLLUserAuthenticaion.ResendBPOTP(loginProviderId);

                if (is_success)
                {
                    return Ok(new RACommonResponse()
                    {
                        result = true,
                        message = "OTP Resent Successfully"
                    });
                }
                else
                {
                    return Ok(new RACommonResponse()
                    {
                        result = true,
                        message = "Failed to send OTP."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [ValidateModel]
        [Route("ResendBPOTPV2")]
        public async Task<IActionResult> ResendBPOTPV2([FromBody] RACommonRequest model)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                bool is_success = await _bLLUserAuthenticaion.ResendBPOTPV2(loginProviderId);

                if (is_success)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = false,
                        message = "OTP Resent Successfully"
                    });
                }
                else
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = "Failed to send OTP."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new RACommonResponse()
                {
                    result = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Reseller Login [without password] 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        //[ResponseType(typeof(LogInResponse))]
        [GzipCompression]
        [ValidateModel]
        [Route("Logout")]
        public async Task<IActionResult> Logout([FromBody] RACommonRequest model)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                Thread logThread = new Thread(async () => await _bLLUserAuthenticaion.Logout(loginProviderId));
                logThread.Start();

                return Ok(new RACommonResponseRevamp
                {
                    isError = false,
                    message = "Successfully logout!"
                });
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = !String.IsNullOrEmpty(error.error_custom_msg) ? error.error_custom_msg : error.error_description
                    });
                }
                catch (Exception)
                {
                    return Ok(new RACommonResponseRevamp
                    {
                        isError = true,
                        message = ex.Message
                    });
                }
            }
        }
    }
}
