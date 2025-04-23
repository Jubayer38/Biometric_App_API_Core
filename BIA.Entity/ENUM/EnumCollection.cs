using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ENUM
{
    internal class EnumCollection
    {
    }

    public enum IntegrationPoints : int
    {
        RA = 1,
        BI = 2,
        BSS = 3,
        BA = 4,
        DMS = 5

    }

    public enum APIVersoinEnum : int
    {
        None = 0,
        Old = 1,
        New = 2
    }

    public enum ChangePasswordEnum : int
    {
        invalidUser = -888,
        passwordNotMatched = -777,
        unableToUpdate = -999
    }


    public enum EnumPurposeNumber
    {
        ReRegistration = 1,
        NewRegistration = 2,
        DeRegistration = 3,
        MNPDeRegistration = 4,
        SIMTransfer = 5,
        SIMReplacement = 6,
        MNPRegistration = 7,
        BulkDeRegistration = 8,
        SIMCategoryMigration = 9,
        CorporateToIndividualTransfer = 10,
        IndividualToCorporateTransfer = 11,
        DeathCaseTransfer = 12,
        DualClaimTransfer = 13,
        SpecialRegistration = 14,
        OtherToNIDOrSmartCardIDTransfer = 15,
        MNPEmergencyReturn = 16,
        mnp_port_in_cancel = 1001,
        ec_validation = 1000

    }

    public enum StatusNo
    {
        bio_verification = 10,//Request Submitted
        bio_req_sending = 20,//Bio Veri Submitted
        order_request = 30,//Bio Veri Success
        order_request_fail = 35,//Bio Requset Fail or Get Status "Status"="error/failed"
        order_req_sending = 40,//Activation Req Submitted
        order_req_success = 50,//Activation successs
        get_subscription_failed = 51,
        user_customer_upd_failed = 52,
        payer_customer_upd_failed = 53,
        customer_info_upd_success = 60,
        qc_unverify_failed = 61,
        qc_verify_not_found = 62,
        qc_unverify_success = 65,
        order_req_failed = 150,//Failed
    }

    public enum DOCTypeNo : int
    {
        nid = 1,
        passport = 2
    }


    public enum DOCTypeIDNoNIDLengthWise : int
    {
        NID_Length_10 = 5,
        NID_Length_Others = 1
    }


    public enum NIDLength : int
    {
        Length_10 = 10,
        Length_13 = 13,
        Length_17 = 17
    }



    public enum EnumSIMReplacementType : int
    {
        ByPOC = 1,
        ByAuthPerson = 2,
        BulkSIMReplacment = 3
    }


    public enum EnumSimCategory : int
    {
        Prepaid = 1,
        Postpaid = 2
    }


    public enum EnumRAOrderStatus : int
    {
        RequestSubmitted = 10,
        BioVerificationSubmitted = 20,
        BioVerificationSuccess = 30,
        OrderFailed = 35,
        ActivationRequestSubmitted = 40,
        ActivationSuccessful = 50,
        Failed = 150
    }


    public enum EnumForgetPWDStatus
    {
        Success = 1,
        SMSSendFailed = -777,
        UpdateFailed = -888,
        UserInfoNotFound = -1111
    }


    public enum EnumSecurityTokenPropertyIndex
    {
        LoginProvider = 0,
        uid = 1,
        unamen = 2,
        dc = 3,//DistributorCode
        deviceId = 4
    }


    public enum EnumInventoryId
    {
        POS = 1,
        DMS = 2,
    }

    public enum EnumPurposeForDBSSOTP
    {
        SIMReplByAuth = 2,
        TOSTwoPartyB2BtoB2C = 3
    }


    public enum EnumPurposeOfSIMCheck
    {
        NewConnection = 1,
        SIMReplacement = 2
    }

    public enum EnumOrderErrorStatus
    {
        OnProcess = -888,
        Failed = -999
    }

    public enum EnumValidateOrder
    {
        ValidOrder = 1,
        MsisdnOnProcess = -111,
        SIMOnProcess = -222,
        OnProcess = -333,
        DBFailed = -999
    }
    public enum IntegrationPoint
    {
        bss = 3,
        bss_service = 10
    }
}
