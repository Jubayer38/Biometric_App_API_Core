using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.ENUM;
using BIA.Entity.ResponseEntity;
using BIA.Entity.ViewModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Data;
using static BIA.Entity.ResponseEntity.CherishedMSISDNCheckResponse;

namespace BIA.BLL.BLLServices
{
    public class BLLDBSSToRAParse
    {
        private readonly DALBiometricRepo _dataManager;
        private readonly BLLCommon _bllCommon;
        private readonly BLLDivDisThana _bLLDivDisThana;

        public BLLDBSSToRAParse(DALBiometricRepo dataManager, BLLCommon bllCommon, BLLDivDisThana bLLDivDisThana)
        {
            _dataManager = dataManager;
            _bllCommon = bllCommon;
            _bLLDivDisThana = bLLDivDisThana;
        }
        public RACommonResponse QCUpdateRespParsing(QCStatusResponseRootobject data)
        {
            int flag = 0;
            RACommonResponse response = new RACommonResponse();
            try
            {
                for (int i = 0; i < data.data.Count; i++)
                {
                    if (data.data[i].attributes != null)
                    {
                        if (data.data[i].attributes.status == "requested"
                            || data.data[i].attributes.status == "scheduled"
                            || data.data[i].attributes.status == "done"
                            || data.data[i].attributes.status == "new")
                        {
                            flag = 1;
                        }
                        else
                        {
                            flag = 0;
                            break;
                        }
                    }
                }

                if (flag == 1)
                {
                    response.result = true;
                    response.message = "Data updated successfully";
                }
                else
                {
                    response.result = false;
                    response.message = "Data update failed";
                }

            }
            catch (Exception ex)
            {
                response.result = false;
                response.message = ex.Message;
                throw ex;
            }
            return response;
        }


        public RACommonResponse CustomerUpdateRespParsing(CustomerUpdateRespRootobject data)
        {
            int flag = 0;
            RACommonResponse response = new RACommonResponse();
            try
            {
                for (int i = 0; i < data.data.Count; i++)
                {
                    if (data.data[i].attributes != null)
                    {
                        if (data.data[i].attributes.status == "requested"
                            || data.data[i].attributes.status == "scheduled"
                            || data.data[i].attributes.status == "done"
                            || data.data[i].attributes.status == "new")
                        {
                            flag = 1;
                        }
                        else
                        {
                            flag = 0;
                            break;
                        }
                    }
                }

                if (flag == 1)
                {
                    response.result = true;
                    response.message = "Data updated successfully";
                }
                else
                {
                    response.result = false;
                    response.message = "Data update failed";
                }

            }
            catch (Exception ex)
            {
                response.result = false;
                response.message = ex.Message;
                throw ex;
            }
            return response;
        }



        public async Task<VMRejectedOrder> RejectionOrdersParsing(string qualityControlId, string customerId, RejectedOrdersAttributes rAattrib
                                                    , CustomerInfoResponseAttributes cIattrib
                                                    , CustomerAddressResponseAttributes cAattrib)
        {
            VMRejectedOrder ro = new VMRejectedOrder();

            try
            {
                ro.quality_control_id = qualityControlId;
                ro.customer_id = customerId;/*bLLCommon.GetTokenNo(rAattrib.msisdn);*///Need to know the business logic for selecting data from DB.
                ro.customer_name = cIattrib.firstname;
                ro.alt_msisdn = cIattrib.altcontactphone;

                var divisions = await Task.Run(() => _bLLDivDisThana.GetDivision());
                ro.division_id = divisions
                    .Where(a => string.Equals(a.DIVISIONNAME, cAattrib.postaldistrict, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.DIVISIONID)
                    .FirstOrDefault();
                ro.division_name = cAattrib.postaldistrict;

                var districts = await Task.Run(() => _bLLDivDisThana.GetDistrict());
                ro.district_id = districts
                    .Where(a => string.Equals(a.DISTRICTNAME, cAattrib.city, StringComparison.OrdinalIgnoreCase) && a.DIVISIONID == ro.division_id)
                    .Select(a => a.DISTRICTID)
                    .FirstOrDefault();
                ro.district_name = cAattrib.city;

                var thanas = await Task.Run(() => _bLLDivDisThana.GetThana());
                ro.thana_id = thanas
                    .Where(a => string.Equals(a.THANANAME, cAattrib.province, StringComparison.OrdinalIgnoreCase)
                        && a.DISTRICTID == ro.district_id)
                    .Select(a => a.THANAID)
                    .FirstOrDefault();
                ro.thana_name = cAattrib.province;

                ro.email = cIattrib.email;
                ro.flat_number = cAattrib.street;
                ro.gender = cIattrib.gender;
                ro.house_number = cAattrib.building;
                ro.is_over_due = 0;//This must be configarable from webConfig.
                ro.mobile_number = rAattrib.msisdn;
                ro.postal_code = cAattrib.postalcode;
                //We get GMT time from DBSS, thus we need to add 6 hours with the GMT time to show the BD time to end user.
                ro.rejection_date = rAattrib.lastmodified.AddHours(6).ToString();
                ro.reject_reason = rAattrib.reason;
                ro.road_number = cAattrib.road;
                ro.village = cAattrib.area;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ro;
        }


        public RACommonResponse SIMValidationParsing(JObject dbssResp)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null)
                {
                    response.result = false;
                    response.message = "Data filed empty.";
                }
                var status = dbssResp?["data"]?["status"]?.ToString();
                response.result = status == "success" ? true : false;
                response.message = "SIM is valid.";

                return response;
            }
            catch (Exception ex)
            {
                //response.result = false;
                //response.message = ex.Message;
                throw ex;
            }
        }


