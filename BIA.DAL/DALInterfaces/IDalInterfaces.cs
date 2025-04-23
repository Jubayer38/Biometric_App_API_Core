using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.ViewModel;
using System.Data;

namespace BIA.DAL.DALInterfaces
{
    public interface IDalInterfaces
    {
        bool UpdateBioDbForReservation(string bi_token_no, string msisdn_reservation_id);
        bool UpdateStatusandErrorMessage(string bi_token, int status, long error_id, string error_description);
        SingleSourceCheckResponseModel SingleSourceCheckFromBioDB(string msisdn, string sim_number, int purpose_No, string poc_number, int sim_rep_type, string dest_doc_id, string dest_dob, string dest_imsi);
        object IsStockAvailable(int stock_id, int channel_id);
        DataTable GetActivityLogData(int activity_type_id, string user_id);
        DataTable GetActivityLogDataV2(int activity_type_id, string user_id);
        DataTable GetPurposeNumbers(RAGetPurposeRequest model);
        long GetTokenNo(string mssisdn);
        DataTable VarificationFinishNotification(BIAFinishNotiRequest model);
        DataTable GetCustomErrorMsg(decimal errorId);
        DataTable GetDivision();
        DataTable GetDistrict();
        DataTable GetThana();
        DataTable GetDivDisThana();
        void RAToDBSSLog(VMBIAToDBSSLog model, string requestTxt, string responseTxt);
        DataTable ManageException(string message, int code, string errorSource);
        void BALogInsert(LogModel log);
        decimal SubmitOrder2(OrderRequest2 model);
        decimal SubmitOrderV3(OrderRequest3 model, string loginProviderId);
        decimal SubmitOrderV4(OrderRequest3 model, string loginProviderId);
        decimal SubmitOrder(OrderRequest model);
        DataTable GetStatus(StatusRequest model);
        long CheckBIAToken(string token_id);
        DataTable GetOrderInfoByTokenNo(decimal token_id);
        DataTable GetPortInOrderConfirmCode(int purposeId, string msisdn);
        DataTable ValidateOrder(VMValidateOrder model);
        DataTable ValidateOrder_(VMValidateOrder model);
        DataTable GetInventoryIdByChannelName(string channelName);
        DataTable GetCenterCodeByUserName(string userName);
        DataTable GetBssDataList(OrderListReqModel reqModel);
        bool UpdateBioDbForOrderReq(string bi_token_no, string order_conframtion_code);
        bool UpdateBioDbForCreateCustomerReq(string bi_token_no, string owner_customer_id);
        bool ClearBookingFlagForOrderReq(int order_booking_flag);
        DataTable GetBTSInformationByLacCid(int lac, int cid);
        DataTable GetSIMReplacementReasons();
        DataTable ValidateUser(vmUserInfo model);
        DataTable ValidateUser(string user_name);
        int GetUserAPIVersion(APIVersionRequest model);
        DataTable GetUserAPIVersionWithAppUpdateCheck(VMAPIVersionRequestWithAppUpdateCheck model);
        long SaveLoginAtmInfo(UserLogInAttempt model);
        long SaveLoginAtmInfoV2(UserLogInAttemptV2 model);
        int IsSecurityTokenValid(string loginProvider);
        int IsAESEligibleUser(string retailer);
        int ChangePassword(VMChangePassword model);
        int ChangePasswordV2(VMChangePassword model);
        DataTable GetPasswordLength();
        DataTable GetPasswordLengthV2();
        DataTable GetUserMobileNoAndOTP(string userName);
        DataTable GetUserMobileNoAndOTPV2(string userName);
        int FORGETPWD(VMForgetPWD model);
        int FORGETPWDV2(VMForgetPWD model);
        DataTable IsUserCurrentlyLoggedIn(decimal userId);
        int IsSecurityTokenValid2(string loginProvider, string deviceId);
        long IsSecurityTokenValidV3(string loginProvider, string deviceId);
        long IsSecurityTokenValidForBPLogin(string loginProvider, string deviceId);
        DataTable GetChangePasswordGlobalSettingsData();
        DataTable GetChangePasswordGlobalSettingsDataV2();
        DataTable ValidateDbssUser(vmUserInfo model);
        DataTable ValidateBPUser(string bp_msisdn, string user_name);
        int GenerateBPLoginOTP(string loginProvider);
        DataTable ValidateBPOtp(decimal bp_otp, decimal retailer_otp, string sessionToken);
        int ResendBPOTP(string loginProviderId);
        long Logout(string loginProvider);
        DataTable GetUnpairedMSISDNSearchDefaultValue(UnpairedMSISDNListReqModel model);
        DataTable GetStockAvailable(string channel_Name);
        decimal UpdateOrder(OrderRequest3 model);
        decimal SubmitOrderV5(OrderRequest3 model, string loginProviderId);


    }
}
