using BIA.BLL.Utility;
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static BIA.Entity.ResponseEntity.CherishCategoryListResModel;

namespace BIA.BLL.BLLServices
{
    public class BLLCommon
    {
        private readonly DALBiometricRepo dataManager;
        private readonly BLLUserAuthenticaion _bLLUserAuthenticaion;
        private readonly IConfiguration _configuration;

        public BLLCommon(DALBiometricRepo _dataManager, BLLUserAuthenticaion bLLUserAuthenticaion, IConfiguration configuration)
        {
            dataManager = _dataManager;
            _bLLUserAuthenticaion = bLLUserAuthenticaion;
            _configuration = configuration;
        }
        public async Task<bool> IsStockAvailable(int stock_id, int channel_id)
        {
            try
            {
                var data = await dataManager.IsStockAvailable(stock_id, channel_id);

                return Convert.ToInt32(data.ToString()) == 1 ? true : false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<ActivityLogResponse> GetActivityLogData(int activity_type_id, string user_id)
        {
            ActivityLogResponse response = new ActivityLogResponse();
            try
            {
                var dataRow = await dataManager.GetActivityLogData(activity_type_id, user_id);

                if (dataRow.Rows.Count > 0)
                {
                    List<VMActivityLog> alrs = new List<VMActivityLog>();
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        VMActivityLog alr = new VMActivityLog();
                        alr.token_id = Convert.ToString(dataRow.Rows[i]["BI_TOKEN_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["BI_TOKEN_NUMBER"]);
                        alr.time = Convert.ToString(dataRow.Rows[i]["CREATE_DATE"] == DBNull.Value ? null : dataRow.Rows[i]["CREATE_DATE"]);
                        string msisdn = Convert.ToString(dataRow.Rows[i]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["MSISDN"]);
                        if(!String.IsNullOrEmpty(msisdn))
                            alr.mobile_number = msisdn.Substring(0, 2) == FixedValueCollection.MSISDNCountryCode ? msisdn.Remove(0, 2) : msisdn;
                        alr.nid = Convert.ToString(dataRow.Rows[i]["DEST_DOC_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOC_ID"]);
                        alr.dob = Convert.ToString(dataRow.Rows[i]["DEST_DOB"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOB"]);
                        alr.type = Convert.ToString(dataRow.Rows[i]["ACCU_TYPE"] == DBNull.Value ? null : dataRow.Rows[i]["ACCU_TYPE"]);

                        string statusName = Convert.ToString(dataRow.Rows[i]["STATUS_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["STATUS_NAME"]);
                        string errDescription = Convert.ToString(dataRow.Rows[i]["ERROR_DESCRIPTION"] == DBNull.Value ? null : dataRow.Rows[i]["ERROR_DESCRIPTION"]);
                        int isStatusNameNotAdd = Convert.ToInt32(dataRow.Rows[i]["IS_NOT_ADDED_STATUS"] == DBNull.Value ? null : dataRow.Rows[i]["IS_NOT_ADDED_STATUS"]);

                        if (!String.IsNullOrEmpty(statusName)
                           && statusName.Contains("Failed")
                           && !String.IsNullOrEmpty(errDescription))
                        {
                            if (isStatusNameNotAdd == 1)
                            {
                                alr.status = errDescription;
                            }
                            else
                            {
                                alr.status = statusName + ", " + errDescription;
                            }
                        }
                        else
                        {
                            alr.status = statusName;
                        }

                        alr.is_re_submittable = Convert.ToInt32(dataRow.Rows[i]["IS_RE_SUBMITTABLE"] == DBNull.Value ? null : dataRow.Rows[i]["IS_RE_SUBMITTABLE"]);
                        alr.re_submit_error_message = Convert.ToString(dataRow.Rows[i]["RE_SUBMIT_ERROR_MESSAGE"] == DBNull.Value ? null : dataRow.Rows[i]["RE_SUBMIT_ERROR_MESSAGE"]);
                        alr.re_submit_expire_time = Convert.ToInt32(dataRow.Rows[i]["ACTIVITYLOGEXPIRTIME"] == DBNull.Value ? null : dataRow.Rows[i]["ACTIVITYLOGEXPIRTIME"]);
                        alr.right_id = Convert.ToInt32(dataRow.Rows[i]["RIGHT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["RIGHT_ID"]);

                        alrs.Add(alr);
                    }

                    response.data = alrs;
                    response.result = true;
                    response.message = MessageCollection.Success;
                    return response;
                }
                else
                {
                    response.data = null;
                    response.result = false;
                    response.message = MessageCollection.NoDataFound;
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public async Task<ActivityLogResponse> GetActivityLogDataV2(int activity_type_id, string user_id)
        {
            ActivityLogResponse response = new ActivityLogResponse();
            try
            {
                var dataRow = await dataManager.GetActivityLogDataV2(activity_type_id, user_id);

                if (dataRow.Rows.Count > 0)
                {
                    List<VMActivityLog> alrs = new List<VMActivityLog>();
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        VMActivityLog alr = new VMActivityLog();
                        alr.token_id = Convert.ToString(dataRow.Rows[i]["BI_TOKEN_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["BI_TOKEN_NUMBER"]);
                        alr.time = Convert.ToString(dataRow.Rows[i]["CREATE_DATE"] == DBNull.Value ? null : dataRow.Rows[i]["CREATE_DATE"]);
                        string msisdn = Convert.ToString(dataRow.Rows[i]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["MSISDN"]);
                        if (!String.IsNullOrEmpty(msisdn))
                            alr.mobile_number = msisdn.Substring(0, 2) == FixedValueCollection.MSISDNCountryCode ? msisdn.Remove(0, 2) : msisdn;
                        alr.nid = Convert.ToString(dataRow.Rows[i]["DEST_DOC_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOC_ID"]);
                        alr.dob = Convert.ToString(dataRow.Rows[i]["DEST_DOB"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOB"]);
                        alr.type = Convert.ToString(dataRow.Rows[i]["ACCU_TYPE"] == DBNull.Value ? null : dataRow.Rows[i]["ACCU_TYPE"]);

                        string statusName = Convert.ToString(dataRow.Rows[i]["STATUS_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["STATUS_NAME"]);
                        string errDescription = Convert.ToString(dataRow.Rows[i]["ERROR_DESCRIPTION"] == DBNull.Value ? null : dataRow.Rows[i]["ERROR_DESCRIPTION"]);

                        int isStatusNameNotAdd = Convert.ToInt32(dataRow.Rows[i]["IS_NOT_ADDED_STATUS"] == DBNull.Value ? null : dataRow.Rows[i]["IS_NOT_ADDED_STATUS"]);

                        if (!String.IsNullOrEmpty(statusName)
                            && statusName.Contains("Failed")
                            && !String.IsNullOrEmpty(errDescription))
                        {
                            if (isStatusNameNotAdd == 1)
                            {
                                alr.status = errDescription;
                            }
                            else
                            {
                                alr.status = statusName + ", " + errDescription;
                            }
                        }
                        else
                        {
                            alr.status = statusName;
                        }

                        alr.is_re_submittable = Convert.ToInt32(dataRow.Rows[i]["IS_RE_SUBMITTABLE"] == DBNull.Value ? null : dataRow.Rows[i]["IS_RE_SUBMITTABLE"]);
                        alr.re_submit_error_message = Convert.ToString(dataRow.Rows[i]["RE_SUBMIT_ERROR_MESSAGE"] == DBNull.Value ? null : dataRow.Rows[i]["RE_SUBMIT_ERROR_MESSAGE"]);
                        alr.re_submit_expire_time = Convert.ToInt32(dataRow.Rows[i]["ACTIVITYLOGEXPIRTIME"] == DBNull.Value ? null : dataRow.Rows[i]["ACTIVITYLOGEXPIRTIME"]);
                        alr.right_id = Convert.ToInt32(dataRow.Rows[i]["RIGHT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["RIGHT_ID"]);

                        alr.is_bp_user = Convert.ToString(dataRow.Rows[i]["IS_BP"] == DBNull.Value ? null : dataRow.Rows[i]["IS_BP"]);
                        alr.bp_msisdn = Convert.ToString(dataRow.Rows[i]["BP_MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["BP_MSISDN"]);

                        alrs.Add(alr);
                    }

                    response.data = alrs;
                    response.result = true;
                    response.message = MessageCollection.Success;
                    return response;
                }
                else
                {
                    response.data = null;
                    response.result = false;
                    response.message = MessageCollection.NoDataFound;
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ActivityLogResponseRevamp> GetActivityLogDataV3(int activity_type_id, string user_id)
        {
            ActivityLogResponseRevamp response = new ActivityLogResponseRevamp();
            int isFtrFeatureOn = 0;
            isFtrFeatureOn = Convert.ToInt32(_configuration.GetSection("AppSettings:isFtrFeatureOn").Value);
            try
            {
                var dataRow = await dataManager.GetActivityLogDataV3(activity_type_id, user_id);

                if (dataRow.Rows.Count > 0)
                {
                    List<VMActivityLogRevamp> alrs = new List<VMActivityLogRevamp>();
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        VMActivityLogRevamp alr = new VMActivityLogRevamp();
                        alr.token_id = Convert.ToString(dataRow.Rows[i]["BI_TOKEN_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["BI_TOKEN_NUMBER"]);
                        alr.time = Convert.ToString(dataRow.Rows[i]["CREATE_DATE"] == DBNull.Value ? null : dataRow.Rows[i]["CREATE_DATE"]);
                        string msisdn = Convert.ToString(dataRow.Rows[i]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["MSISDN"]);
                        if (!String.IsNullOrEmpty(msisdn))
                            alr.mobile_number = msisdn.Substring(0, 2) == FixedValueCollection.MSISDNCountryCode ? msisdn.Remove(0, 2) : msisdn;
                        alr.nid = Convert.ToString(dataRow.Rows[i]["DEST_DOC_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOC_ID"]);
                        alr.dob = Convert.ToString(dataRow.Rows[i]["DEST_DOB"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOB"]);
                        alr.type = Convert.ToString(dataRow.Rows[i]["ACCU_TYPE"] == DBNull.Value ? null : dataRow.Rows[i]["ACCU_TYPE"]);

                        string statusName = Convert.ToString(dataRow.Rows[i]["STATUS_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["STATUS_NAME"]);
                        string errDescription = Convert.ToString(dataRow.Rows[i]["ERROR_DESCRIPTION"] == DBNull.Value ? null : dataRow.Rows[i]["ERROR_DESCRIPTION"]);

                        int isStatusNameNotAdd = Convert.ToInt32(dataRow.Rows[i]["IS_NOT_ADDED_STATUS"] == DBNull.Value ? null : dataRow.Rows[i]["IS_NOT_ADDED_STATUS"]);

                        if (!String.IsNullOrEmpty(statusName)
                            && statusName.Contains("Failed")
                            && !String.IsNullOrEmpty(errDescription))
                        {
                            if (isStatusNameNotAdd == 1)
                            {
                                alr.status = errDescription;
                            }
                            else
                            {
                                alr.status = statusName + ", " + errDescription;
                            }
                        }
                        else
                        {
                            alr.status = statusName;
                        }

                        alr.is_re_submittable = Convert.ToInt32(dataRow.Rows[i]["IS_RE_SUBMITTABLE"] == DBNull.Value ? null : dataRow.Rows[i]["IS_RE_SUBMITTABLE"]);
                        alr.re_submit_error_message = Convert.ToString(dataRow.Rows[i]["RE_SUBMIT_ERROR_MESSAGE"] == DBNull.Value ? null : dataRow.Rows[i]["RE_SUBMIT_ERROR_MESSAGE"]);
                        alr.re_submit_expire_time = Convert.ToInt32(dataRow.Rows[i]["ACTIVITYLOGEXPIRTIME"] == DBNull.Value ? null : dataRow.Rows[i]["ACTIVITYLOGEXPIRTIME"]);
                        alr.right_id = Convert.ToInt32(dataRow.Rows[i]["RIGHT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["RIGHT_ID"]);

                        alr.is_bp_user = Convert.ToString(dataRow.Rows[i]["IS_ARRANGED"] == DBNull.Value ? null : dataRow.Rows[i]["IS_ARRANGED"]);
                        alr.bp_msisdn = Convert.ToString(dataRow.Rows[i]["BP_MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["BP_MSISDN"]);
                        alr.designation = Convert.ToString(dataRow.Rows[i]["DESIGNATION"] == DBNull.Value ? null : dataRow.Rows[i]["DESIGNATION"]);
                        alr.action_point = Convert.ToString(dataRow.Rows[i]["ACTION_POINT"] == DBNull.Value ? null : dataRow.Rows[i]["ACTION_POINT"]);
                        if (!String.IsNullOrEmpty(alr.action_point)){
                            if (Convert.ToChar(alr.action_point.Substring(0, 1)) == ',')
                            {
                                alr.action_point = alr.action_point.Substring(1);
                            }
                        }
                        if (isFtrFeatureOn == 1)
                        {
                            alr.recharge_status = Convert.ToString(dataRow.Rows[i]["RECHARGE_MESSAGE"] == DBNull.Value ? null : dataRow.Rows[i]["RECHARGE_MESSAGE"]);
                            alr.is_recharge_done = Convert.ToInt32(dataRow.Rows[i]["ISRECHARGE_DONE"] == DBNull.Value ? null : dataRow.Rows[i]["ISRECHARGE_DONE"]);
                        }

                        alrs.Add(alr);
                    }

                    response.data = alrs;
                    response.isError = false;
                    response.message = MessageCollection.Success;
                    return response;
                }
                else
                {
                    response.data = null;
                    response.isError = true;
                    response.message = MessageCollection.NoDataFound;
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<PurposeNumberReponse> GetPurposeNumbers(RAGetPurposeRequest model)
        {
            List<PurposeNumberReponseData> pns = new List<PurposeNumberReponseData>();
            PurposeNumberReponse pnRes = new PurposeNumberReponse();
            try
            {

                var dataRow = await dataManager.GetPurposeNumbers(model);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {

                        PurposeNumberReponseData pn = new PurposeNumberReponseData();
                        pn.purpose_id = Convert.ToInt32(dataRow.Rows[i]["PURPOSE_ID"] == DBNull.Value ? null : dataRow.Rows[i]["PURPOSE_ID"]);
                        pn.purpose_name = Convert.ToString(dataRow.Rows[i]["PURPOSE_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["PURPOSE_NAME"]);
                        pns.Add(pn);
                    }

                    pnRes.data = pns;
                    pnRes.result = true;
                    pnRes.message = MessageCollection.Success;
                }
                else
                {
                    pnRes.data = pns;
                    pnRes.result = false;
                    pnRes.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pnRes;
        }

        public async Task<PurposeNumberReponseRev> GetPurposeNumbersV2(RAGetPurposeRequest model)
        {
            List<PurposeNumberReponseDataRev> pns = new List<PurposeNumberReponseDataRev>();
            PurposeNumberReponseRev pnRes = new PurposeNumberReponseRev();
            try
            {
                var dataRow = await dataManager.GetPurposeNumbers(model);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {

                        PurposeNumberReponseDataRev pn = new PurposeNumberReponseDataRev();
                        pn.purpose_id = Convert.ToInt32(dataRow.Rows[i]["PURPOSE_ID"] == DBNull.Value ? null : dataRow.Rows[i]["PURPOSE_ID"]);
                        pn.purpose_name = Convert.ToString(dataRow.Rows[i]["PURPOSE_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["PURPOSE_NAME"]);
                        pns.Add(pn);
                    }

                    pnRes.data = pns;
                    pnRes.isError = false;
                    pnRes.message = MessageCollection.Success;
                }
                else
                {
                    pnRes.data = pns;
                    pnRes.isError = true;
                    pnRes.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return pnRes;
        }

        public async Task<long> GetTokenNo(string mssisdn)
        {
            long result = 0;
            try
            {
                result = Convert.ToInt32(mssisdn);
                result = await dataManager.GetTokenNo(mssisdn);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        #region Get Distributor Code From Session Token

        public string GetDistributorCodeFromSessionToken(string sessiontoken)
        {
            string distCode = null;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 2)
                        {
                            distCode = splitedData[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return distCode;

            //string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
            //int indexOfKey = decryptedToken.IndexOf(",dc:");
            //string distCode = decryptedToken.Substring(indexOfKey + ",dc:".Length, decryptedToken.Length - (indexOfKey + ",dc:".Length));
        }

        public async Task<string> GetDistributorCodeFromSessionTokenV2(string sessiontoken, string userName)
        {
            string distCode = string.Empty;            
            try
            {
                int isEligible  = Convert.ToInt32(_configuration.GetSection("AppSettings:IsEligibleAES").Value);


                if (isEligible == 1)
                {
                    bool isEligibleUser = await _bLLUserAuthenticaion.IsAESEligibleUser(userName);
                    if (isEligibleUser == true)
                    {
                        distCode = GetDistCodeFromSesTokenForAES(sessiontoken);
                        return distCode;
                    }
                    else
                    {
                        distCode = GetDistCodeFromSesTokenForMD5(sessiontoken);
                        return distCode;
                    }
                }
                else
                {
                    distCode = GetDistCodeFromSesTokenForAES(sessiontoken);
                    return distCode;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
            //int indexOfKey = decryptedToken.IndexOf(",dc:");
            //string distCode = decryptedToken.Substring(indexOfKey + ",dc:".Length, decryptedToken.Length - (indexOfKey + ",dc:".Length));
        }

        private string GetDistCodeFromSesTokenForAES(string sessiontoken)
        {
            string distCode = string.Empty;
            try
            {
                string decryptedToken = AESCryptography.Decrypt(sessiontoken);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:", ",random:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 3)
                        {
                            distCode = splitedData[i];
                            break;
                        }
                    }
                }
                return distCode;
            }
            catch (Exception)
            {
                throw;
            }

        }
        private string GetDistCodeFromSesTokenForMD5(string sessiontoken)
        {
            string distCode = string.Empty;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 3)
                        {
                            distCode = splitedData[i];
                            break;
                        }
                    }
                }
                return distCode;
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

        #region Get User Id From Session Token

        public string GetUserIdFromSessionToken(string sessiontoken)
        {
            string userId = null;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 1)
                        {
                            userId = splitedData[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userId;
        }
        #endregion

        #region Get User Name From Session Token
        public string GetUserNameFromSessionToken(string sessiontoken)
        {
            string userName = String.Empty;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 2)
                        {
                            userName = splitedData[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userName;
        }
        public string GetUserNameFromSessionTokenV2(string sessiontoken)
        {
            string userName = String.Empty;

            try
            {
                string decriptedSecurityToken = string.Empty;
                string decriptedSecurityTokenMD5 = string.Empty;
                try
                {
                    decriptedSecurityToken = AESCryptography.Decrypt(sessiontoken);
                    if (decriptedSecurityToken.Equals("InvalidSessionToken"))
                    {
                        decriptedSecurityToken = string.Empty;
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(sessiontoken, true);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        decriptedSecurityTokenMD5 = Cryptography.Decrypt(sessiontoken, true);
                    }
                    catch (Exception)
                    {
                        return string.Empty;
                    }
                }
                if (!String.IsNullOrEmpty(decriptedSecurityTokenMD5))
                {
                    userName = GetUserNameFromMD5Token(decriptedSecurityTokenMD5);
                }
                else
                {
                    userName = GetUserNameFromAESToken(decriptedSecurityToken);
                }

                return userName;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private string GetUserNameFromAESToken(string sessiontoken)
        {
            string userName = String.Empty;
            try
            {
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:" };
                var splitedData = sessiontoken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 2)
                        {
                            userName = splitedData[i];
                            break;
                        }
                    }
                }

                return userName;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string GetUserNameFromMD5Token(string sessiontoken)
        {
            string userName = String.Empty;
            try
            {
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:" };
                var splitedData = sessiontoken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 2)
                        {
                            userName = splitedData[i];
                            break;
                        }
                    }
                }

                return userName;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Get Device Id From Session Token

        public string GetDeviceIdFromSessionToken(string sessiontoken)
        {
            string deviceId = null;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 1)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == 4)
                        {
                            deviceId = splitedData[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return deviceId;
        }
        #endregion

        #region Get Data From Security Token 

        public string GetDataFromSecurityToken(string decryptedSessiontoken, int tonkenPropertyIndex)
        {
            string data = null;
            try
            {
                //string decryptedToken = Cryptography.Decrypt(decryptedSessiontoken, true);

                string[] tokenProperties = StringFormatCollection.AccessTokenPropertyArray;
                var splitedDataList = decryptedSessiontoken.Split(tokenProperties, StringSplitOptions.None);

                if (tokenProperties.Length <= splitedDataList.Count()
                    && tonkenPropertyIndex <= splitedDataList.Count())
                {
                    for (int i = 0; i < splitedDataList.Count(); i++)
                    {
                        if (i == tonkenPropertyIndex)
                        {
                            data = splitedDataList[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        public string GetDataFromSecurityTokenV2(string decryptedSessiontoken, int tonkenPropertyIndex)
        {
            string data = null;
            try
            {
                //string decryptedToken = Cryptography.Decrypt(decryptedSessiontoken, true);

                string[] tokenProperties = StringFormatCollection.AccessTokenPropertyArrayV2;
                var splitedDataList = decryptedSessiontoken.Split(tokenProperties, StringSplitOptions.None);

                if (tokenProperties.Length <= splitedDataList.Count()
                    && tonkenPropertyIndex <= splitedDataList.Count())
                {
                    for (int i = 0; i < splitedDataList.Count(); i++)
                    {
                        if (i == tonkenPropertyIndex)
                        {
                            data = splitedDataList[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }

        public string GetDataFromSecurityTokenV3(string decryptedSessiontoken, int tonkenPropertyIndex)
        {
            string data = null;
            try
            {
                //string decryptedToken = Cryptography.Decrypt(decryptedSessiontoken, true);

                string[] tokenProperties = StringFormatCollection.AccessTokenPropertyArray;
                var splitedDataList = decryptedSessiontoken.Split(tokenProperties, StringSplitOptions.None);

                if (tokenProperties.Length <= splitedDataList.Count()
                    && tonkenPropertyIndex <= splitedDataList.Count())
                {
                    for (int i = 0; i < splitedDataList.Count(); i++)
                    {
                        if (i == tonkenPropertyIndex)
                        {
                            data = splitedDataList[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return data;
        }
        #endregion

        #region Get Login Provider From Session Token

        public string GetLoginProviderFromSessionToken(string sessiontoken, int tokenPropertyIndex)
        {
            string loginProvider = null;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:" };
                var splitedData = decryptedToken.Split(tokenProperties, StringSplitOptions.None);
                if (splitedData.Count() > 0)
                {
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i == tokenPropertyIndex)
                        {
                            loginProvider = splitedData[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginProvider;
        }
        #endregion

        #region Check Security Token Format

        public bool CheckSecurityTokenFormat(string sessiontoken)
        {
            bool reasult = false;
            try
            {
                string decryptedToken = Cryptography.Decrypt(sessiontoken, true);
                string[] tokenProperties = StringFormatCollection.AccessTokenPropertyArray;

                for (int i = 0; i < tokenProperties.Length; i++)
                {
                    if (decryptedToken.Contains(tokenProperties[i]))
                    {
                        int strIndex = decryptedToken.IndexOf(tokenProperties[i]);
                        int tempStrIndex = 0;

                        if (strIndex > tempStrIndex)
                        {
                            tempStrIndex = strIndex;
                            reasult = true;
                            continue;
                        }
                        else
                        {
                            reasult = false;
                            break;
                        }
                    }
                    else
                    {
                        reasult = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return reasult;
        }

        public bool CheckSecurityTokenFormatV2(string sessiontoken)
        {
            bool reasult = false;
            try
            {
                string[] tokenProperties = StringFormatCollection.AccessTokenPropertyArrayV2;

                for (int i = 0; i < tokenProperties.Length; i++)
                {
                    if (sessiontoken.Contains(tokenProperties[i]))
                    {
                        int strIndex = sessiontoken.IndexOf(tokenProperties[i]);
                        int tempStrIndex = 0;

                        if (strIndex > tempStrIndex)
                        {
                            tempStrIndex = strIndex;
                            reasult = true;
                            continue;
                        }
                        else
                        {
                            reasult = false;
                            break;
                        }
                    }
                    else
                    {
                        reasult = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return reasult;
        }
        public bool CheckSecurityTokenFormatV3(string sessiontoken)
        {
            bool reasult = false;
            try
            {
                string[] tokenProperties = StringFormatCollection.AccessTokenPropertyArray;

                for (int i = 0; i < tokenProperties.Length; i++)
                {
                    if (sessiontoken.Contains(tokenProperties[i]))
                    {
                        int strIndex = sessiontoken.IndexOf(tokenProperties[i]);
                        int tempStrIndex = 0;

                        if (strIndex > tempStrIndex)
                        {
                            tempStrIndex = strIndex;
                            reasult = true;
                            continue;
                        }
                        else
                        {
                            reasult = false;
                            break;
                        }
                    }
                    else
                    {
                        reasult = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return reasult;
        }
        #endregion 
        public async Task<string> GetUnpairedMSISDNSearchDefaultValue(UnpairedMSISDNListReqModel model)
        {
            string msisdn = string.Empty;
            try
            {
                var dataRow = await dataManager.GetUnpairedMSISDNSearchDefaultValue(model);

                if (dataRow.Rows.Count > 0)
                {
                    msisdn = dataRow.Rows[0]["MSISDNPFX"] == DBNull.Value ? null : dataRow.Rows[0]["MSISDNPFX"].ToString();                   
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return msisdn;
        } 

        public async Task<string> GetUnpairedMSISDNSearchDefaultValueV2(UnpairedMSISDNListReqModel model)
        {
            string msisdn = string.Empty;
            try
            {
                var dataRow = await dataManager.GetUnpairedMSISDNSearchDefaultValueV2(model);

                if (dataRow.Rows.Count > 0)
                {
                    msisdn = dataRow.Rows[0]["MSISDNPFX"] == DBNull.Value ? null : dataRow.Rows[0]["MSISDNPFX"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return msisdn;
        }

        public async Task<string> GetUnpairedMSISDNSearchDefaultValueCherished(UnpairedMSISDNListReqModelV2 model)
        {
            string msisdn = string.Empty;
            try
            {
                var dataRow = await dataManager.GetUnpairedMSISDNSearchDefaultValueCherished(model);

                if (dataRow.Rows.Count > 0)
                {
                    msisdn = dataRow.Rows[0]["MSISDNPFX"] == DBNull.Value ? null : dataRow.Rows[0]["MSISDNPFX"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return msisdn;
        }

        public async Task<ChannelWiseResponse> GetPaymentMethod(RAGetPaymentMehtodRequest model)
        {
            List<ChannelWiseResponseData> cws = new List<ChannelWiseResponseData>();
            ChannelWiseResponse cwRes = new ChannelWiseResponse();
            try
            {
                var dataRow = await dataManager.GetPaymentMethod(model);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {

                        ChannelWiseResponseData cw = new ChannelWiseResponseData();
                        cw.payment_amount = Convert.ToString(dataRow.Rows[i]["PAYMENT_AMOUNT"] == DBNull.Value ? null : dataRow.Rows[i]["PAYMENT_AMOUNT"]);
                        cw.payment_method = Convert.ToString(dataRow.Rows[i]["PAYMENT_METHOD"] == DBNull.Value ? null : dataRow.Rows[i]["PAYMENT_METHOD"]);
                        cws.Add(cw);
                    }

                    cwRes.data = cws;
                    cwRes.result = true;
                    cwRes.message = MessageCollection.Success;
                }
                else
                {
                    cwRes.data = cws;
                    cwRes.result = false;
                    cwRes.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return cwRes;
        }

        public async Task<ChannelWiseResponseRev> GetPaymentMethodV2(RAGetPaymentMehtodRequest model, string userName)
        {
            List<ChannelWiseResponseDataRev> cws = new List<ChannelWiseResponseDataRev>();
            ChannelWiseResponseRev cwRes = new ChannelWiseResponseRev();
            try
            {
                var dataRow = await dataManager.GetPaymentMethodV2(model, userName);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {

                        ChannelWiseResponseDataRev cw = new ChannelWiseResponseDataRev();
                        cw.payment_amount = Convert.ToString(dataRow.Rows[i]["PAYMENT_AMOUNT"] == DBNull.Value ? null : dataRow.Rows[i]["PAYMENT_AMOUNT"]);
                        cw.payment_method = Convert.ToString(dataRow.Rows[i]["PAYMENT_METHOD"] == DBNull.Value ? null : dataRow.Rows[i]["PAYMENT_METHOD"]);
                        cws.Add(cw);
                    }

                    cwRes.data = cws;
                    cwRes.isError = false;
                    cwRes.message = MessageCollection.Success;
                }
                else
                {
                    cwRes.data = cws;
                    cwRes.isError = true;
                    cwRes.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return cwRes;
        }

        public async Task<RechargeAmountData> GetRechargeAmount(RechargeAmountReqModel model, string userName)
        {
            List<RechargeAmountResponse> rechList = new List<RechargeAmountResponse>();
            RechargeAmountData rechargeAmnt = new RechargeAmountData();
            try
            { 
                DataTable dataRow = await dataManager.GetRechargeAmount(model, userName);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {                         
                        RechargeAmountResponse rcrg = new RechargeAmountResponse();
                        rcrg.rechargeAmount= Convert.ToDouble(dataRow.Rows[i]["AMOUNT"] == DBNull.Value ? null : dataRow.Rows[i]["AMOUNT"]);
                        rcrg.amountId = Convert.ToDouble(dataRow.Rows[i]["AMOUNTVALUE"] == DBNull.Value ? null : dataRow.Rows[i]["AMOUNTVALUE"]);
                        rechList.Add(rcrg);
                    }

                    rechargeAmnt.data = rechList;
                    rechargeAmnt.isError = false;
                    rechargeAmnt.message = MessageCollection.Success;
                }
                else
                {
                    rechargeAmnt.data = rechList;
                    rechargeAmnt.isError = true;
                    rechargeAmnt.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return rechargeAmnt;
        }

        public async Task<RechargeAmountData> GetRechargeAmountV2(RechargeAmountReqModelRev model, string userName)
        {
            List<RechargeAmountResponse> rechList = new List<RechargeAmountResponse>();
            RechargeAmountData rechargeAmnt = new RechargeAmountData();
            try
            {
                DataTable dataRow =await dataManager.GetRechargeAmountV2(model, userName);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        RechargeAmountResponse rcrg = new RechargeAmountResponse();
                        rcrg.rechargeAmount = Convert.ToDouble(dataRow.Rows[i]["AMOUNT"] == DBNull.Value ? null : dataRow.Rows[i]["AMOUNT"]);
                        rcrg.amountId = Convert.ToDouble(dataRow.Rows[i]["AMOUNTVALUE"] == DBNull.Value ? null : dataRow.Rows[i]["AMOUNTVALUE"]);
                        rechList.Add(rcrg);
                    }

                    rechargeAmnt.data = rechList;
                    rechargeAmnt.isError = false;
                    rechargeAmnt.message = MessageCollection.Success;
                }
                else
                {
                    rechargeAmnt.data = rechList;
                    rechargeAmnt.isError = true;
                    rechargeAmnt.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return rechargeAmnt;
        }
        public async Task<long> AppInfoUpdate(AppInfoUpdateReqModel model, string loginProvider)
        {
            long response = 0; 
            try
            {
                response = await dataManager.AppInfoUpdate(model,loginProvider);

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<BTSCode> GetBTSCode(SiteIdRequestModel model)
        {
            List<RechargeAmountResponse> rechList = new List<RechargeAmountResponse>();
            RechargeAmountData rechargeAmnt = new RechargeAmountData();
            BTSCode bTSCode = new BTSCode();    
            try
            {
                DataTable dataRow = await dataManager.GetBTSCode(model);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count;)
                    {
                        bTSCode.bts_code = Convert.ToString(dataRow.Rows[i]["BTS_CODE"]);
                        break;
                    }                  
                }                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return bTSCode;
        }

        public async Task<BlackListedWordModel> GetBlackListedWordForAddress()
        {
            RechargeAmountData rechargeAmnt = new RechargeAmountData();
            BlackListedWordModel blackListed = new BlackListedWordModel();
            try
            {
                DataTable dataTable = await dataManager.GetBlackListedWordForAddress();
                blackListed.data = dataTable.AsEnumerable().Select(row => row.Field<string>("ADDRESS_JUNK")).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return blackListed;
        }
        public async Task<BlackListedWordModel> GetBlackListedWordForName()
        {
            RechargeAmountData rechargeAmnt = new RechargeAmountData();
            BlackListedWordModel blackListed = new BlackListedWordModel();
            try
            {
                DataTable dataTable = await dataManager.GetBlackListedWordForName();
                blackListed.data = dataTable.AsEnumerable().Select(row => row.Field<string>("NAME_JUNK")).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return blackListed;
        }

        public async Task<ScannerInfoRespModel> GetScannerInfo(ScannerInfoReqModel model)
        {
            ScannerInfoRespModel scanner = new ScannerInfoRespModel();
            try
            {
                DataTable dataTable = await dataManager.GetScannerInfo(model);

                if (dataTable.Rows.Count > 0)
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        scanner.data = new ScannerData
                        {
                            is_bl_scanner = Convert.ToString(dataTable.Rows[i]["IS_BL_SCANNER"])
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return scanner;
        }
        public async Task<FTROfferIdRespModel> GetOfferIdforFTR(string channelName, string userId, string bi_token_number)
        {
            List<RechargeAmountResponse> rechList = new List<RechargeAmountResponse>();
            RechargeAmountData rechargeAmnt = new RechargeAmountData();
            FTROfferIdRespModel fTROfferId = new FTROfferIdRespModel();
            try
            {
                DataTable dataRow = await dataManager.GetOfferId(channelName, userId, bi_token_number);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        fTROfferId.offer_id = Convert.ToString(dataRow.Rows[i]["OFFERID"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return fTROfferId;
        }

        #region Cherish Number Sell
        public async Task<CherishCategoryListResModel> GetCherishCategoyListData(string channelName)
        {
            CherishCategoryListResModel response = new CherishCategoryListResModel();
            string defaultCategory = string.Empty;
            defaultCategory = _configuration.GetSection("AppSettings:default_category").Value;
            response.default_category=defaultCategory;
            try
            {
                var dataRow = await dataManager.GetCherishCategoryData(channelName);

                if (dataRow.Rows.Count > 0)
                {
                    List<CategoryList> alrs = new List<CategoryList>();
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        CategoryList data=new CategoryList();
                        data.category_id = dataRow.Rows[i]["NAME"] == DBNull.Value ? null : dataRow.Rows[i]["NAME"].ToString();
                        var amount = Convert.ToString(dataRow.Rows[i]["AMOUNT"] == DBNull.Value ? null : dataRow.Rows[i]["AMOUNT"]);
                        var message = Convert.ToString(dataRow.Rows[i]["MESSAGE"] == DBNull.Value ? null : dataRow.Rows[i]["MESSAGE"]);

                        data.category_Name =data.category_id+ " @"+message+" " +amount+"";

                        alrs.Add(data);
                    }

                    response.data = alrs;
                    response.isError = false;
                    response.message = MessageCollection.Success;
                    return response;
                }
                else
                {
                    response.data = null;
                    response.isError = true;
                    response.message = MessageCollection.NoDataFound;
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<CherishCategory> GetDesiredCategoryMessage(string categoryName, string channel_name)
        {
            CherishCategory res = new CherishCategory();
            try
            {
                var dataRow = await dataManager.GetDesiredCatMessage(categoryName,channel_name);

                if (dataRow.Rows.Count > 0)
                {
                        res.name = dataRow.Rows[0]["NAME"] == DBNull.Value ? null : dataRow.Rows[0]["NAME"].ToString();
                        res.channel_name = dataRow.Rows[0]["CHANNEL_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["CHANNEL_NAME"].ToString();
                        var amount = Convert.ToString(dataRow.Rows[0]["AMOUNT"] == DBNull.Value ? null : dataRow.Rows[0]["AMOUNT"]);
                    if (res != null && res.channel_name != null && res.channel_name.ToLower() == "B2C_postpaid".ToLower())
                    {
                        res.message = "This is " + res.name + " number; Applicable Recharge bundle " + res.name + "= " + amount +" Tk.";
                    }
                    else
                    {
                        res.message = "This is " + res.name + " number; Applicable Recharge amount " + res.name + "= " + amount + " Tk.";
                    }  
                }
                
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<SubscriptionTypeResModel> GetSubscriptionTypes(RASubscriptionTypeReq model)
        {
            List<SubscriptionTypeResData> subscriptions = new List<SubscriptionTypeResData>();
            SubscriptionTypeResModel subscriptionsRes = new SubscriptionTypeResModel();
            try
            {
                var dataRow = await dataManager.GetSubscriptionsTypes(model);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {

                        SubscriptionTypeResData pn = new SubscriptionTypeResData();
                        pn.subscription_id = Convert.ToInt32(dataRow.Rows[i]["SUBSCRIPTION_ID"] == DBNull.Value ? null : dataRow.Rows[i]["SUBSCRIPTION_ID"]);
                        pn.subscription_name = Convert.ToString(dataRow.Rows[i]["NAME"] == DBNull.Value ? null : dataRow.Rows[i]["NAME"]);
                        subscriptionsRes.data.Add(pn);


                        subscriptions.Add(pn);
                    }

                    subscriptionsRes.data = subscriptions;
                    subscriptionsRes.isError = false;
                    subscriptionsRes.message = MessageCollection.Success;
                }
                else
                {
                    subscriptionsRes.data = subscriptions;
                    subscriptionsRes.isError = true;
                    subscriptionsRes.message = MessageCollection.NoDataFound;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptionsRes;
        }

        public async Task<string> GetCategoryMinAmount(string category) 
        {
            string amount = string.Empty;
            try
            {
                var dataRow = await dataManager.GetCategoryMinAmount(category);

                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        amount = Convert.ToString(dataRow.Rows[0]["AMOUNT"] == DBNull.Value ? null : dataRow.Rows[0]["AMOUNT"]);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return amount;
        }
        #endregion
    }
}