        public RACommonResponse SIMValidationParsing2(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }

                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }

                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Prepaid
                    && isPired == true)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower()/*"paired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PairedMSISDN.ToLower() /*"paired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPrepaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Postpaid
                    && isPired == true)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower()/*"paired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower() /*"postpaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PairedMSISDN.ToLower()/*"paired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPairedSIM;
                        return response;
                    }
                    else if (dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePostpaid.ToLower() /*"postpaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPostpaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Prepaid
                    && isPired == false)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAnUnpairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePrepaid.ToLower()/*"prepaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPrepaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Postpaid
                    && isPired == false)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower()/*"postpaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAnUnpairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePostpaid.ToLower()/*"postpaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPostpaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }
                //----------------SIMReplacement--------------
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.SIMReplacement
                    && !String.IsNullOrEmpty(oldSimType))
                {
                    if (oldSimType.ToLower() == FixedValueCollection.SIMTypeUSIM /*"usim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower() /*"sim_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotASwapSIM;
                            return response;
                        }
                    }
                    else if (oldSimType.ToLower() == FixedValueCollection.SIMTypeSIM/*"sim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeEV_SWAP.ToLower() /*"ev_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotAEVSwapSIM;
                            return response;
                        }
                    }
                    //============New SIM Type "PLI" Added=======
                    else if (oldSimType.ToLower() == FixedValueCollection.SIMTypePLI/*"pli"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower() /*"ev_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotASwapSIM;
                            return response;
                        }
                    }
                    //==========x============
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMTypeIsNotSIMOrUSIM;
                        return response;
                    }
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.InvalidAttempt + " while checking SIM!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RACommonResponse SIMValidationParsing3(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*e-sim*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*e_sim_swap*/)
                {
                    {
                        response.result = false;
                        response.message = "This is not Physical SIM.";
                        return response;
                    }
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower() /*ryz-prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryz-esim*/)
                {
                    {
                        response.result = false;
                        response.message = "Incorrect Product!";
                        return response;
                    }
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }               
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Prepaid
                    && isPired == true)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower()/*"paired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PairedMSISDN.ToLower() /*"paired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPrepaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Postpaid
                    && isPired == true)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower()/*"paired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower() /*"postpaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PairedMSISDN.ToLower()/*"paired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePostpaid.ToLower() /*"postpaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPostpaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Prepaid
                    && isPired == false)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower() /*"prepaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAnUnpairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePrepaid.ToLower()/*"prepaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPrepaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }

                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection
                    && simCategory == (int)EnumSimCategory.Postpaid
                    && isPired == false)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/
                        && dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower()/*"postpaid"*/)
                    {
                        response.result = true;
                        response.message = MessageCollection.SIMValid;
                        return response;
                    }
                    else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.UnairedMSISDN.ToLower()/*"unpaired"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAnUnpairedSIM;
                        return response;
                    }
                    else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() != FixedValueCollection.PaymentTypePostpaid.ToLower()/*"postpaid"*/)
                    {
                        response.result = false;
                        response.message = MessageCollection.NotAPostpaidSIM;
                        return response;
                    }
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMInvalid;
                        return response;
                    }
                }
                //----------------SIMReplacement--------------
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.SIMReplacement
                    && !String.IsNullOrEmpty(oldSimType))
                {
                    if (oldSimType.ToLower() == FixedValueCollection.SIMTypeUSIM /*"usim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower() /*"sim_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotASwapSIM;
                            return response;
                        }
                    }
                    else if (oldSimType.ToLower() == FixedValueCollection.SIMTypeSIM/*"sim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeEV_SWAP.ToLower() /*"ev_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotAEVSwapSIM;
                            return response;
                        }
                    }
                    //============New SIM Type "PLI" Added=======
                    else if (oldSimType.ToLower() == FixedValueCollection.SIMTypePLI/*"pli"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower() /*"ev_swap"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.NotASwapSIM;
                            return response;
                        }
                    }
                    //==========x============
                    else
                    {
                        response.result = false;
                        response.message = MessageCollection.SIMTypeIsNotSIMOrUSIM;
                        return response;
                    }
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.InvalidAttempt + " while checking SIM!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RACommonResponse SIMValidationParsing4(JObject dbssResp, int purposeOfSIMCheck, int? simCategory, bool? isPired, string oldSimType)
        {
            RACommonResponse response = new RACommonResponse();
            try
            {
                if (dbssResp?["data"]?["status"] == null
                    && dbssResp?["data"]?["logical_inventory_status"] == null
                    && dbssResp?["data"]?["physical_inventory_status"] == null
                    && String.IsNullOrEmpty(dbssResp?["data"]?["status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["logical_inventory_status"]?.ToString())
                    && String.IsNullOrEmpty(dbssResp?["data"]?["physical_inventory_status"]?.ToString()))
                {
                    response.result = false;
                    response.message = MessageCollection.DataNotFound;
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaid.ToLower()/*prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePostpaid.ToLower() /*postpaid*/)
                {
                    response.result = false;
                    response.message = "This is not eSIM";
                    return response;
                }
                else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypePrepaidStarTrek.ToLower()/*ryz-prepaid*/
                    || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESimStarTrek.ToLower() /*ryz-esim*/)
                {
                    response.result = false;
                    response.message = "Incorrect Product!";
                    return response;
                }
                else if (dbssResp?["data"]?["status"]?.ToString().ToLower() == "failed")
                {
                    response.result = false;

                    response.message = MessageCollection.SIMIsNotInInventory;
                    if (dbssResp != null && dbssResp.ContainsKey("data") && dbssResp["data"] != null && (dbssResp["data"] is JObject dataObj && dataObj.ContainsKey("error_message")))
                    {
                        var errorMessage = dbssResp["data"]["error_message"]?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            response.message = errorMessage;
                        }
                    }

                    return response;
                }
                else if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == "used")
                {
                    response.result = false;
                    response.message = MessageCollection.SIMIsUsed;
                    return response;
                }
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection && isPired == false)
                {
                    if (dbssResp?["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/
                        && (dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*"eSim"*/
                        || dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*"e_sim_swap"*/))
                    {
                        if (dbssResp["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/
                         && dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*"eSim"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else if (dbssResp["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.UnairedMSISDN.ToLower() /*"unpaired"*/
                        && dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*"e_sim_swap"*/)
                        {
                            response.result = false;
                            response.message = MessageCollection.SIM_Not_Match;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.SIMInvalid;
                            return response;
                        }
                    }
                    response.result = false;
                    response.message = "This is not eSIM.";
                    return response;
                }
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.NewConnection && isPired == true)
                {
                    if (dbssResp["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower() /*"paired"*/
                        && (dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*"e-sim"*/
                        || dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*"e_sim_swap"*/))
                    {
                        if (dbssResp["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower() /*"paired"*/
                         && dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower() /*"e-sim"*/)
                        {
                            response.result = true;
                            response.message = MessageCollection.SIMValid;
                            return response;
                        }
                        else if (dbssResp["data"]?["logical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PairedMSISDN.ToLower() /*"paired"*/
                        && dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*"e_sim_swap"*/)
                        {
                            response.result = false;
                            response.message = MessageCollection.SIM_Not_Match;
                            return response;
                        }
                        else
                        {
                            response.result = false;
                            response.message = MessageCollection.SIMInvalid;
                            return response;
                        }
                    }
                    response.result = false;
                    response.message = "This is not eSIM.";
                    return response;
                }
                //----------------SIMReplacement--------------
                else if (purposeOfSIMCheck == (int)EnumPurposeOfSIMCheck.SIMReplacement && !String.IsNullOrEmpty(oldSimType))
                {
                    if (oldSimType.ToLower() == FixedValueCollection.SIMTypeUSIM /*"usim"*/ || oldSimType.ToLower() == FixedValueCollection.SIMTypeSIM/*"sim"*/)
                    {
                        if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower() /*"e_sim_swap"*/
                            || dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower()/*e-sim*/)
                        {
                            if (dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeE_SIM_SWAP.ToLower())
                            {
                                response.result = true;
                                response.message = MessageCollection.SIMValid;
                                return response;
                            }
                            else if (dbssResp["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PaymentTypeESim.ToLower())
                            {
                                response.result = false;
                                response.message = MessageCollection.SIM_Not_Match;
                                return response;
                            }
                            else
                            {
                                response.result = false;
                                response.message = MessageCollection.SIMInvalid;
                                return response;
                            }
                        }
                        else if (dbssResp?["data"]?["physical_inventory_status"]?.ToString().ToLower() == FixedValueCollection.PhycalInventorySIMTypeSIM_SWAP.ToLower())
                        {
                            response.result = false;
                            response.message = "This is not eSIM.";
                            return response;
                        }
                    }
                    response.result = false;
                    response.message = "Old SIM type should be USIM.";
                    return response;
                }
                else
                {
                    response.result = false;
                    response.message = MessageCollection.InvalidAttempt + " while checking SIM!";
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public VMBioCancelMSISDNValidationReqParsing BioCancelMSISDNValidationReqParsing(JObject dbssRespObj)
        {
            VMBioCancelMSISDNValidationReqParsing raResp = new VMBioCancelMSISDNValidationReqParsing();
            try
            {
                if (dbssRespObj?["data"]?.Count() > 0 && dbssRespObj?["included"]?.Count() > 1)
                {
                    var dataObj = (JObject)dbssRespObj["data"].FirstOrDefault();
                    var includedObj = (JObject)dbssRespObj["included"].FirstOrDefault();
                    if (dataObj != null && dataObj.ContainsKey("attributes") && includedObj != null && includedObj.ContainsKey("attributes"))
                    {
                        if (String.IsNullOrEmpty((string)dbssRespObj?["data"]?[0]?["id"]))
                        {
                            raResp.result = false;
                            raResp.message = "Subscription ID field empty!";
                            return raResp;
                        }

                        if (String.IsNullOrEmpty((string)dbssRespObj?["included"]?[0]?["attributes"]?["id-document-type"]))
                        {
                            raResp.result = false;
                            raResp.message = MessageCollection.DataNotFound;
                            return raResp;
                        }

                        if ((string)dbssRespObj?["included"]?[0]?["attributes"]?["id-document-type"] != "national_id"
                            && (string)dbssRespObj?["included"]?[0]?["attributes"]?["id-document-type"] != "smart_national_id")
                        {
                            raResp.result = false;
                            raResp.message = "Customer is not registered with National ID!";
                            return raResp;
                        }

                        if (String.IsNullOrEmpty((string)dbssRespObj?["data"]?[0]?["attributes"]?["payment-type"]))
                        {
                            raResp.result = false;
                            raResp.message = "payment-type field empty!";
                            return raResp;
                        }
                        
                        var dataAttrbObj = (JObject)dataObj.Property("attributes")!.Value;

                        var includeAttrbObj = (JObject)includedObj.Property("attributes")!.Value;

                        if (dataAttrbObj.ContainsKey("status") && dataAttrbObj.Property("status")!.HasValues)
                        {
                            var datasStatus = (string)dataAttrbObj.Property("status")!.Value;
                            //if (datasStatus == "active")
                            //{
                            //    if (includeAttrbObj != null && includeAttrbObj.ContainsKey("date-of-birth")
                            //        && includeAttrbObj.ContainsKey("id-document-number"))
                            //    {
                            //        raResp.dob = (string)includeAttrbObj.Property("date-of-birth");
                            //        raResp.nid = (string)includeAttrbObj.Property("id-document-number");
                            //        raResp.result = true;
                            //        raResp.message = MessageCollection.Success;
                            //        raResp.subscription_id = (long)dbssRespObj?["data"]?[0]?["id"] != 0 ? (long)dbssRespObj?["data"]?[0]?["id"]:0;
                            //        raResp.dest_sim_category = Convert.ToString(dbssRespObj?["data"]?[0]?["attributes"]?["payment-type"]) == "prepaid" ? (int)EnumSimCategory.Prepaid : (int)EnumSimCategory.Postpaid;
                            //    }
                            //}
                            if (datasStatus == "active")
                            {
                                if (includeAttrbObj != null &&
                                    includeAttrbObj.TryGetValue("date-of-birth", out var dobValue) &&
                                    includeAttrbObj.TryGetValue("id-document-number", out var nidValue))
                                {
                                    raResp.dob = dobValue.ToString();
                                    raResp.nid = nidValue.ToString();
                                    raResp.result = true;
                                    raResp.message = MessageCollection.Success;

                                    var dataObjv2 = dbssRespObj?["data"]?[0];
                                    raResp.subscription_id = dataObjv2?["id"] != null && long.TryParse(dataObjv2["id"]?.ToString(), out var subscriptionId)
                                                              ? subscriptionId
                                                              : 0;

                                    string paymentType = Convert.ToString(dataObj?["attributes"]?["payment-type"]);
                                    raResp.dest_sim_category = paymentType == "prepaid"
                                                               ? (int)EnumSimCategory.Prepaid
                                                               : (int)EnumSimCategory.Postpaid;
                                }
                            }

                            else
                            {
                                raResp.dob = null;
                                raResp.nid = null;
                                raResp.result = false;
                                raResp.message = "MSISDN is not active.";
                            }
                        }
                        else
                        {
                            raResp.dob = null;
                            raResp.nid = null;
                            raResp.result = false;
                            raResp.message = "Required field missing! (MSISDN contains no activity status.)";
                        }
                    }
                    else
                    {
                        raResp.dob = null;
                        raResp.nid = null;
                        raResp.result = false;
                        raResp.message = "Data not found.";
                    }
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private IndividualSIMReplacementMSISDNCheckResponse IndividualSIMReplacementMSISDNReqParsing(JObject dbssRespObj)
        {
            IndividualSIMReplacementMSISDNCheckResponse raResp = new IndividualSIMReplacementMSISDNCheckResponse();
            try
            {
                if (dbssRespObj["data"] == null
                    && dbssRespObj["data"].Count() <= 0)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj?["data"]?[0]?["attributes"]?["status"]))
                {
                    raResp.result = false;
                    raResp.message = "Msisdn status not found!";
                    return raResp;
                }

                if ((string)dbssRespObj?["data"]?[0]?["attributes"]?["status"] == "terminated")
                {
                    raResp.result = false;
                    raResp.message = "Msisdn is not valid for SIM replacemnt!";
                    return raResp;
                }

                if (dbssRespObj?["included"]?.Count() == 2
                    && dbssRespObj["included"]!.HasValues)
                {
                    if (dbssRespObj?["data"]?[0]?["id"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Subscription ID field empty!";
                        return raResp;
                    }
                    if (dbssRespObj?["data"]?[0]?["attributes"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Data field empty!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj?["included"]?[0]?["attributes"]?["icc"]))
                    {
                        raResp.result = false;
                        raResp.message = "Old SIM number not found!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj?["included"]?[0]?["attributes"]?["sim-type"]))
                    {
                        raResp.result = false;
                        raResp.message = "sim-type not found!";
                        return raResp;
                    }
                    if (!String.IsNullOrEmpty((string)dbssRespObj["data"]?[0]?["attributes"]?["status"])
                        && dbssRespObj?["included"]?[1]?["attributes"]?["is-company"] != null
                        && !String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["id-document-type"]))
                    {
                        string msdidnStatus = (string)dbssRespObj?["data"]?[0]?["attributes"]?["status"];
                        bool isCompany = dbssRespObj?["included"]?[1]?["attributes"]?["is-company"] != null ? Convert.ToBoolean(dbssRespObj?["included"]?[1]?["attributes"]?["is-company"]) : false;
                        string idDocumentType = (string)dbssRespObj?["included"]?[1]?["attributes"]?["id-document-type"];
                        //string oldSim = (string)dbssRespObj["included"][1]["attributes"]["icc"];
                        //string oldSimType = (string)dbssRespObj["included"][1]["attributes"]["sim-type"];

                        if (msdidnStatus != "active"
                            && msdidnStatus != "idle")
                        {
                            raResp.result = false;
                            raResp.message = "This MSISDN is not in active status.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else if (idDocumentType != "national_id" && idDocumentType != "smart_national_id")
                        {
                            raResp.result = false;
                            raResp.message = "Customer is not registered with National ID.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else if (isCompany == true)
                        {
                            raResp.result = false;
                            raResp.message = "This MSISDN is not eligible for individual SIM replacement.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else
                        {
                            string firstName = (string)dbssRespObj?["included"]?[1]?["attributes"]?["first-name"];
                            raResp.saf_status = String.IsNullOrEmpty(firstName) ? false : true;
                            string docIdNumber = (string)dbssRespObj?["included"]?[1]?["attributes"]?["id-document-number"];//Nid
                            string dob = (string)dbssRespObj?["included"]?[1]?["attributes"]?["date-of-birth"];
                            raResp.customer_id = (string)dbssRespObj?["included"]?[1]?["id"];
                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            raResp.dob = dob;
                            raResp.doc_id_number = docIdNumber;
                            raResp.dbss_subscription_id = (int)dbssRespObj?["data"]?[0]?["id"];
                            raResp.old_sim_number = (string)dbssRespObj?["included"]?[0]?["attributes"]?["icc"];
                            raResp.old_sim_type = (string)dbssRespObj?["included"]?[0]?["attributes"]?["sim-type"];
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = MessageCollection.DataNotFound;
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        /// in Include attribute for:- 
        ///                index 0: owner-customer
        ///                index 1: sim-cards
        ///                index 2: user-customer
        /// </summary>
        /// <param name="dbssRespObj"></param>
        /// <returns></returns>
        /// 
        private IndividualSIMReplacementMSISDNCheckResponse IndividualSIMReplacementMSISDNReqParsingV2(JObject dbssRespObj)
        {
            IndividualSIMReplacementMSISDNCheckResponse raResp = new IndividualSIMReplacementMSISDNCheckResponse();
            try
            {
                if(dbssRespObj != null)
                {
                    if (!dbssRespObj.ContainsKey("data") || dbssRespObj["data"] == null || !dbssRespObj["data"].Any())
                    {
                        raResp.result = false;
                        raResp.message = MessageCollection.SIMReplNoDataFound;
                        return raResp;
                    }

                    if (dbssRespObj["data"][0]["attributes"] == null ||
                        dbssRespObj["data"][0]["attributes"]["status"] == null ||
                        String.IsNullOrEmpty((string)dbssRespObj["data"][0]["attributes"]["status"]))
                    {
                        raResp.result = false;
                        raResp.message = "Msisdn status not found!";
                        return raResp;
                    }

                    if ((string)dbssRespObj["data"][0]["attributes"]["status"] == "terminated")
                    {
                        raResp.result = false;
                        raResp.message = "Msisdn is not valid for SIM replacemnt!";
                        return raResp;
                    }

                    if ((string)dbssRespObj?["data"]?[0]?["attributes"]?["status"] != "active"
                         && (string)dbssRespObj?["data"]?[0]?["attributes"]?["status"] != "idle")
                    {
                        raResp.result = false;
                        raResp.message = MessageCollection.MSISDNStatusNotActiveOrIdle;
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                        return raResp;
                    }

                    if (dbssRespObj["included"] == null
                        || dbssRespObj?["included"]?.Count() != 3)
                    {
                        raResp.result = false;
                        raResp.message = "Data not found in include field!";
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                        return raResp;
                    }

                    if (dbssRespObj?["data"]?[0]?["id"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Subscription ID field empty!";
                        return raResp;
                    }
                    if (dbssRespObj?["included"]?[0]?["attributes"] == null
                        || dbssRespObj?["included"]?[1]?["attributes"] == null
                        || dbssRespObj?["included"]?[2]?["attributes"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "No data in attributes in included!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj?["included"]?[1]?["attributes"]?["icc"]))
                    {
                        raResp.result = false;
                        raResp.message = "Old SIM number not found!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj["included"]?[1]?["attributes"]?["sim-type"]))
                    {
                        raResp.result = false;
                        raResp.message = "sim-type not found!";
                        return raResp;
                    }

                    if (dbssRespObj["included"]?[0]?["attributes"]?["is-company"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Company information not found!";
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                        return raResp;
                    }

                    if (dbssRespObj["included"]?[0]?["attributes"]?["id-document-type"] == null
                         || String.IsNullOrEmpty((string)dbssRespObj["included"]?[0]?["attributes"]?["id-document-type"]))
                    {
                        raResp.result = false;
                        raResp.message = "id-document-type not found!";
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                        return raResp;
                    }

                    string idDocumentType = (string)dbssRespObj["included"]?[0]?["attributes"]?["id-document-type"];

                    if (idDocumentType != "national_id"
                        && idDocumentType != "smart_national_id")
                    {
                        raResp.result = false;
                        raResp.message = "Customer is not registered with National ID.";
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                        return raResp;
                    }
                    else if ((bool)dbssRespObj["included"]?[0]?["attributes"]?["is-company"] == true)
                    {
                        raResp.result = false;
                        raResp.message = "This MSISDN is not eligible for individual SIM replacement.";
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                        return raResp;
                    }
                    else
                    {
                        var includedObj = dbssRespObj?["included"];

                        string firstName = includedObj?[2]?["attributes"]?["first-name"]?.ToString() ?? string.Empty;
                        raResp.saf_status = !String.IsNullOrEmpty(firstName);

                        raResp.customer_id = includedObj?[2]?["id"]?.ToString() ?? string.Empty;

                        raResp.dob = includedObj?[0]?["attributes"]?["date-of-birth"]?.ToString() ?? string.Empty;
                        raResp.doc_id_number = includedObj?[0]?["attributes"]?["id-document-number"]?.ToString() ?? string.Empty;

                        if (dbssRespObj?["data"]?[0]?["id"] != null && int.TryParse(dbssRespObj?["data"]?[0]?["id"]?.ToString(), out var subscriptionId))
                        {
                            raResp.dbss_subscription_id = subscriptionId;
                        }
                        else
                        {
                            raResp.dbss_subscription_id = 0;
                        }

                        raResp.old_sim_number = includedObj?[1]?["attributes"]?["icc"]?.ToString() ?? string.Empty;
                        raResp.old_sim_type = includedObj?[1]?["attributes"]?["sim-type"]?.ToString() ?? string.Empty;

                        raResp.result = true;
                        raResp.message = MessageCollection.MSISDNValid;

                        return raResp;
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.SIMReplNoDataFound;
                    return raResp;
                }
                //else
                //{
                //    string firstName = dbssRespObj["included"]?[2]?["attributes"]?["first-name"] != null ? Convert.ToString(dbssRespObj["included"]?[2]?["attributes"]?["first-name"]) : "";
                //    raResp.saf_status = String.IsNullOrEmpty(firstName) ? false : true;
                //    raResp.customer_id = dbssRespObj?["included"]?[2]?["id"] != null ? Convert.ToString(dbssRespObj?["included"]?[2]?["id"]) :"";

                //    raResp.dob = (string)dbssRespObj?["included"]?[0]?["attributes"]?["date-of-birth"];
                //    raResp.doc_id_number = (string)dbssRespObj?["included"]?[0]?["attributes"]?["id-document-number"];//Nid

                //    raResp.dbss_subscription_id = (int)dbssRespObj?["data"]?[0]?["id"];
                //    raResp.old_sim_number = (string)dbssRespObj?["included"]?[1]?["attributes"]?["icc"];
                //    raResp.old_sim_type = (string)dbssRespObj?["included"]?[1]?["attributes"]?["sim-type"];
                //    raResp.result = true;
                //    raResp.message = MessageCollection.MSISDNValid;
                //    return raResp;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// in Include attribute for:- 
        ///                index 0: owner-customer
        ///                index 1: sim-cards
        ///                index 2: user-customer
        /// </summary>
        /// <param name="dbssRespObj"></param>
        /// <returns></returns>
        /// 
        public IndividualSIMReplacementMSISDNCheckResponse IndividualSIMReplacementMSISDNReqParsingV3(JObject dbssRespObj)
        {
            IndividualSIMReplacementMSISDNCheckResponse raResp = new IndividualSIMReplacementMSISDNCheckResponse();
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.SIMReplNoDataFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"]))
                {
                    raResp.result = false;
                    raResp.message = "Msisdn status not found!";
                    return raResp;
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] == "terminated")
                {
                    raResp.result = false;
                    raResp.message = "Msisdn is not valid for SIM replacemnt!";
                    return raResp;
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                     && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNStatusNotActiveOrIdle;
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                if (!dbssRespObj["included"].HasValues
                    || (dbssRespObj["included"].Count() != 2
                    && dbssRespObj["included"].Count() != 3))
                {
                    raResp.result = false;
                    raResp.message = "Data not found in include field!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                if (dbssRespObj["data"]["id"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Subscription ID field empty!";
                    return raResp;
                }
                if (dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Data not found in include field!";
                    return raResp;
                }
                if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["icc"]))
                {
                    raResp.result = false;
                    raResp.message = "Old SIM number not found!";
                    return raResp;
                }
                if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["sim-type"]))
                {
                    raResp.result = false;
                    raResp.message = "sim-type not found!";
                    return raResp;
                }

                if (dbssRespObj["included"][0]["attributes"]["is-company"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Company information not found!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                if (dbssRespObj["included"][0]["attributes"]["id-document-type"] == null
                     || String.IsNullOrEmpty((string)dbssRespObj["included"][0]["attributes"]["id-document-type"]))
                {
                    raResp.result = false;
                    raResp.message = "id-document-type not found!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }

                string idDocumentType = (string)dbssRespObj["included"][0]["attributes"]["id-document-type"];

                if (idDocumentType != "national_id"
                    && idDocumentType != "smart_national_id")
                {
                    raResp.result = false;
                    raResp.message = "Customer is not registered with National ID!";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                else if ((bool)dbssRespObj["included"][0]["attributes"]["is-company"] == true)
                {
                    raResp.result = false;
                    raResp.message = "This MSISDN is not eligible for individual SIM replacement.";
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                else
                {
                    raResp.saf_status = true;//[Has_SAF] By deafult this value is true. At this moment we are not checking saf status because DBSS API   
                    raResp.customer_id = String.Empty;
                    raResp.dob = (string)dbssRespObj["included"][0]["attributes"]["date-of-birth"];
                    raResp.doc_id_number = (string)dbssRespObj["included"][0]["attributes"]["id-document-number"];
                    raResp.dbss_subscription_id = (int)dbssRespObj["data"]["id"];
                    raResp.old_sim_number = (string)dbssRespObj["included"][1]["attributes"]["icc"];
                    raResp.old_sim_type = (string)dbssRespObj["included"][1]["attributes"]["sim-type"];
                    raResp.result = true;
                    raResp.message = MessageCollection.MSISDNValid;
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public RACommonResponse UnpairedMSISDNReqParsingForMNPProtIn(JObject dbssRespObj)
        {
            RACommonResponse raResp = new RACommonResponse();
            try
            {
                if (dbssRespObj["data"] == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.NoDataFound;
                    return raResp;
                }

                if (dbssRespObj["data"]["attributes"] == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["is-controlled"]))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                }

                if ((bool)dbssRespObj["data"]["attributes"]["is-controlled"] == true)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNAlreadyExists;
                }
                else
                {
                    raResp.result = true;
                    raResp.message = MessageCollection.Success;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }

        public MSISDNCheckResponse PreToPostMSISDNReqParsing(JObject dbssRespObj)
        {
            MSISDNCheckResponse raResp = new MSISDNCheckResponse();
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.SIMReplNoDataFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"]))
                {
                    raResp.result = false;
                    raResp.message = "Msisdn status not found!";
                    return raResp;
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] == "terminated")
                {
                    raResp.result = false;
                    raResp.message = "Msisdn is not valid for prepaid to postpaid!";
                    return raResp;
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                     && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNStatusNotActiveOrIdle;
                    raResp.dob = null;
                    raResp.nid = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                if ((string)dbssRespObj["data"]["attributes"]["payment-type"] == "postpaid")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.PreToPostMigrationFailedMessage;
                    raResp.dob = null;
                    raResp.nid = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                if (!dbssRespObj["included"].HasValues || dbssRespObj["included"].Count() < 2)
                {
                    raResp.result = false;
                    raResp.message = "Data not found in include field!";
                    raResp.dob = null;
                    raResp.nid = null;
                    raResp.saf_status = false;
                    return raResp;
                }
                if (dbssRespObj["data"]["id"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Subscription ID field empty!";
                    return raResp;
                }
                if (dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Data not found in attributes field!";
                    return raResp;
                }
                //bool isCompany = false;
                //try { isCompany = (bool)dbssRespObj["included"]?[2]?["attributes"]?["is-company"]; }
                //catch (Exception ex) { throw new Exception(ex.Message); }

                //if (isCompany == true)
                //{
                //    raResp.result = false;
                //    raResp.message = "This MSISDN is not eligible for this operation.";
                //    raResp.dob = null;
                //    raResp.nid = null;
                //    raResp.saf_status = false;
                //    return raResp;
                //}
                else
                {
                    string firstName = string.Empty;
                    string userCustomerId = string.Empty;
                    string ownerCustomerId = string.Empty;
                    int totalData = dbssRespObj["included"].Count();

                    try { userCustomerId = (string)dbssRespObj["data"]?["relationships"]?["user-customer"]?["data"]?["id"]; }
                    catch (Exception) { throw new Exception("Data not found in user-customer!"); }

                    try { ownerCustomerId = (string)dbssRespObj["data"]?["relationships"]?["owner-customer"]?["data"]?["id"]; }
                    catch (Exception) { throw new Exception("Data not found in owner-customer!"); }

                    string idDocumentType = string.Empty;
                    string dedicatedID = string.Empty;
                    string[] dedicatedArr = null;
                    try
                    {
                        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                        dedicatedID = configuration.GetSection("AppSettings:dedicated_Ac_Id").Value;
                        //dedicatedID = System.Configuration.ConfigurationManager.AppSettings["dedicated_Ac_Id"]; 
                    }
                    catch (Exception) { throw new Exception("Data not found in web config."); }

                    if (dedicatedID.Contains(','))
                    {
                        dedicatedArr = dedicatedID.Split(',');
                    }
                    else
                    {
                        dedicatedArr = dedicatedID.Split(' ');
                    }

                    for (int i = 0; i < totalData; i++)
                    {
                        //if (!String.IsNullOrEmpty((string)dbssRespObj["included"]?[i]["attributes"]?["dedicated-account-id"])
                        //    && !String.IsNullOrEmpty((string)dbssRespObj["included"]?[i]["attributes"]?["amount"]))
                        //{
                        //    raResp.dedicated_Ac_Id = (string)dbssRespObj["included"]?[i]["attributes"]?["dedicated-account-id"];
                        //    raResp.amount = (decimal)dbssRespObj["included"]?[i]["attributes"]?["amount"];

                        //    if (dedicatedArr.Any(x => x.Equals(raResp.dedicated_Ac_Id)) && raResp.amount > 0)
                        //    {
                        //        raResp.result = false;
                        //        raResp.message = "Customer is due with loan amount: " + raResp.amount.ToString() + "Tk";
                        //        return raResp;
                        //    }
                        //}
                        var dedicatedAccountId = dbssRespObj["included"]?[i]?["attributes"]?["dedicated-account-id"]?.ToString();
                        var amountString = dbssRespObj["included"]?[i]?["attributes"]?["amount"]?.ToString();

                        if (!string.IsNullOrEmpty(dedicatedAccountId) && !string.IsNullOrEmpty(amountString) && decimal.TryParse(amountString, out var amount))
                        {
                            raResp.dedicated_Ac_Id = dedicatedAccountId;
                            raResp.amount = amount;

                            if (dedicatedArr.Any(x => x.Equals(raResp.dedicated_Ac_Id)) && raResp.amount > 0)
                            {
                                raResp.result = false;
                                raResp.message = $"Customer is due with loan amount: {raResp.amount} Tk";
                                return raResp;
                            }
                        }
                        if (ownerCustomerId != null)
                        {
                            if (ownerCustomerId.Equals((string)dbssRespObj["included"]?[i]["attributes"]?["id-document-type"]))
                            {
                                idDocumentType = (string)dbssRespObj["included"]?[i]["attributes"]?["id-document-type"];

                                if (idDocumentType != "national_id")
                                {
                                    if (idDocumentType != "smart_national_id")
                                    {
                                        raResp.result = false;
                                        raResp.message = "Customer is not registered with NID or Smart NID!";
                                        raResp.dob = null;
                                        raResp.nid = null;
                                        raResp.saf_status = false;
                                        return raResp;
                                    }
                                }
                            }
                        }
                        if (userCustomerId != null && userCustomerId.Equals((string)dbssRespObj["included"]?[i]["id"]))
                        {
                            firstName = (string)dbssRespObj["included"]?[i]["attributes"]?["first-name"];
                            if (!String.IsNullOrEmpty(firstName))
                            {
                                raResp.saf_status = true;//[Has_SAF] By deafult this value is true. At this moment we are not checking saf status because DBSS API
                                raResp.customer_id = userCustomerId;
                            }
                            else
                            {
                                raResp.saf_status = false;
                                raResp.customer_id = userCustomerId;
                            }
                        }
                        else if (ownerCustomerId != null && ownerCustomerId.Equals((string)dbssRespObj["included"]?[i]["id"]))
                        {
                            try { raResp.dob = (string)dbssRespObj["included"]?[i]["attributes"]?["date-of-birth"]; }
                            catch (Exception) { }

                            try { raResp.nid = (string)dbssRespObj["included"]?[i]["attributes"]?["id-document-number"]; }
                            catch (Exception) { }

                            if (String.IsNullOrEmpty(raResp.dob) || String.IsNullOrEmpty(raResp.nid))
                            {
                                raResp.result = false;
                                raResp.message = "date-of-birth or id-document-number is not exist!";
                                raResp.dob = null;
                                raResp.nid = null;
                                raResp.saf_status = false;
                                return raResp;
                            }
                        }
                    }
                    raResp.dbss_subscription_id = (int)dbssRespObj["data"]?["id"];
                    raResp.result = true;
                    raResp.message = MessageCollection.MSISDNValid;
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //User to User
        internal IndividualSIMReplacementMSISDNCheckResponse PopulateDataForGeneralUserToGeneralUserSIMRepl(JObject dbssRespObj)
        {
            IndividualSIMReplacementMSISDNCheckResponse raResp = new IndividualSIMReplacementMSISDNCheckResponse();
            try
            {
                if (dbssRespObj["included"].Count() == 3
                    && dbssRespObj["included"].HasValues)
                {
                    if (dbssRespObj["data"][0]["id"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Subscription ID field empty!";
                        return raResp;
                    }
                    if (dbssRespObj["data"][0]["attributes"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Data field empty!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["icc"]))
                    {
                        raResp.result = false;
                        raResp.message = "Old SIM number not found!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["sim-type"]))
                    {
                        raResp.result = false;
                        raResp.message = "sim-type not found!";
                        return raResp;
                    }
                    if (!String.IsNullOrEmpty((string)dbssRespObj["data"][0]["attributes"]["status"])
                        && dbssRespObj["included"][0]["attributes"]["is-company"] != null
                        && !String.IsNullOrEmpty((string)dbssRespObj["included"][0]["attributes"]["id-document-type"]))
                    {
                        string msdidnStatus = (string)dbssRespObj["data"][0]["attributes"]["status"];
                        bool isCompany = (bool)dbssRespObj["included"][0]["attributes"]["is-company"];
                        string idDocumentType = (string)dbssRespObj["included"][0]["attributes"]["id-document-type"];

                        if (msdidnStatus != "active"
                            && msdidnStatus != "idle")
                        {
                            raResp.result = false;
                            raResp.message = "This MSISDN is not in active status.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else if (idDocumentType != "national_id" && idDocumentType != "smart_national_id")
                        {
                            raResp.result = false;
                            raResp.message = "Customer is not registered with National ID.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else if (isCompany == true)
                        {
                            raResp.result = false;
                            raResp.message = "This MSISDN is not eligible for individual SIM replacement.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else
                        {
                            string firstName = (string)dbssRespObj["included"][0]["attributes"]["first-name"];
                            raResp.saf_status = String.IsNullOrEmpty(firstName) ? false : true;
                            string docIdNumber = (string)dbssRespObj["included"][0]["attributes"]["id-document-number"];//Nid
                            string dob = (string)dbssRespObj["included"][0]["attributes"]["date-of-birth"];
                            raResp.customer_id = (string)dbssRespObj["included"][0]["id"];
                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            raResp.dob = dob;
                            raResp.doc_id_number = docIdNumber;
                            raResp.dbss_subscription_id = (int)dbssRespObj["data"][0]["id"];
                            raResp.old_sim_number = (string)dbssRespObj["included"][1]["attributes"]["icc"];
                            raResp.old_sim_type = (string)dbssRespObj["included"][1]["attributes"]["sim-type"];
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = MessageCollection.DataNotFound;
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //POC to User
        internal IndividualSIMReplacementMSISDNCheckResponse PopulateDataForPOCUserToGeneralUserSIMRepl(JObject dbssRespObj)
        {
            IndividualSIMReplacementMSISDNCheckResponse raResp = new IndividualSIMReplacementMSISDNCheckResponse();
            try
            {
                if (dbssRespObj["included"].Count() == 3
                && dbssRespObj["included"].HasValues)
                {
                    if (dbssRespObj["data"][0]["id"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Subscription ID field empty!";
                        return raResp;
                    }
                    if (dbssRespObj["data"][0]["attributes"] == null)
                    {
                        raResp.result = false;
                        raResp.message = "Data field empty!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["icc"]))
                    {
                        raResp.result = false;
                        raResp.message = "Old SIM number not found!";
                        return raResp;
                    }
                    if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["sim-type"]))
                    {
                        raResp.result = false;
                        raResp.message = "sim-type not found!";
                        return raResp;
                    }
                    if (dbssRespObj["included"][0]["attributes"]["is-company"] != null
                        && !String.IsNullOrEmpty((string)dbssRespObj["included"][0]["attributes"]["id-document-type"]))
                    {
                        string idDocumentType = (string)dbssRespObj["included"][0]["attributes"]["id-document-type"];

                        if (idDocumentType != "national_id"
                            && idDocumentType != "smart_national_id")
                        {
                            raResp.result = false;
                            raResp.message = "Customer is not registered with National ID.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else if ((bool)dbssRespObj["included"][0]["attributes"]["is-company"] == true)
                        {
                            raResp.result = false;
                            raResp.message = "This MSISDN is not eligible for individual SIM replacement.";
                            raResp.dob = null;
                            raResp.doc_id_number = null;
                            raResp.saf_status = false;
                        }
                        else
                        {
                            string firstName = (string)dbssRespObj["included"][2]["attributes"]["first-name"];
                            raResp.saf_status = String.IsNullOrEmpty(firstName) ? false : true;
                            raResp.customer_id = (string)dbssRespObj["included"][2]["id"];


                            raResp.dob = (string)dbssRespObj["included"][0]["attributes"]["date-of-birth"];
                            raResp.doc_id_number = (string)dbssRespObj["included"][0]["attributes"]["id-document-number"];//Nid

                            raResp.dbss_subscription_id = (int)dbssRespObj["data"][0]["id"];
                            raResp.old_sim_number = (string)dbssRespObj["included"][1]["attributes"]["icc"];
                            raResp.old_sim_type = (string)dbssRespObj["included"][1]["attributes"]["sim-type"];
                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = MessageCollection.DataNotFound;
                        raResp.dob = null;
                        raResp.doc_id_number = null;
                        raResp.saf_status = false;
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    raResp.dob = null;
                    raResp.doc_id_number = null;
                    raResp.saf_status = false;
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public SIMReplacementMSISDNCheckResponse CorporateSIMReplacementCustomerInfoReqParsing(CorporateSIMReplacemnetCustomerInfoRootobject dbssRespObj, string pocMsisdnNo)
        {
            SIMReplacementMSISDNCheckResponse raResp = new SIMReplacementMSISDNCheckResponse();
            ///SIMReplacementMSISDNCheckResponse raResp = null;
            try
            {
                if (dbssRespObj.data.attributes == null)
                {
                    return raResp = new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = "Customer info not found."
                    };
                }

                if (dbssRespObj.data.attributes.contactphone != null)
                {
                    if(pocMsisdnNo.Substring(0,2) == "88")
                    {
                        if (!dbssRespObj.data.attributes.contactphone.Trim().Equals(pocMsisdnNo))
                        {
                            return raResp = new SIMReplacementMSISDNCheckResponse()
                            {
                                result = false,
                                message = "Child MSISDN does not belong to the POC."
                            };
                        }
                    }
                    else
                    {
                        if (!dbssRespObj.data.attributes.contactphone.Trim().Equals("88" + pocMsisdnNo))
                        {
                            return raResp = new SIMReplacementMSISDNCheckResponse()
                            {
                                result = false,
                                message = "Child MSISDN does not belong to the POC."
                            };
                        }
                    }
                    
                }

                if (dbssRespObj.data.attributes.iddocumenttype != null
                    && !dbssRespObj.data.attributes.iddocumenttype.Contains("national_id"))
                {
                    return raResp = new SIMReplacementMSISDNCheckResponse()
                    {
                        result = false,
                        message = "Customer is not registired with National ID or Smart National ID."
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return raResp = new SIMReplacementMSISDNCheckResponse()
            {
                doc_id_number = dbssRespObj.data.attributes.iddocumentnumber,
                dob = dbssRespObj.data.attributes.dateofbirth,
                result = true,
                message = MessageCollection.MSISDNValid
            };
        }

        #region OTP Validation Res-Parsing
        /// <summary>
        /// OTP Res-Parsing
        /// </summary>
        /// <param name="dbssRespObj"></param>
        /// <returns></returns>
        public OTPResponse OTPRespParsing(DBSSOTPResponseRootobject dbssRespObj)
        {
            try
            {
                if (dbssRespObj.data == null)
                {
                    return new OTPResponse()
                    {
                        is_otp_valid = false,
                        result = false,
                        message = MessageCollection.InvalidOTP
                    };
                }
                return new OTPResponse()
                {
                    is_otp_valid = true,
                    result = true,
                    message = MessageCollection.ValidOTP
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public OTPResponseRev OTPRespParsingV2(DBSSOTPResponseRootobject dbssRespObj)
        {
            try
            {
                if (dbssRespObj.data == null)
                {
                    return new OTPResponseRev()
                    {
                        isError = true,
                        message = MessageCollection.InvalidOTP,
                        data = new OTPRespData()
                        {
                            is_otp_valid = false
                        }
                    };
                }
                return new OTPResponseRev()
                {
                    isError = false,
                    message = MessageCollection.ValidOTP,
                    data = new OTPRespData()
                    {
                        is_otp_valid = true
                    }
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        public CorporateSIMReplacementCheckResponseWithCustomerId CorporateSIMReplacementMSISDNReqParsing(CorporateSIMReplacementResponseRootobject dbssRespObj)
        {
            CorporateSIMReplacementCheckResponseWithCustomerId response = null;
            string customerId = null;
            try
            {
                if (dbssRespObj.data.Count <= 0)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "No data found!"
                    };
                }

                if (dbssRespObj.data[0].id == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "Subscription Id field empty!"
                    };
                }

                if (dbssRespObj.data[0].attributes == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "Data not found."
                    };
                }

                if (dbssRespObj.data[0].relationships == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "Related data field not found."
                    };
                }

                if (dbssRespObj.data[0].attributes.status == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "Data not found."
                    };
                }

                string msdidnStatus = dbssRespObj.data[0].attributes.status;

                if (msdidnStatus != "active" && msdidnStatus != "idle")
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "MSISDN is not in active status."
                    };
                }

                if (dbssRespObj.data[0].relationships.coordinatorcustomer == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "Co-ordinator customer info not found."
                    };
                }

                if (dbssRespObj.data[0].relationships.coordinatorcustomer.data == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "MSISDN does not belong to a corporate number."
                    };
                }

                if (dbssRespObj.data[0].relationships.coordinatorcustomer.data.id == null)
                {
                    return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                    {
                        result = false,
                        message = "Customer ID field empty."
                    };
                }

                customerId = dbssRespObj.data[0].relationships.coordinatorcustomer.data.id;

                return response = new CorporateSIMReplacementCheckResponseWithCustomerId()
                {
                    dbss_subscription_id = Convert.ToInt64(dbssRespObj.data[0].id),//intialized subscription id
                    result = true,
                    message = MessageCollection.Success,
                    customer_id = customerId,
                    old_sim_number = ""
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public CorporateSIMReplacementCheckResponseWithCustomerId CorporateSIMReplacementMSISDNReqParsing2(JObject dbssRespObj)
        {
            CorporateSIMReplacementCheckResponseWithCustomerId raResp = new CorporateSIMReplacementCheckResponseWithCustomerId();

            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() < 1
                    || !dbssRespObj["included"].HasValues
                    || dbssRespObj["included"].Count() < 2)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.SIMReplNoDataFound;
                    return raResp;
                }

                if (dbssRespObj["data"]["id"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Subscription ID field empty!";
                    return raResp;
                }

                if (dbssRespObj["data"]["attributes"] == null
                    || dbssRespObj["data"]["relationships"] == null
                    || dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.SIMReplNoDataFound;
                    return raResp;
                }

                if (dbssRespObj["data"]["relationships"]["coordinator-customer"] == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.POCInfoNotFound;
                    return raResp;
                }

                if (dbssRespObj["data"]["relationships"]["coordinator-customer"]["data"] == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.POCInfoNotFound;
                    return raResp;
                }

                if (dbssRespObj["data"]["relationships"]["coordinator-customer"]["data"]["id"] == null)
                {
                    raResp.result = false;
                    raResp.message = "Customer ID not found!";
                    return raResp;
                }

                if ((bool)dbssRespObj["included"][0]["attributes"]["is-company"] == false)
                {
                    raResp.result = false;
                    raResp.message = "This MSISDN is not eligible for corporate SIM replacement.";
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["included"][1]["attributes"]["icc"]))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.OldSIMNotFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                    || dbssRespObj["included"][0]["attributes"]["is-company"] == null
                    || String.IsNullOrEmpty((string)dbssRespObj["included"][0]["attributes"]["id-document-type"]))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                string oldSim = (string)dbssRespObj["included"][1]["attributes"]["icc"];

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                    && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    raResp.result = false;
                    raResp.message = "This MSISDN is not in active or idle status.";
                    return raResp;
                }

                raResp.result = true;
                raResp.message = MessageCollection.MSISDNValid;
                raResp.dbss_subscription_id = (int)dbssRespObj["data"]["id"];
                raResp.old_sim_number = oldSim;
                raResp.customer_id = (string)dbssRespObj["data"]["relationships"]["coordinator-customer"]["data"]["id"];
                raResp.old_sim_type = (string)dbssRespObj["included"][1]["attributes"]["sim-type"];

                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public PaiedMSISDNCheckResponse PairedMSISDNReqParsing2(PairedMSISDNValidationResponseRootobject dbssRespObj)
        {
            PaiedMSISDNCheckResponse raResp = new PaiedMSISDNCheckResponse();
            string simNo = String.Empty;
            try
            {
                if (dbssRespObj.data.attributes == null)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty(dbssRespObj.data.attributes.msisdn)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.status)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.icc)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.subscriptionType)
                    )
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (dbssRespObj.data.attributes.status != FixedValueCollection.ValidPairedMSISDNStatus)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInvalid;
                    return raResp;
                }

                raResp.result = true;
                raResp.sim_number = dbssRespObj.data.attributes.icc.Remove(0, FixedValueCollection.SIMCode.Length);
                raResp.subscription_type_code = dbssRespObj.data.attributes.subscriptionType;
                raResp.imsi = dbssRespObj.data.attributes.imsi;
                raResp.message = MessageCollection.MSISDNValid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }
        public PaiedMSISDNCheckResponseDataRev PairedMSISDNReqParsingV3(PairedMSISDNValidationResponseRootobject dbssRespObj)
        {
            PaiedMSISDNCheckResponseDataRev raResp = new PaiedMSISDNCheckResponseDataRev();
            string simNo = String.Empty;
            try
            {
                if (dbssRespObj.data.attributes == null)
                {
                    raResp.isError = true;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty(dbssRespObj.data.attributes.msisdn)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.status)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.icc)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.subscriptionType)
                    )
                {
                    raResp.isError = true;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (dbssRespObj.data.attributes.status != FixedValueCollection.ValidPairedMSISDNStatus)
                {
                    raResp.isError = true;
                    raResp.message = MessageCollection.MSISDNInvalid;
                    return raResp;
                }

                raResp.isError = false;
                if(dbssRespObj.data != null)
                {
                    raResp.data = new PaiedMSISDNCheckResponseRev()
                    {
                        sim_number = dbssRespObj.data.attributes.icc.Remove(0, FixedValueCollection.SIMCode.Length),
                        subscription_type_code = dbssRespObj.data.attributes.subscriptionType,
                        imsi = dbssRespObj.data.attributes.imsi,
                        
                    };
                }
                raResp.message = MessageCollection.MSISDNValid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }

        public PaiedMSISDNCheckResponseDataRevV1 PairedMSISDNReqParsingV4(PairedMSISDNValidationResponseRootobject dbssRespObj)
        {
            PaiedMSISDNCheckResponseDataRevV1 raResp = new PaiedMSISDNCheckResponseDataRevV1();
            string simNo = String.Empty;
            try
            {
                if (dbssRespObj.data.attributes == null)
                {
                    raResp.isError = true;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (String.IsNullOrEmpty(dbssRespObj.data.attributes.msisdn)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.status)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.icc)
                    || String.IsNullOrEmpty(dbssRespObj.data.attributes.subscriptionType)
                    )
                {
                    raResp.isError = true;
                    raResp.message = MessageCollection.DataNotFound;
                    return raResp;
                }

                if (dbssRespObj.data.attributes.status != FixedValueCollection.ValidPairedMSISDNStatus)
                {
                    raResp.isError = true;
                    raResp.message = MessageCollection.MSISDNInvalid;
                    return raResp;
                }

                raResp.isError = false;
                if (dbssRespObj.data != null)
                {
                    raResp.data = new PaiedMSISDNCheckResponseRevV1()
                    {
                        sim_number = dbssRespObj.data.attributes.icc.Remove(0, FixedValueCollection.SIMCode.Length),
                        subscription_type_code = dbssRespObj.data.attributes.subscriptionType,
                        imsi = dbssRespObj.data.attributes.imsi,
                        number_category=dbssRespObj.data.attributes.numbercategory

                    };
                }
                raResp.message = MessageCollection.MSISDNValid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }

        public PaiedMSISDNCheckResponse PairedMSISDNReqParsing(JObject dbssRespObj)
        {
            PaiedMSISDNCheckResponse raResp = new PaiedMSISDNCheckResponse();
            string simNo = String.Empty;
            try
            {

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (dbssRespObj["data"]["attributes"]["icc"] != null)
                        {
                            simNo = (string)dbssRespObj["data"]["attributes"]["icc"];
                        }
                    }
                }

                if (!String.IsNullOrEmpty(simNo))
                {
                    raResp.result = true;
                    raResp.message = "MSISDN is Valid.";
                    raResp.sim_number = simNo;
                }
                else
                {
                    raResp.result = false;
                    raResp.sim_number = null;
                    raResp.message = "SIM number is not attached with MSISDN.";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }

        public UnpairedMSISDNCheckResponse UnpairedMSISDNReqParsing(JObject dbssRespObj, string retailer_id)
        {
            UnpairedMSISDNCheckResponse raResp = new UnpairedMSISDNCheckResponse();
            try
            {
                string status = String.Empty;
                int stockId = 0;
                string reserved_for = string.Empty;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                        }
                    }
                }
                if(stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNReserved;
                    return raResp;
                }
                if (status == "available")
                {
                    raResp = ValidateCherishedNumer(dbssRespObj, retailer_id);
                    raResp.stock_id = stockId;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public PairedMSISDNDataRev PairedMSISDNSearchParsing(JObject dbssRespObj)
        {
            PairedMSISDNDataRev raResp = new PairedMSISDNDataRev();
            try
            {
                if (dbssRespObj != null)
                {
                    if (dbssRespObj["data"] != null)
                    {
                        if (dbssRespObj["data"]?["relationships"] != null)
                        {
                            var msisdnData = dbssRespObj["data"]?["relationships"]?["msisdn"]?["data"];

                            if (msisdnData != null)
                            {
                                raResp.data = new ReponseDataRev()
                                {
                                    msisdn = (string)msisdnData["id"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return raResp;
        }

        public UnpairedMSISDNCheckResponse UnpairedMSISDNReqParsingV2(JObject dbssRespObj, string retailer_id)
        {
            UnpairedMSISDNCheckResponse raResp = new UnpairedMSISDNCheckResponse();
            try
            {
                string status = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;
                string reserved_for = string.Empty;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                        }
                    }
                }
                if(stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNReserved;
                    return raResp;
                }
                if (status == "available")
                {
                    raResp = ValidateCherishedNumer(dbssRespObj, retailer_id);
                    raResp.stock_id = stockId;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public async Task<CherishedMSISDNCheckResponse> UnpairedMSISDNReqParsingV3(JObject dbssRespObj, string retailer_id, string channel_name)
        {

            CherishedMSISDNCheckResponse raResp = new CherishedMSISDNCheckResponse();
            try
            {
                string status = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;
                string reserved_for = string.Empty;
                string cherish_category_config = string.Empty;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                            number_category = (string)dbssRespObj["data"]["attributes"]["number-category"];
                        }
                    }
                }
                if (stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNReserved;
                    return raResp;
                }
                if (status == "available")
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                    cherish_category_config = configuration.GetSection("AppSettings:cherish_categories").Value;
                    if (cherish_category_config.Contains(","))
                    {
                        cofigValue = cherish_category_config.Split(',');
                    }
                    else
                    {
                        cofigValue = cherish_category_config.Split(' ');
                    }

                    if (cofigValue.Any(x => x == number_category))
                    {
                        var category = cofigValue.Where(x => x.Equals(number_category)).FirstOrDefault();
                        if (category != null)
                        {
                            var catInfo= await _bllCommon.GetDesiredCategoryMessage(category,channel_name);

                            raResp.data_message = catInfo.message;
                            raResp.category_name = catInfo.name;
                            raResp.isDesiredCategory = true;
                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                        }
                        
                    }
                    else
                    {
                        raResp = ValidateCherishedNumerV2(dbssRespObj, retailer_id);
                        raResp.message = MessageCollection.MSISDNValid;
                        raResp.isDesiredCategory = false;
                    }
                    raResp.stock_id = stockId;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public UnpairedMSISDNCheckResponse ValidateCherishedNumer(JObject dbssRespObj, string retailer_id)
        {

            UnpairedMSISDNCheckResponse raResp = new UnpairedMSISDNCheckResponse();

            string status = String.Empty;
            int stockId = 0;
            string retailer_code = String.Empty;
            string number_category = String.Empty;
            string category_config = String.Empty;
            string[] cofigValue = null;

            try
            {
                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        try
                        {
                            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                            category_config = configuration.GetSection("AppSettings:number_category").Value;

                        }
                        catch (Exception) { throw new Exception("Key not found in appsettings"); }

                        if (category_config.Contains(","))
                        {
                            cofigValue = category_config.Split(',');
                        }
                        else
                        {
                            cofigValue = category_config.Split(' ');
                        }

                        if (dbssRespObj["data"]["attributes"]["number-category"] != null)
                        {
                            retailer_code = dbssRespObj["data"]["attributes"]["salesman-id"].ToString();
                            number_category = dbssRespObj["data"]["attributes"]["number-category"].ToString();

                            if (!String.IsNullOrEmpty(retailer_code))
                            {
                                if (retailer_code.Length < 6)
                                {
                                    char pad = '0';
                                    retailer_code = retailer_code.PadLeft(6, pad);
                                }
                            }

                            if (!String.IsNullOrEmpty(retailer_code) && !String.IsNullOrEmpty(number_category) && cofigValue.Any(x => x != number_category)) // from Web.config 
                            {
                                if (retailer_id.Equals(retailer_code))
                                {
                                    raResp.result = true;
                                    raResp.message = MessageCollection.ValidCherishedNumber;
                                }
                                else
                                {
                                    raResp.result = false;
                                    raResp.message = MessageCollection.InvalidCherishedNumber;
                                }
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber;
                            }
                            else if (!String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber; ;
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x != number_category))
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN not tagged with this Retailer (ID: " + retailer_id + ")";
                            }
                            else
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN is not Valid.";
                            }
                        }
                        else
                        {
                            raResp.result = false;
                            raResp.message = "Invalid MSISDN Category!";
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "No Data found!";
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "No Data found!";
                }

                return raResp;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public CherishedMSISDNCheckResponse ValidateCherishedNumerV2(JObject dbssRespObj, string retailer_id)
        {

            CherishedMSISDNCheckResponse raResp = new CherishedMSISDNCheckResponse();

            string status = String.Empty;
            int stockId = 0;
            string retailer_code = String.Empty;
            string number_category = String.Empty;
            string category_config = String.Empty;
            string[] cofigValue = null;

            try
            {
                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        try
                        {
                            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                            category_config = configuration.GetSection("AppSettings:number_category").Value;

                        }
                        catch (Exception) { throw new Exception("Key not found in appsettings"); }

                        if (category_config.Contains(","))
                        {
                            cofigValue = category_config.Split(',');
                        }
                        else
                        {
                            cofigValue = category_config.Split(' ');
                        }

                        if (dbssRespObj["data"]["attributes"]["number-category"] != null)
                        {
                            retailer_code = dbssRespObj["data"]["attributes"]["salesman-id"].ToString();
                            number_category = dbssRespObj["data"]["attributes"]["number-category"].ToString();

                            if (!String.IsNullOrEmpty(retailer_code))
                            {
                                if (retailer_code.Length < 6)
                                {
                                    char pad = '0';
                                    retailer_code = retailer_code.PadLeft(6, pad);
                                }
                            }

                            if (!String.IsNullOrEmpty(retailer_code) && !String.IsNullOrEmpty(number_category) && cofigValue.Any(x => x != number_category)) // from Web.config 
                            {
                                if (retailer_id.Equals(retailer_code))
                                {
                                    raResp.result = true;
                                    raResp.message = MessageCollection.ValidCherishedNumber;
                                }
                                else
                                {
                                    raResp.result = false;
                                    raResp.message = MessageCollection.InvalidCherishedNumber;
                                }
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber;
                            }
                            else if (!String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = MessageCollection.ValidCherishedNumber; ;
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x != number_category))
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN not tagged with this Retailer (ID: " + retailer_id + ")";
                            }
                            else
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN is not Valid.";
                            }
                        }
                        else
                        {
                            raResp.result = false;
                            raResp.message = "Invalid MSISDN Category!";
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "No Data found!";
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "No Data found!";
                }

                return raResp;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public CherishMSISDNCheckResponse CherishMSISDNReqParsing(JObject dbssRespObj, string retailer_id)
        {
            CherishMSISDNCheckResponse raResp = new CherishMSISDNCheckResponse();
            try
            {
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    category_config = configuration.GetSection("AppSettings:number_category").Value;

                }
                catch (Exception) { throw new Exception("Key not found in Web config"); }

                if (category_config.Contains(","))
                {
                    cofigValue = category_config.Split(',');
                }
                else
                {
                    cofigValue = category_config.Split(' ');
                }

                if(dbssRespObj != null)
                {
                    if (dbssRespObj["data"] != null)
                    {
                        if (dbssRespObj["data"]?["attributes"] != null)
                        {
                            if (dbssRespObj["data"]?["attributes"]?["number-category"] != null)
                            {
                                retailer_code = dbssRespObj["data"]?["attributes"]?["salesman-id"].ToString();
                                number_category = dbssRespObj["data"]?["attributes"]?["number-category"].ToString();

                                if (!String.IsNullOrEmpty(retailer_code))
                                {
                                    if (retailer_code.Length < 6)
                                    {
                                        char pad = '0';
                                        retailer_code = retailer_code.PadLeft(6, pad);
                                    }
                                }

                                if (!String.IsNullOrEmpty(retailer_code) && !String.IsNullOrEmpty(number_category) && cofigValue.Any(x => x != number_category)) // from Web.config 
                                {
                                    if (retailer_id.Equals(retailer_code))
                                    {
                                        raResp.result = true;
                                        raResp.message = MessageCollection.ValidCherishedNumber; ;
                                    }
                                    else
                                    {
                                        raResp.result = false;
                                        raResp.message = MessageCollection.InvalidCherishedNumber; ;
                                    }
                                }
                                else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                                {
                                    raResp.result = true;
                                    raResp.message = MessageCollection.ValidCherishedNumber; ;
                                }
                                else if (!String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                                {
                                    raResp.result = true;
                                    raResp.message = MessageCollection.ValidCherishedNumber;
                                }
                                else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x != number_category))
                                {
                                    raResp.result = false;
                                    raResp.message = "MSISDN not tagged with this Retailer (ID: " + retailer_id + ")";
                                }
                                else
                                {
                                    raResp.result = false;
                                    raResp.message = "MSISDN is not Valid.";
                                }
                            }
                            else
                            {
                                raResp.result = false;
                                raResp.message = "number-category is Empty!";
                            }
                        }
                        else
                        {
                            raResp.result = false;
                            raResp.message = "No Data found!";
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "No Data found!";
                    }
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }
        public RACommonResponse UnpairedMSISDNReqParsingForTOS(JObject dbssRespObj, string retailer_id)
        {
            CherishMSISDNCheckResponse raResp = new CherishMSISDNCheckResponse();
            try
            {
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;

                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                    category_config = configuration.GetSection("AppSettings:number_category").Value;
                }
                catch (Exception) { throw new Exception("Key not found in Web config"); }

                if (category_config.Contains(","))
                {
                    cofigValue = category_config.Split(',');
                }
                else
                {
                    cofigValue = category_config.Split(' ');
                }

                if (dbssRespObj?["data"]?["attributes"] != null)
                {
                    retailer_code = dbssRespObj["data"]?["attributes"]?["salesman-id"]?.ToString() ?? string.Empty;

                    number_category = dbssRespObj["data"]?["attributes"]?["number-category"]?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(retailer_code) && retailer_code.Length < 6)
                    {
                        retailer_code = retailer_code.PadLeft(6, '0');
                    }

                    if (!string.IsNullOrEmpty(number_category) && !cofigValue.Contains(number_category))
                    {
                        raResp.result = true;
                        raResp.message = MessageCollection.ValidCherishedNumber;
                    }
                    else if (cofigValue.Contains(number_category))
                    {
                        raResp.result = false;
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "Cherish validation: number-category is Empty!";
                        throw new Exception(raResp.message);
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = dbssRespObj?["data"] == null
                        ? "Cherish validation: MSISDN not found!"
                        : "Cherish validation: No Data found!";
                    throw new Exception(raResp.message);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return raResp;
        }
        public List<SubscriptionTypeReponseData> SubscripTypesReqParsing(List<object> dbssRespModel)
        {
            List<SubscriptionTypeReponseData> subscriptionTypes = new List<SubscriptionTypeReponseData>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dbssRespModel[i].ToString());
                    SubscriptionTypeReponseData raResp = new SubscriptionTypeReponseData();
                    raResp.subscription_id = (string)rss["id"];
                    raResp.subscription_name = (string)rss["attributes"]["code"];
                    subscriptionTypes.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptionTypes;
        }

        public List<SubscriptionTypeReponseDataRev> SubscripTypesReqParsingV2(List<object> dbssRespModel)
        {
            List<SubscriptionTypeReponseDataRev> subscriptionTypes = new List<SubscriptionTypeReponseDataRev>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dbssRespModel[i].ToString());
                    SubscriptionTypeReponseDataRev raResp = new SubscriptionTypeReponseDataRev();
                    raResp.subscription_id = (string)rss["id"];
                    raResp.subscription_name = (string)rss["attributes"]["code"];
                    subscriptionTypes.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return subscriptionTypes;
        }


        public List<SubscriptionTypeByIdReponseData> SubscripTypesByIdReqParsing(List<object> dbssRespModel)
        {
            List<SubscriptionTypeByIdReponseData> subscriptionTypes = new List<SubscriptionTypeByIdReponseData>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject jessonResponse = JObject.Parse(dbssRespModel[i].ToString());
                    SubscriptionTypeByIdReponseData raResp = new SubscriptionTypeByIdReponseData();
                    raResp.subscription_type_id = (string)jessonResponse["id"];
                    raResp.subscription_type_name = (string)jessonResponse["attributes"]["code"];
                    subscriptionTypes.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptionTypes;
        }
        public List<SubscriptionTypeByIdReponseDataRev> SubscripTypesByIdReqParsingRev(List<object> dbssRespModel)
        {
            List<SubscriptionTypeByIdReponseDataRev> subscriptionTypes = new List<SubscriptionTypeByIdReponseDataRev>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject jessonResponse = JObject.Parse(dbssRespModel[i].ToString());
                    SubscriptionTypeByIdReponseDataRev raResp = new SubscriptionTypeByIdReponseDataRev();
                    raResp.subscription_type_id = (string)jessonResponse["id"];
                    raResp.subscription_type_name = (string)jessonResponse["attributes"]["code"];
                    subscriptionTypes.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return subscriptionTypes;
        }

        public List<PackagesReponseData> PackagesParsing(List<object> dbssRespModel)
        {
            List<PackagesReponseData> packages = new List<PackagesReponseData>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dbssRespModel[i].ToString());

                    PackagesReponseData raResp = new PackagesReponseData();

                    if (rss["id"] != null && rss["attributes"]["code"] != null)
                    {
                        raResp.package_id = (string)rss["id"];
                        raResp.package_name = (string)rss["attributes"]["code"];
                        packages.Add(raResp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return packages;
        }

        public async Task<List<PackagesReponseDataRev>> PackagesParsingV3(List<object> dbssRespModel, string category)
        {
            List<PackagesReponseDataRev> packages = new List<PackagesReponseDataRev>();
            try
            {
                string mintAmount = await _bllCommon.GetCategoryMinAmount(category);
                
                if(dbssRespModel != null)
                {
                    for (int i = 0; i < dbssRespModel.Count; i++)
                    {
                        JObject rss = JObject.Parse(dbssRespModel[i].ToString());

                        PackagesReponseDataRev raResp = new PackagesReponseDataRev();

                        string typeProducts = !String.IsNullOrEmpty(rss["type"].ToString()) ? Convert.ToString(rss["type"]) : "";

                        if (!String.IsNullOrEmpty(typeProducts))
                        {
                            if (typeProducts.Equals("subscription-type-products"))
                            {
                               long productPrice = Convert.ToInt64((string)rss["attributes"]["price"]);
                               
                                if(productPrice >= Convert.ToInt32(mintAmount))
                                {
                                    string typeId = (string)rss["id"];

                                    if (typeId.Contains("-"))
                                    {
                                        int lastIndex = typeId.LastIndexOf('-');
                                        string productId = typeId.Substring(lastIndex + 1);

                                        for (int j = 0; j < dbssRespModel.Count; j++)
                                        {
                                            JObject rssSecond = JObject.Parse(dbssRespModel[j].ToString());
                                            raResp.package_id = (string)rssSecond["id"];
                                            if (productId.Equals(raResp.package_id))
                                            {
                                                if (rss["id"] != null && rssSecond["attributes"]["code"] != null)
                                                {
                                                    raResp.package_name = (string)rssSecond["attributes"]["code"];
                                                    packages.Add(raResp);
                                                }                                                
                                            }                                            
                                        }
                                    }                                    
                                }                               
                            } 
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return packages;
        }

        public List<PackagesReponseDataRev> PackagesParsingV2(List<object> dbssRespModel)
        {
            List<PackagesReponseDataRev> packages = new List<PackagesReponseDataRev>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dbssRespModel[i].ToString());

                    PackagesReponseDataRev raResp = new PackagesReponseDataRev();

                    if (rss["id"] != null && rss["attributes"]["code"] != null)
                    {
                        raResp.package_id = (string)rss["id"];
                        raResp.package_name = (string)rss["attributes"]["code"];
                        packages.Add(raResp);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return packages;
        }


        public OldSIMNnumberResponse OldSIMNumberParsing(SIMNumberParsingRootobject rootObj)
        {
            OldSIMNnumberResponse response = new OldSIMNnumberResponse();
            if (rootObj.data.Count <= 0)
            {
                response.result = false;
                response.message = MessageCollection.NoDataFound;
                return response;
            }

            if (rootObj.data[0].attributes == null)
            {
                response.result = false;
                response.message = MessageCollection.DataNotFound;
                return response;
            }

            if (rootObj.data[0].attributes.icc == null)
            {
                response.result = false;
                response.message = MessageCollection.DataNotFound;
                return response;
            }

            response.old_sim_number = rootObj.data[0].attributes.icc;
            response.message = MessageCollection.Success;
            return response;

        }


        public string PaymentTypeFromSubscripTypeReqParsing(JObject dbssRespModel)
        {
            try
            {
                if (!dbssRespModel["data"].HasValues)
                {
                    return String.Empty;
                    //return null;
                }

                if (!dbssRespModel["data"]["attributes"].HasValues)
                {
                    return String.Empty;
                    //return null;
                }

                if (dbssRespModel["data"]["attributes"]["payment-type"] == null)
                {
                    return String.Empty;
                    //return null;
                }

                return (string)dbssRespModel["data"]["attributes"]["payment-type"];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region TOS NID to NID
        /// <summary>
        /// in Include attribute for:- 
        ///                index 0: 
        ///                index 1: 
        ///                index 2: 
        /// </summary>
        /// <param name="dbssRespObj"></param>
        /// <returns></returns>
        /// 
        public TosNidToNidMSISDNCheckResponse TosNidToNidMSISDNReqParsingV1(JObject dbssRespObj)
        {
            TosNidToNidMSISDNCheckResponse raResp = new TosNidToNidMSISDNCheckResponse();
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    throw new Exception(MessageCollection.SIMReplNoDataFound);
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"]))
                {
                    throw new Exception("Msisdn status not found!");
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] == "terminated")
                {
                    throw new Exception("Msisdn is not valid for TOS!");
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                     && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    throw new Exception(MessageCollection.MSISDNStatusNotActiveOrIdle);
                }

                if (!dbssRespObj["included"].HasValues)
                {
                    throw new Exception("Data not found in include field!");
                }

                if (dbssRespObj["included"].Count() < 2)
                {
                    throw new Exception("Customer info or SIM cards info missing in include field!");
                }
                if (dbssRespObj["data"]["id"] == null)
                {
                    throw new Exception("Subscription ID field empty!");
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["payment-type"]))
                {
                    throw new Exception("Source customer payment type not found!");
                }

                if (dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    throw new Exception("Data not found in include field!");
                }

                //==================
                string src_sim_cards_id;
                string src_owner_customer_id;
                string src_user_customer_id;
                string src_payer_customer_id;
                string src_sim_category;
                try
                {
                    src_sim_cards_id = (string)dbssRespObj["data"]["relationships"]["sim-cards"]["data"][0]["id"];
                    src_owner_customer_id = (string)dbssRespObj["data"]["relationships"]["owner-customer"]["data"]["id"];
                    src_user_customer_id = (string)dbssRespObj["data"]["relationships"]["payer-customer"]["data"]["id"];
                    src_payer_customer_id = (string)dbssRespObj["data"]["relationships"]["user-customer"]["data"]["id"];
                    src_sim_category = (string)dbssRespObj["data"]["attributes"]["payment-type"];
                }
                catch (Exception)
                {
                    throw new Exception("Required data not found in relationships field!");
                }

                switch (dbssRespObj["included"].Count())
                {
                    case 0:
                        throw new Exception("Required data not found in include field!");

                    case 1:
                        throw new Exception("Required data not found in include field!");

                    case 2: //oc=uc=pc, sim_cards
                        if (src_owner_customer_id.Equals(src_user_customer_id)
                            && src_user_customer_id.Equals(src_payer_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = raResp.src_user_customer_id = raResp.src_payer_customer_id = src_owner_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }

                    case 3: //oc, uc=pc, sim_cards || oc=pc, uc, sim_cards || oc=uc, pc, sim_cards
                        if (!src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_owner_customer_id.Equals(src_payer_customer_id)
                            && src_user_customer_id.Equals(src_payer_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = src_owner_customer_id;
                            raResp.src_user_customer_id = raResp.src_payer_customer_id = src_user_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        //oc = pc, uc, sim_cards
                        else if (src_owner_customer_id.Equals(src_payer_customer_id)
                                    && !src_owner_customer_id.Equals(src_user_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = raResp.src_payer_customer_id = src_owner_customer_id;
                            raResp.src_user_customer_id = src_user_customer_id;


                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        //oc=uc, pc, sim_cards
                        else if (src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_user_customer_id.Equals(src_payer_customer_id))
                        {
                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = raResp.src_user_customer_id = src_owner_customer_id;
                            raResp.src_payer_customer_id = src_payer_customer_id;


                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }

                    case 4: //ow, uc, pc, sim_cards
                        if (!src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_user_customer_id.Equals(src_payer_customer_id)
                            && !src_owner_customer_id.Equals(src_payer_customer_id))
                        {
                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);
                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = src_owner_customer_id;
                            raResp.src_user_customer_id = src_user_customer_id;
                            raResp.src_payer_customer_id = src_payer_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public TosNidToNidMSISDNCheckResponseRevamp TosNidToNidMSISDNReqParsingV3(JObject dbssRespObj)
        {
            TosNidToNidMSISDNCheckResponseRevamp raResp = new TosNidToNidMSISDNCheckResponseRevamp();
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    throw new Exception(MessageCollection.SIMReplNoDataFound);
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"]))
                {
                    throw new Exception("Msisdn status not found!");
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] == "terminated")
                {
                    throw new Exception("Msisdn is not valid for TOS!");
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                     && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    throw new Exception(MessageCollection.MSISDNStatusNotActiveOrIdle);
                }

                if (!dbssRespObj["included"].HasValues)
                {
                    throw new Exception("Data not found in include field!");
                }

                if (dbssRespObj["included"].Count() < 2)
                {
                    throw new Exception("Customer info or SIM cards info missing in include field!");
                }
                if (dbssRespObj["data"]["id"] == null)
                {
                    throw new Exception("Subscription ID field empty!");
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["payment-type"]))
                {
                    throw new Exception("Source customer payment type not found!");
                }

                if (dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    throw new Exception("Data not found in include field!");
                }

                //==================
                string src_sim_cards_id;
                string src_owner_customer_id;
                string src_user_customer_id;
                string src_payer_customer_id;
                string src_sim_category;
                try
                {
                    src_sim_cards_id = (string)dbssRespObj["data"]["relationships"]["sim-cards"]["data"][0]["id"];
                    src_owner_customer_id = (string)dbssRespObj["data"]["relationships"]["owner-customer"]["data"]["id"];
                    src_user_customer_id = (string)dbssRespObj["data"]["relationships"]["payer-customer"]["data"]["id"];
                    src_payer_customer_id = (string)dbssRespObj["data"]["relationships"]["user-customer"]["data"]["id"];
                    src_sim_category = (string)dbssRespObj["data"]["attributes"]["payment-type"];
                }
                catch (Exception)
                {
                    throw new Exception("Required data not found in relationships field!");
                }

                switch (dbssRespObj["included"].Count())
                {
                    case 0:
                        throw new Exception("Required data not found in include field!");

                    case 1:
                        throw new Exception("Required data not found in include field!");

                    case 2: //oc=uc=pc, sim_cards
                        if (src_owner_customer_id.Equals(src_user_customer_id)
                            && src_user_customer_id.Equals(src_payer_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.data.dob = customerAndSimCardsInfo.dob;
                            raResp.data.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.data.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.data.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.data.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.data.src_owner_customer_id = raResp.data.src_user_customer_id = raResp.data.src_payer_customer_id = src_owner_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.isError = false;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }

                    case 3: //oc, uc=pc, sim_cards || oc=pc, uc, sim_cards || oc=uc, pc, sim_cards
                        if (!src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_owner_customer_id.Equals(src_payer_customer_id)
                            && src_user_customer_id.Equals(src_payer_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.data = new TosNidToNidMSISDNCheckResponse()
                            {
                                dob = customerAndSimCardsInfo.dob,
                                doc_id_number = customerAndSimCardsInfo.doc_id_number,
                                dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id,
                                old_sim_number = customerAndSimCardsInfo.old_sim_number,
                                old_sim_type = customerAndSimCardsInfo.old_sim_type,
                                src_owner_customer_id = src_owner_customer_id,
                                src_user_customer_id = raResp.data.src_payer_customer_id = src_user_customer_id
                            };


                            if (src_sim_category == "prepaid")
                            {
                                raResp.data.src_sim_category = (int)EnumSimCategory.Prepaid;
                            }
                            else if (src_sim_category == "postpaid")
                            {
                                raResp.data.src_sim_category = (int)EnumSimCategory.Postpaid;
                            }
                            else
                            {
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");
                            }
                            raResp.isError = false;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        //oc = pc, uc, sim_cards
                        else if (src_owner_customer_id.Equals(src_payer_customer_id)
                                    && !src_owner_customer_id.Equals(src_user_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.data.dob = customerAndSimCardsInfo.dob;
                            raResp.data.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.data.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.data.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.data.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.data.src_owner_customer_id = raResp.data.src_payer_customer_id = src_owner_customer_id;
                            raResp.data.src_user_customer_id = src_user_customer_id;


                            if (src_sim_category == "prepaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.isError = false;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        //oc=uc, pc, sim_cards
                        else if (src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_user_customer_id.Equals(src_payer_customer_id))
                        {
                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.data.dob = customerAndSimCardsInfo.dob;
                            raResp.data.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.data.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.data.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.data.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.data.src_owner_customer_id = raResp.data.src_user_customer_id = src_owner_customer_id;
                            raResp.data.src_payer_customer_id = src_payer_customer_id;


                            if (src_sim_category == "prepaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.isError = false;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }

                    case 4: //ow, uc, pc, sim_cards
                        if (!src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_user_customer_id.Equals(src_payer_customer_id)
                            && !src_owner_customer_id.Equals(src_payer_customer_id))
                        {
                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);
                            raResp.data.dob = customerAndSimCardsInfo.dob;
                            raResp.data.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.data.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.data.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.data.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.data.src_owner_customer_id = src_owner_customer_id;
                            raResp.data.src_user_customer_id = src_user_customer_id;
                            raResp.data.src_payer_customer_id = src_payer_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.data.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.isError = false;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public TOSDebtStatusResponse TOSDebtStatusCheckParse(JObject dbssRespObj)
        {
            TOSDebtStatusResponse response = new TOSDebtStatusResponse();
            response.result = false;
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    response.result = false;
                    response.message = MessageCollection.MSISDNValid;
                    return response;
                }
                int totalData = dbssRespObj["data"].Count();

                for (int i = 0; i < totalData; i++)
                {
                    if (!String.IsNullOrEmpty((string)dbssRespObj["data"][i]["attributes"]["debt"]))
                    {
                        response.debt = (decimal)dbssRespObj["data"][i]["attributes"]["debt"];

                        if (response.debt > 0)
                        {
                            response.result = true;
                            response.message = "Please pay your due bill to do the transfer of ownership, Thank you!";
                            return response;
                        }
                    }
                    response.message = MessageCollection.MSISDNValid;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public TOSLoanStatusResponse TosNiDtoNIDLoanStatusCheckParsing(JObject dbssRespObj)
        {
            TOSLoanStatusResponse raResp = new TOSLoanStatusResponse();
            string dedicatedID = string.Empty;
            string[] dedicatedArr = null;
            raResp.result = true;
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    raResp.message = MessageCollection.MSISDNValid;
                    return raResp;
                }
                int totalData = dbssRespObj["data"].Count();
                try
                {
                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
                    dedicatedID = configuration.GetSection("AppSettings:dedicated_Ac_Id_TOS").Value;
                }
                catch (Exception) { throw new Exception("Data not found in web config."); }

                if (dedicatedID.Contains(','))
                {
                    dedicatedArr = dedicatedID.Split(',');
                }
                else
                {
                    dedicatedArr = dedicatedID.Split(' ');
                }

                for (int i = 0; i < totalData; i++)
                {
                    if (!String.IsNullOrEmpty((string)dbssRespObj["data"][i]["attributes"]["dedicated-account-id"])
                                && !String.IsNullOrEmpty((string)dbssRespObj["data"][i]["attributes"]["amount"]))
                    {
                        raResp.dedicated_Ac_Id = (string)dbssRespObj["data"][i]["attributes"]["dedicated-account-id"];
                        raResp.amount = (decimal)dbssRespObj["data"][i]["attributes"]["amount"];

                        if (dedicatedArr.Any(x => x.Equals(raResp.dedicated_Ac_Id)) && raResp.amount > 0)
                        {
                            raResp.result = false;
                            raResp.message = "Customer has loan amount: " + raResp.amount.ToString() + " TK. Pls. recharge to do the TOS.";
                            return raResp;
                        }
                    }
                }
                raResp.message = MessageCollection.MSISDNValid;
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public TosNidToNidMSISDNCheckResponse ValidateCherishedMSISDNforTOS(JObject dbssRespObj, string retailer_id)
        {

            TosNidToNidMSISDNCheckResponse raResp = new TosNidToNidMSISDNCheckResponse();

            string status = String.Empty;
            int stockId = 0;
            string retailer_code = String.Empty;
            string number_category = String.Empty;
            string category_config = String.Empty;
            string[] cofigValue = null;

            try
            {
                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        try
                        {
                            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                            category_config = configuration.GetSection("AppSettings:number_category").Value;

                        }
                        catch (Exception) { throw new Exception("Key not found in Web config"); }

                        if (category_config.Contains(","))
                        {
                            cofigValue = category_config.Split(',');
                        }
                        else
                        {
                            cofigValue = category_config.Split(' ');
                        }

                        if (dbssRespObj["data"]["attributes"]["number-category"] != null)
                        {
                            retailer_code = dbssRespObj["data"]["attributes"]["salesman-id"].ToString();
                            number_category = dbssRespObj["data"]["attributes"]["number-category"].ToString();

                            if (!String.IsNullOrEmpty(retailer_code))
                            {
                                if (retailer_code.Length < 6)
                                {
                                    char pad = '0';
                                    retailer_code = retailer_code.PadLeft(6, pad);
                                }
                            }

                            if (!String.IsNullOrEmpty(retailer_code) && !String.IsNullOrEmpty(number_category) && cofigValue.Any(x => x != number_category)) // from Web.config 
                            {
                                if (retailer_id.Equals(retailer_code))
                                {
                                    raResp.result = true;
                                    raResp.message = "MSISDN is valid!";
                                }
                                else
                                {
                                    raResp.result = false;
                                    raResp.message = "Retailer is not eligible for Cherish Registration.";
                                }
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = "MSISDN is valid!.";
                            }
                            else if (!String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x == number_category))
                            {
                                raResp.result = true;
                                raResp.message = "MSISDN is valid!.";
                            }
                            else if (String.IsNullOrEmpty(retailer_code) && cofigValue.Any(x => x != number_category))
                            {
                                raResp.result = false;
                                raResp.message = "salesman-id is null.";
                            }
                            else
                            {
                                raResp.result = false;
                                raResp.message = "MSISDN not Valid.";
                            }
                        }
                        else
                        {
                            raResp.result = false;
                            raResp.message = "number-category is Empty!";
                        }
                    }
                    else
                    {
                        raResp.result = false;
                        raResp.message = "attributes is empty!";
                    }
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "data is Empty!";
                }

                return raResp;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region NEW PARSING SYSTEM AFTER FIXATION        
        public TosNidToNidMSISDNCheckResponse TosNidToNidMSISDNReqParsingV2(JObject dbssRespObj)
        {
            TosNidToNidMSISDNCheckResponse raResp = new TosNidToNidMSISDNCheckResponse();
            try
            {
                if (!dbssRespObj["data"].HasValues
                    || dbssRespObj["data"].Count() <= 0)
                {
                    throw new Exception(MessageCollection.SIMReplNoDataFound);
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"]))
                {
                    throw new Exception("Msisdn status not found!");
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] == "terminated")
                {
                    throw new Exception("Msisdn is not valid for TOS!");
                }

                if ((string)dbssRespObj["data"]["attributes"]["status"] != "active"
                     && (string)dbssRespObj["data"]["attributes"]["status"] != "idle")
                {
                    throw new Exception(MessageCollection.MSISDNStatusNotActiveOrIdle);
                }

                if (!dbssRespObj["included"].HasValues)
                {
                    throw new Exception("Data not found in include field!");
                }

                if (dbssRespObj["included"].Count() < 2)
                {
                    throw new Exception("Customer info or SIM cards info missing in include field!");
                }
                if (dbssRespObj["data"]["id"] == null)
                {
                    throw new Exception("Subscription ID field empty!");
                }

                if (String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["payment-type"]))
                {
                    throw new Exception("Source customer payment type not found!");
                }

                if (dbssRespObj["included"][0]["attributes"] == null
                    || dbssRespObj["included"][1]["attributes"] == null)
                {
                    throw new Exception("Data not found in include field!");
                }

                //==================
                string src_sim_cards_id;
                string src_owner_customer_id;
                string src_user_customer_id;
                string src_payer_customer_id;
                string src_sim_category;
                try
                {
                    src_sim_cards_id = (string)dbssRespObj["data"]["relationships"]["sim-cards"]["data"][0]["id"];
                    src_owner_customer_id = (string)dbssRespObj["data"]["relationships"]["owner-customer"]["data"]["id"];
                    src_user_customer_id = (string)dbssRespObj["data"]["relationships"]["payer-customer"]["data"]["id"];
                    src_payer_customer_id = (string)dbssRespObj["data"]["relationships"]["user-customer"]["data"]["id"];
                    src_sim_category = (string)dbssRespObj["data"]["attributes"]["payment-type"];
                }
                catch (Exception)
                {
                    throw new Exception("Required data not found in relationships field!");
                }

                switch (dbssRespObj["included"].Count())
                {
                    case 0:
                        throw new Exception("Required data not found in include field!");

                    case 1:
                        throw new Exception("Required data not found in include field!");

                    case 2: //oc=uc=pc, sim_cards
                        if (src_owner_customer_id.Equals(src_user_customer_id)
                            && src_user_customer_id.Equals(src_payer_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = raResp.src_user_customer_id = raResp.src_payer_customer_id = src_owner_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }

                    case 3: //oc, uc=pc, sim_cards || oc=pc, uc, sim_cards || oc=uc, pc, sim_cards
                        if (!src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_owner_customer_id.Equals(src_payer_customer_id)
                            && src_user_customer_id.Equals(src_payer_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = src_owner_customer_id;
                            raResp.src_user_customer_id = raResp.src_payer_customer_id = src_user_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        //oc = pc, uc, sim_cards
                        else if (src_owner_customer_id.Equals(src_payer_customer_id)
                                    && !src_owner_customer_id.Equals(src_user_customer_id))
                        {

                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = raResp.src_payer_customer_id = src_owner_customer_id;
                            raResp.src_user_customer_id = src_user_customer_id;


                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;

                        }
                        //oc=uc, pc, sim_cards
                        else if (src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_user_customer_id.Equals(src_payer_customer_id))
                        {
                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);

                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = raResp.src_user_customer_id = src_owner_customer_id;
                            raResp.src_payer_customer_id = src_payer_customer_id;


                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }


                    case 4: //ow, uc, pc, sim_cards
                        if (!src_owner_customer_id.Equals(src_user_customer_id)
                            && !src_user_customer_id.Equals(src_payer_customer_id)
                            && !src_owner_customer_id.Equals(src_payer_customer_id))
                        {
                            int ownerCustomerIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_owner_customer_id);
                            int simCardsIndex = getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(dbssRespObj["included"] as JArray, src_sim_cards_id);

                            VMOwnerCustomerAndSimCardsInfo customerAndSimCardsInfo = getOwnerCustomerandSimCardsInfo(dbssRespObj, ownerCustomerIndex, simCardsIndex);
                            raResp.dob = customerAndSimCardsInfo.dob;
                            raResp.doc_id_number = customerAndSimCardsInfo.doc_id_number;
                            raResp.dbss_subscription_id = customerAndSimCardsInfo.dbss_subscription_id;
                            raResp.old_sim_number = customerAndSimCardsInfo.old_sim_number;
                            raResp.old_sim_type = customerAndSimCardsInfo.old_sim_type;
                            raResp.src_owner_customer_id = src_owner_customer_id;
                            raResp.src_user_customer_id = src_user_customer_id;
                            raResp.src_payer_customer_id = src_payer_customer_id;

                            if (src_sim_category == "prepaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Prepaid;
                            else if (src_sim_category == "postpaid")
                                raResp.src_sim_category = (int)EnumSimCategory.Postpaid;
                            else
                                throw new Exception("Unknown source customer payment type (SIM categoty)!");

                            raResp.result = true;
                            raResp.message = MessageCollection.MSISDNValid;
                            return raResp;
                        }
                        else
                        {
                            throw new Exception("Invalid DBSS Repponse!");
                        }
                }
                return raResp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion



        private VMOwnerCustomerAndSimCardsInfo getOwnerCustomerandSimCardsInfo(JObject obj,
                                                                                int ownerCustomerPropertyIndex,
                                                                                int simCardsPropertyIndex)
        {
            try
            {
                if (String.IsNullOrEmpty((string)obj["included"][simCardsPropertyIndex]["attributes"]["icc"]))
                {
                    throw new Exception("Old SIM number not found!");
                }
                if (String.IsNullOrEmpty((string)obj["included"][simCardsPropertyIndex]["attributes"]["sim-type"]))
                {
                    throw new Exception("sim-type not found!");
                }

                if (obj["included"][ownerCustomerPropertyIndex]["attributes"]["is-company"] == null)
                {
                    throw new Exception("Company information not found!");
                }

                if (/* obj["included"][ownerCustomerPropertyIndex]["attributes"]["id-document-type"] == null
                     ||*/ String.IsNullOrEmpty((string)obj["included"][ownerCustomerPropertyIndex]["attributes"]["id-document-type"]))
                {
                    throw new Exception("id-document-type not found!");
                }

                string idDocumentType = (string)obj["included"][ownerCustomerPropertyIndex]["attributes"]["id-document-type"];

                if (idDocumentType != "national_id"
                    && idDocumentType != "smart_national_id")
                {
                    throw new Exception("Customer is not registered with National ID!");
                }
                else if ((bool)obj["included"][ownerCustomerPropertyIndex]["attributes"]["is-company"] == true)
                {
                    throw new Exception("This MSISDN is not eligible for individual SIM replacement.");
                }

                VMOwnerCustomerAndSimCardsInfo resp = new VMOwnerCustomerAndSimCardsInfo();

                resp.dob = (string)obj["included"][ownerCustomerPropertyIndex]["attributes"]["date-of-birth"];
                resp.doc_id_number = (string)obj["included"][ownerCustomerPropertyIndex]["attributes"]["id-document-number"];//

                resp.dbss_subscription_id = (int)obj["data"]["id"];
                resp.old_sim_number = (string)obj["included"][simCardsPropertyIndex]["attributes"]["icc"];
                resp.old_sim_type = (string)obj["included"][simCardsPropertyIndex]["attributes"]["sim-type"];
                return resp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        private int getIndexNumberByObjectPropertyIdFromIncludeTagForGetSubscriptionByMsisdnDbssResponse(JArray array, string id)
        {
            int index = 0;
            try
            {
                foreach (var item in array.Children())
                {
                    if (item["id"].ToString() == id)
                    {
                        break;
                    }
                    index++;
                }
                return index;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<GetImsiRespObj> GetImsiRespParsingAsync(JObject dbssRespObj)
        {
            GetImsiRespObj raResp = new GetImsiRespObj();
            string imsi = String.Empty;

            if (dbssRespObj["data"] != null)
            {
                if (dbssRespObj["data"]["attributes"] != null)
                {
                    if (dbssRespObj["data"]["attributes"]["imsi"] != null)
                    {
                        imsi = (string)dbssRespObj["data"]["attributes"]["imsi"];
                    }
                }
            }

            if (String.IsNullOrEmpty(imsi))
            {
                raResp.result = false;
                raResp.message = "IMSI not found!";
            }
            else
            {
                raResp.result = true;
                raResp.imsi = imsi;
                raResp.message = MessageCollection.Success;
            }
            return raResp;
        }

        public List<ReponseData> UnpairedMSISDNListDataParsing(List<object> dbssRespModel)
        {
            List<ReponseData> reponseList = new List<ReponseData>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dbssRespModel[i].ToString());
                    ReponseData raResp = new ReponseData();
                    raResp.msisdn = rss["attributes"]["msisdn"].ToString();
                    reponseList.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return reponseList;
        }

        public List<ReponseDataRev> UnpairedMSISDNListDataParsingV2(List<object> dbssRespModel)
        {
            List<ReponseDataRev> reponseList = new List<ReponseDataRev>();
            try
            {
                for (int i = 0; i < dbssRespModel.Count; i++)
                {


                    JObject rss = JObject.Parse(dbssRespModel[i].ToString());
                    ReponseDataRev raResp = new ReponseDataRev();
                    raResp.msisdn = rss["attributes"]?["msisdn"]?.ToString();



                    reponseList.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return reponseList;
        }

        public List<SIMReponseData> UnpairedSIMListDataParsing(List<object> dmsRespModel)
        {
            List<SIMReponseData> reponseList = new List<SIMReponseData>();
            try
            {
                for (int i = 0; i < dmsRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dmsRespModel[i].ToString());
                    SIMReponseData raResp = new SIMReponseData();
                    raResp.sim_serial = rss["sim_serial"].ToString();
                    reponseList.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return reponseList;
        }

        public List<SIMReponseDataRev> UnpairedSIMListDataParsingV2(List<object> dmsRespModel)
        {
            List<SIMReponseDataRev> reponseList = new List<SIMReponseDataRev>();
            try
            {
                for (int i = 0; i < dmsRespModel.Count; i++)
                {
                    JObject rss = JObject.Parse(dmsRespModel[i].ToString());
                    SIMReponseDataRev raResp = new SIMReponseDataRev();
                    raResp.sim_serial = rss["sim_serial"].ToString();
                    reponseList.Add(raResp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return reponseList;
        }

        public async Task<string> GetStockResponses(string channelName)
        {
            StockResponse stock = new StockResponse();
            try
            {
                DataTable data = await _dataManager.GetStockAvailable(channelName);

                if (data.Rows.Count > 0)
                {
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        stock.channelId = Convert.ToString(data.Rows[i]["CHANNELID"] == DBNull.Value ? null : data.Rows[i]["CHANNELID"]);
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }

            return stock.channelId;
        }

        #region Cherish Number Sell
        public UnpairedMSISDNCheckResponse MSISDNReqParsingCherish(JObject dbssRespObj, string retailer_id, string selectedCategory)
        {
            UnpairedMSISDNCheckResponse raResp = new UnpairedMSISDNCheckResponse();
            try
            {
                string status = String.Empty;
                int stockId = 0;
                string retailer_code = String.Empty;
                string number_category = String.Empty;
                string category_config = String.Empty;
                string[] cofigValue = null;
                string reserved_for = string.Empty;

                if (dbssRespObj["data"] != null)
                {
                    if (dbssRespObj["data"]["attributes"] != null)
                    {
                        if (!String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["status"])
                            && !String.IsNullOrEmpty((string)dbssRespObj["data"]["attributes"]["stock"]))
                        {
                            status = (string)dbssRespObj["data"]["attributes"]["status"];
                            stockId = (int)dbssRespObj["data"]["attributes"]["stock"];
                            reserved_for = (string)dbssRespObj["data"]["attributes"]["reserved-for"];
                            number_category = (string)dbssRespObj["data"]["attributes"]["number-category"];
                        }
                    }
                }
                if (selectedCategory.ToLower() != number_category.ToLower())
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.CherishCategoryMismatch;
                    return raResp;
                }
                if (stockId == 33)
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.StockIDMismatch;
                    return raResp;
                }
                if (!String.IsNullOrEmpty(reserved_for))
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNReserved;
                    return raResp;
                }
                if (status == "available")
                {
                    raResp.result = true;
                    raResp.stock_id = stockId;
                    return raResp;
                }
                else if (status == "in_use")
                {
                    raResp.result = false;
                    raResp.message = MessageCollection.MSISDNInUse;
                    return raResp;
                }
                else
                {
                    raResp.result = false;
                    raResp.message = "MSISDN is invalid.";
                    return raResp;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion
    }
}
