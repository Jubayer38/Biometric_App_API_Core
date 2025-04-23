using BIA.Entity.Connectivities;
using BIA.Entity.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Collections
{
    public static class SettingsValues
    {
        public static string conn { get; private set; }
        public static string connwithparam { get; private set; }
        public static string dbssBaseUsrl { get; private set; }
        public static string dbssBaseUsrlWithParam { get; private set; } 
        public static string jwtSequrityKey { get; private set; } 
        public static string dmsBaseUrl { get; private set; } 
        public static string dmsUserName { get; private set; } 
        public static string dmsPassword { get; private set; }  
        public static string retailBaseUrl { get; private set; } 
        public static string rsoBaseUrl { get; private set; } 
        public static string SingleSourceUrl { get; private set; } 
        public static string SingleSourceUserName { get; private set; } 
        public static string SingleSourcePassword { get; private set; } 
        public static string userUpdateUserName { get; private set; } 
        public static string userUpdatePassword { get; private set; } 
        public static string singleSourceCheckingMessage { get; private set; } 
        public static string rsoAppUserName { get; private set; } 
        public static int is_bts_allow { get; private set; }
        public static string eShopBaseUrl { get; set; }
        public static string eShopCredential { get; set; }
        public static string EV_API_URL { get; private set; }
        public static string EV_API_QueryString { get; private set; }
        public static string EV_API_RequestBody { get; private set; }
        public static string FTRReqType { get; private set; }
        public static string FTRRequestChannel { get; private set; }
        public static int FTRRequestID { get; private set; }
        public static int is_retailer_ApiCorre { get; private set; }
        public static string AirBaseUrl { get; private set; }
        public static string AirUserName { get; private set; }
        public static string AirPassword { get; private set; }
        public static string originNodeType { get; private set; }
        public static int subscriberNumberNAI { get; private set; }
        public static long negotiatedCapabilities { get; private set; }
        public static int FTRExpairyDate { get; private set; }
        public static string AirAuthToken { get; private set; }
        public static string fpScore { get; private set; }
        public static int is_esafValidationNeed { get; private set; } 
        public static int is_ryze_allow { get; private set; }
          
        public static string session_message { get; private set; }
        public static string paMessage { get; set; }

        public static string GetConnectionString()  
        {   
            if(String.IsNullOrEmpty(conn))  
            {
                conn = ConnectivitiesValues.GetConnectionString();
            }
            return conn;             
        }
        public static string GetConnectionWithName(string name)
        {
            if(String.IsNullOrEmpty(connwithparam))
            {
                connwithparam = ConnectivitiesValues.GetConnectionWithNameParam(name);
            }
            return connwithparam;
        }

        public static string GetDbssBaseUrl()
        {
            if (String.IsNullOrEmpty(dbssBaseUsrl))
            {
                dbssBaseUsrl = ConnectivitiesValues.GetDBSSBaseUrl();
            } 
            return dbssBaseUsrl; 
        }

        public static string GetDbssBaseUrlWithParam(string name)
        { 
            if (String.IsNullOrEmpty(dbssBaseUsrlWithParam))
            {
                dbssBaseUsrlWithParam = ConnectivitiesValues.GetDBSSBaseUrlWithaParam(name);
            }
            return dbssBaseUsrlWithParam;
        }
        public static string GetJWTSequrityKey() 
        {
            if (String.IsNullOrEmpty(jwtSequrityKey))
            {
                jwtSequrityKey = ConnectivitiesValues.GetJWTSecurityKey();
            }
            return jwtSequrityKey;
        }
        public static string GetDMSBaseUrl()
        {
            if (String.IsNullOrEmpty(dmsBaseUrl))
            {
                dmsBaseUrl = ConnectivitiesValues.GetDMSBaseUrl();
            } 
            return dmsBaseUrl;
        }
        public static string GetDMSUserName()
        {
            if (String.IsNullOrEmpty(dmsUserName))
            {
                dmsUserName = ConnectivitiesValues.GetDMSUserName();
            }
            return dmsUserName; 
        }
        public static string GetDMSPassword()
        {
            if (String.IsNullOrEmpty(dmsPassword))
            {
                dmsPassword = ConnectivitiesValues.GetDMSPassword();
            }
            return dmsPassword;
        }
         
        public static string GetRetailerAppBaseUrl()
        {
            if (String.IsNullOrEmpty(retailBaseUrl))
            {
                retailBaseUrl = ConnectivitiesValues.GetRetailerAppBaseUrl();
            }
            return retailBaseUrl;
        }
        public static string GetRSOBaseUrl()
        { 
            if (String.IsNullOrEmpty(rsoBaseUrl))
            {
                rsoBaseUrl = ConnectivitiesValues.GetRSOBaseUrl();
            }
            return rsoBaseUrl;
        } 

        public static string GetSingleSourceUrl()
        {
            if (String.IsNullOrEmpty(SingleSourceUrl))
            {
                SingleSourceUrl = ConnectivitiesValues.GetSingleSourceBaseUrl();
            }
            return SingleSourceUrl;
        }

        public static string GetSingleSourceUserName()
        {
            if (String.IsNullOrEmpty(SingleSourceUserName))
            {
                SingleSourceUserName = ConnectivitiesValues.GetSingleSourceUserName();
            }
            return SingleSourceUserName;
        } 
        public static string GetSingleSourcePassword()
        {
            if (String.IsNullOrEmpty(SingleSourcePassword))
            {
                SingleSourcePassword = ConnectivitiesValues.GetSingleSourcePassword();
            }
            return SingleSourcePassword;
        }
        public static string GetUserStatusUpdateUserName() 
        {
            if (String.IsNullOrEmpty(userUpdateUserName))
            {
                userUpdateUserName = ConnectivitiesValues.RetailerUpdateStatusUserName();
            }
            return userUpdateUserName;
        }
        public static string GetUserStatusUpdatePassword()
        {
            if (String.IsNullOrEmpty(userUpdatePassword)) 
            {
                userUpdatePassword = ConnectivitiesValues.RetailerUpdateStatusPassword();
            }
            return userUpdatePassword;
        }
        public static string GetSingleSourceMessage()
        {
            if (String.IsNullOrEmpty(singleSourceCheckingMessage))
            {
                singleSourceCheckingMessage = ConnectivitiesValues.SingleSourceActiveMessage();
            }
            return singleSourceCheckingMessage;
        }
        public static string GetRSOAppUserName()
        {
            if (String.IsNullOrEmpty(rsoAppUserName))
            {
                rsoAppUserName = ConnectivitiesValues.GetRSOUserNameForRaiseComplaint();
            }
            return rsoAppUserName;
        }

        public static int GetBTSCodeShowingOrNot()
        {
            is_bts_allow = ConnectivitiesValues.GetBTSCodeShowingOrNot();
            return is_bts_allow;
        }

        public static string GeteShopBaseUrl()
        { 
            if (String.IsNullOrEmpty(eShopBaseUrl))
            {
                eShopBaseUrl = ConnectivitiesValues.GeteShopBaseURL();
            }
            return eShopBaseUrl;
        }
         
        public static string GeteShopCredential()
        {
            if (String.IsNullOrEmpty(eShopCredential))
            {
                eShopCredential = ConnectivitiesValues.GeteShopCredential();
            }
            return eShopCredential;
        }

        public static string GetEV_API_URL()
        {
            if (String.IsNullOrEmpty(EV_API_URL))
            {
                EV_API_URL = ConnectivitiesValues.GetEVAPIUrl();
            }
            return EV_API_URL;
        }

        public static string GetEV_API_QueryString()
        {
            if (String.IsNullOrEmpty(EV_API_QueryString))
            {
                EV_API_QueryString = ConnectivitiesValues.GetEVAPIQueryString();
            }
            return EV_API_QueryString;
        }
        public static string GetEV_API_RequestBody()
        {
            if (String.IsNullOrEmpty(EV_API_RequestBody))
            {
                EV_API_RequestBody = ConnectivitiesValues.GetEVAPIReqBody();
            }
            return EV_API_RequestBody;
        }

        public static string GetFTRRequestType()
        {
            if (String.IsNullOrEmpty(FTRReqType))
            {
                FTRReqType = ConnectivitiesValues.GetFTRRequestType();
            }
            return FTRReqType;
        }
        public static string GetFTRRequestChannel()
        {
            if (String.IsNullOrEmpty(FTRRequestChannel))
            {
                FTRRequestChannel = ConnectivitiesValues.GetFTRRequestChannel();
            }
            return FTRRequestChannel;
        }
        public static int GetFTRRequestID()
        {
            FTRRequestID = ConnectivitiesValues.GetFTRRequestID();
            return FTRRequestID;
        }
        public static int GetIsRetailerAPICore()
        {
            is_retailer_ApiCorre = ConnectivitiesValues.GetIsRetailerAPICore();
            return is_retailer_ApiCorre;
        }
        public static string GetAirBaseUrl()
        {
            if (String.IsNullOrEmpty(AirBaseUrl))
            {
                AirBaseUrl = ConnectivitiesValues.GetAirUrl();
            }
            return AirBaseUrl;
        }
        public static string GetAirUserName()
        { 
            if (String.IsNullOrEmpty(AirUserName))
            {
                AirUserName = ConnectivitiesValues.GetAirUserName();
            }
            return AirUserName;
        }
        public static string GetAirPassword()
        {
            if (String.IsNullOrEmpty(AirPassword))
            {
                AirPassword = ConnectivitiesValues.GetAirCred();
            }
            return AirPassword;
        }
        public static string GetoriginNodeType()
        {
            if (String.IsNullOrEmpty(originNodeType))
            {
                originNodeType = ConnectivitiesValues.GetoriginNodeType();
            }
            return originNodeType;
        }
        public static int GetsubscriberNumberNAI()
        {
            subscriberNumberNAI = ConnectivitiesValues.GetsubscriberNumberNAI();
            return subscriberNumberNAI;
        }
        public static long GetnegotiatedCapabilities()
        {
            negotiatedCapabilities = ConnectivitiesValues.GetnegotiatedCapabilities();
            return negotiatedCapabilities;
        }
        public static int GetFTRExpairyDate()
        {
            FTRExpairyDate = ConnectivitiesValues.GetExpairyDate();
            return FTRExpairyDate;
        }
        public static string GetAirAuthToken()
        {
            if (String.IsNullOrEmpty(AirAuthToken))
            {
                AirAuthToken = ConnectivitiesValues.GetAirAuthToken();
            }
            return AirAuthToken;
        }
        public static string GetFPDefaultScore()
        {
            if (String.IsNullOrEmpty(fpScore))
            {
                fpScore = ConnectivitiesValues.GetFPDefaultScore();
            }
            return fpScore;
        }
         
        public static int GetETSAFValidationValue()
        {
            is_esafValidationNeed = ConnectivitiesValues.GetETSAFValidationValue();
            return is_esafValidationNeed;
        }

        public static int GetRyzeAllowOrNot()
        { 
            is_ryze_allow = ConnectivitiesValues.GetRYZEstockAllow();
            return is_ryze_allow;
        }
       
        public static string GetSessionMessage()
        {
            if (String.IsNullOrEmpty(session_message))
            {
                session_message = ConnectivitiesValues.GetSessionMessage();
            }
            return session_message;
        }

        public static string GetPaMessage()
        {
            if (String.IsNullOrEmpty(paMessage))
            {
                paMessage = ConnectivitiesValues.GetPaMessage();
            }
            return paMessage;
        }
    }
} 
