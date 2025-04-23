using BIA.Entity.Interfaces;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Connectivities
{ 
    public static class ConnectivitiesValues
    {
        static IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
       public static string GetConnectionString()
        {
            string connectionString = String.Empty;
            try
            {
                connectionString = configuration.GetSection("ConnectionStrings:BioDBConnectionString:DefaultConnectionString").Value;

                connectionString = AESCryptography.Decrypt(connectionString);
            }
            catch (Exception)
            {
                throw;
            }
            return connectionString;
        }

        public static string GetConnectionWithNameParam(string name)
        {
            string connectionString = String.Empty;
            try
            {
                connectionString = configuration.GetSection("ConnectionStrings:BioDBConnectionString:" + name).Value;

                connectionString = AESCryptography.Decrypt(connectionString);
            }
            catch (Exception)
            {
                throw;
            }
            return connectionString;
        }

        public static string GetDBSSBaseUrl()
        {
            string url = String.Empty;
            try
            {
                url = configuration.GetSection("AppSettings:dbssBaseUri").Value;

                url = AESCryptography.Decrypt(url);
            }
            catch (Exception)
            {

                throw;
            }
            return url;
        } 
        public static string GetDBSSBaseUrlWithaParam(string _baseURL)
        {
            string url = String.Empty;
            try
            {
                url = configuration.GetSection(_baseURL).Value; ;

                url = AESCryptography.Decrypt(url);
            }
            catch (Exception)
            {
                throw;
            }
            return String.Empty;
        }
         
        public static string GetJWTSecurityKey()
        {
            string secreteKey = String.Empty; 
            try
            {
                secreteKey = configuration.GetSection("AppSettings:tokenServiceKey").Value;

                secreteKey = AESCryptography.Decrypt(secreteKey);
            }
            catch (Exception)
            {

                throw;
            }
            return secreteKey;
        }
        public static string GetDMSBaseUrl()
        {
            string dmsBaseUsi = String.Empty;
            try
            {
                dmsBaseUsi = configuration.GetSection("AppSettings:dmsBaseUrl").Value;

                dmsBaseUsi = AESCryptography.Decrypt(dmsBaseUsi);
            }
            catch (Exception)
            {

                throw;
            }
            return dmsBaseUsi;
        } 
        public static string GetDMSUserName()
        {
            string dmsUserName = String.Empty;
            try
            {
                dmsUserName = configuration.GetSection("AppSettings:userName").Value;

                dmsUserName = AESCryptography.Decrypt(dmsUserName);
            }
            catch (Exception)
            {
                throw;
            }
            return dmsUserName;
        }
        public static string GetDMSPassword() 
        {
            string dmsPassword = String.Empty;
            try
            {
                dmsPassword = configuration.GetSection("AppSettings:dms_pas").Value;

                dmsPassword = AESCryptography.Decrypt(dmsPassword);
            }
            catch (Exception)
            {
                throw;
            }
            return dmsPassword;
        }
        public static string GetRetailerAppBaseUrl()
        {
            string retailBaseUrl = String.Empty; 
            try
            {
                retailBaseUrl = configuration.GetSection("AppSettings:RetilerBaseAPI").Value;

                retailBaseUrl = AESCryptography.Decrypt(retailBaseUrl);
            }
            catch (Exception)
            {
                throw;
            } 
            return retailBaseUrl;
        }
        public static string GetRSOBaseUrl()
        {
            string rsoBaseUrl = String.Empty;
            try
            {
                rsoBaseUrl = configuration.GetSection("AppSettings:RSOBaseAPI").Value;

                rsoBaseUrl = AESCryptography.Decrypt(rsoBaseUrl);
            }
            catch (Exception)
            {
                throw;
            }
            return rsoBaseUrl;
        }
        public static string GetSingleSourceBaseUrl()
        {  
            string singleSourceUrl = String.Empty;
            try
            {
                singleSourceUrl = configuration.GetSection("AppSettings:sigleSourceAPI").Value;

                singleSourceUrl = AESCryptography.Decrypt(singleSourceUrl);
            }
            catch (Exception)
            {
                throw;
            }
            return singleSourceUrl;
        }

        public static string GetSingleSourcePassword()
        {
            string singleSourcePassword = String.Empty;
            try
            {
                singleSourcePassword = configuration.GetSection("AppSettings:single_source_pas").Value;

                singleSourcePassword = AESCryptography.Decrypt(singleSourcePassword);
            }
            catch (Exception)
            {
                throw;
            }
            return singleSourcePassword;
        } 

        public static string GetSingleSourceUserName()
        {
            string singleSourceUserName = String.Empty;
            try
            {
                singleSourceUserName = configuration.GetSection("AppSettings:single_source_userName").Value;

                singleSourceUserName = AESCryptography.Decrypt(singleSourceUserName);
            }
            catch (Exception)
            {
                throw;
            }
            return singleSourceUserName; 
        }
        public static string RetailerUpdateStatusUserName()
        { 
            string updateStatusUserName = String.Empty;
            try 
            {
                updateStatusUserName = configuration.GetSection("AppSettings:biometricRETUserName").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return updateStatusUserName; 
        }
        public static string RetailerUpdateStatusPassword()
        {
            string updateStatusPassword = String.Empty; 
            try
            {
                updateStatusPassword = configuration.GetSection("AppSettings:biometricRETPas").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return updateStatusPassword;
        }

        public static string SingleSourceActiveMessage() 
        {
            string singleSorceMessage = String.Empty;
            try
            {
                singleSorceMessage = configuration.GetSection("AppSettings:single_source_active_message").Value;
            }
            catch (Exception)
            {
                throw;
            } 
            return singleSorceMessage; 
        }
        public static string GetRSOUserNameForRaiseComplaint()
        {
            string rsoUserName = String.Empty;
            try
            {
                rsoUserName = configuration.GetSection("AppSettings:rso_user_Name").Value;

                rsoUserName = AESCryptography.Decrypt(rsoUserName);
            }
            catch (Exception)
            {
                throw;
            }
            return rsoUserName;
        }
        public static int GetBTSCodeShowingOrNot()
        {
            int isBtsAllow = 0;
            try
            {
                isBtsAllow = Convert.ToInt32(configuration.GetSection("AppSettings:is_bts_show").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return isBtsAllow;
        }
         
        public static string GeteShopBaseURL()
        {
            string eShopBaseUrl = String.Empty;
            try
            {
                eShopBaseUrl = configuration.GetSection("AppSettings:eShopBaseUrl").Value;

                eShopBaseUrl = AESCryptography.Decrypt(eShopBaseUrl);
            }
            catch (Exception)
            {
                throw;
            }
            return eShopBaseUrl; 
        }
        public static string GeteShopCredential()
        {
            string eShopCredential = String.Empty;
            try
            {
                eShopCredential = configuration.GetSection("AppSettings:eShopCredential").Value;

                eShopCredential = AESCryptography.Decrypt(eShopCredential);
            }
            catch (Exception)
            {
                throw;
            }
            return eShopCredential;
        }
        public static string GetEVAPIUrl()
        {
            string BalanceCheckURL = String.Empty;
            try
            {
                BalanceCheckURL = configuration.GetSection("AppSettings:BalanceCheckURL").Value;
                BalanceCheckURL = AESCryptography.Decrypt(BalanceCheckURL);
            }
            catch (Exception)
            {
                throw;
            }
            return BalanceCheckURL;
        }
        public static string GetEVAPIQueryString()
        {
            string BalanceCheckQueryString = String.Empty;
            try
            {
                BalanceCheckQueryString = configuration.GetSection("AppSettings:BalanceCheckQueryString").Value;
                BalanceCheckQueryString = AESCryptography.Decrypt(BalanceCheckQueryString);
            }
            catch (Exception)
            {
                throw;
            }
            return BalanceCheckQueryString;
        }
        public static string GetEVAPIReqBody()
        {
            string BalanceCheckBody = String.Empty;
            try
            {
                BalanceCheckBody = configuration.GetSection("AppSettings:BalanceCheckBody").Value;
                BalanceCheckBody = AESCryptography.Decrypt(BalanceCheckBody);
            }
            catch (Exception)
            {
                throw;
            }
            return BalanceCheckBody;
        }
        public static string GetFTRRequestType()
        {
            string FTRRequestType = string.Empty;
            try
            {
                FTRRequestType = configuration.GetSection("AppSettings:FTRRequestType").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return FTRRequestType;
        }

        public static string GetFTRRequestChannel()
        {
            string FTRRequestChannel = string.Empty;
            try
            {
                FTRRequestChannel = configuration.GetSection("AppSettings:FTRRequestChannel").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return FTRRequestChannel;
        }

        public static int GetFTRRequestID()
        {
            int FTRRequestID = 0;
            try
            {
                FTRRequestID = Convert.ToInt32(configuration.GetSection("AppSettings:FTRRequestID").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return FTRRequestID;
        }
        public static int GetIsRetailerAPICore()
        {
            int isRetailerApiCore = 0;
            try
            {
                isRetailerApiCore = Convert.ToInt32(configuration.GetSection("AppSettings:IsRetailerAppCore").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return isRetailerApiCore;
        }

        public static string GetAirUrl()
        { 
            string AirUrl = string.Empty; 
            try
            {
                AirUrl = configuration.GetSection("AppSettings:AirBaseUrl").Value;
                AirUrl = AESCryptography.Decrypt(AirUrl);
            }
            catch (Exception)
            {
                throw;
            }
            return AirUrl;
        }

        public static string GetAirUserName()
        {
            string AirUserName = string.Empty;
            try
            {
                AirUserName = configuration.GetSection("AppSettings:AirUserName").Value;
                AirUserName = AESCryptography.Decrypt(AirUserName);
            }
            catch (Exception)
            {
                throw;
            }
            return AirUserName;
        }
        public static string GetAirCred()
        {
            string AirCred = string.Empty; 
            try
            {
                AirCred = configuration.GetSection("AppSettings:AirCred").Value;
                AirCred = AESCryptography.Decrypt(AirCred);
            }
            catch (Exception)
            {
                throw;
            }
            return AirCred;
        }
        public static string GetoriginNodeType()
        {
            string originNodeType = string.Empty;
            try
            {
                originNodeType = configuration.GetSection("AppSettings:originNodeType").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return originNodeType;
        }
        public static int GetsubscriberNumberNAI()
        {
            int subscriberNumberNAI = 0;
            try
            {
                subscriberNumberNAI = Convert.ToInt32(configuration.GetSection("AppSettings:subscriberNumberNAI").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return subscriberNumberNAI;
        }
        public static long GetnegotiatedCapabilities()
        {
            long subscriberNumberNAI = 0;
            try
            {
                subscriberNumberNAI = Convert.ToInt64(configuration.GetSection("AppSettings:negotiatedCapabilities").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return subscriberNumberNAI;
        }
        public static int GetExpairyDate()
        {
            int FTRExpairyDate = 0;  
            try
            {
                FTRExpairyDate = Convert.ToInt32(configuration.GetSection("AppSettings:FTRExpairationMinute").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return FTRExpairyDate;
        }
        public static string GetAirAuthToken()
        {
            string AirAuthToken = string.Empty;
            try
            {
                AirAuthToken = configuration.GetSection("AppSettings:AirAuthToken").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return AirAuthToken;
        }

        public static string GetFPDefaultScore()
        {
            string fpScore = string.Empty;
            try
            {
                fpScore = configuration.GetSection("AppSettings:FPDefaultScore").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return fpScore;
        }
        public static int GetETSAFValidationValue()
        {
            int isESAFAllow = 0;
            try
            {
                isESAFAllow = Convert.ToInt32(configuration.GetSection("AppSettings:is_esafValidationNeed").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return isESAFAllow;
        }
         
        public static int GetRYZEstockAllow()
        {
            int isRyzeAllow = 0;
            try
            {
                isRyzeAllow = Convert.ToInt32(configuration.GetSection("AppSettings:is_ryze_stock_allow").Value);
            }
            catch (Exception)
            {
                throw;
            }
            return isRyzeAllow;
        }        

        public static string GetSessionMessage()
        {
            string session_message = string.Empty;
            try
            {
                session_message = configuration.GetSection("AppSettings:session_message").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return session_message;
        }

        public static string GetPaMessage()
        {
            string session_message = string.Empty;
            try
            {
                session_message = configuration.GetSection("AppSettings:pa_message").Value;
            }
            catch (Exception)
            {
                throw;
            }
            return session_message;
        }
    }
}
