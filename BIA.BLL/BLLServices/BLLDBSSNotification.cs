using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BIA.Entity.ResponseEntity.CherishedMSISDNCheckResponse;

namespace BIA.BLL.BLLServices
{
    public class BLLDBSSNotification
    {
        private readonly DALBiometricRepo dataManager;

        public BLLDBSSNotification(DALBiometricRepo _dataManager)
        {
            dataManager = _dataManager;
        }
        public async Task<DBSSNotificationResponse> VarificationFinishNotification(BIAFinishNotiRequest request)
        {
            DBSSNotificationResponse response = new DBSSNotificationResponse();
            try
            {
                var dataRow = await dataManager.VarificationFinishNotification(request);
                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        int status = Convert.ToInt32(dataRow.Rows[i]["STATUS"] == DBNull.Value ? null : dataRow.Rows[i]["STATUS"]);
                        int purposeNo = Convert.ToInt32(dataRow.Rows[i]["PURPOSE_NUMBER"]);

                        response.bi_token_number = Convert.ToString(dataRow.Rows[i]["BI_TOKEN_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["BI_TOKEN_NUMBER"]);
                        response.msisdn = Convert.ToString(dataRow.Rows[i]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["MSISDN"]);
                        response.bss_request_id = Convert.ToString(dataRow.Rows[i]["BSS_REQUEST_ID"] == DBNull.Value ? null : dataRow.Rows[i]["BSS_REQUEST_ID"]);
                        response.purpose_number = Convert.ToInt32(dataRow.Rows[i]["PURPOSE_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["PURPOSE_NUMBER"]);
                        response.sim_category = Convert.ToInt32(dataRow.Rows[i]["DEST_SIM_CATEGORY"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_SIM_CATEGORY"]);
                        response.sim_number = Convert.ToString(dataRow.Rows[i]["DEST_SIM_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_SIM_NUMBER"]);
                        response.subscription_code = Convert.ToString(dataRow.Rows[i]["SUBSCRIPTION_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["SUBSCRIPTION_CODE"]);
                        response.package_code = Convert.ToString(dataRow.Rows[i]["PACKAGE_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["PACKAGE_CODE"]);
                        response.dest_doc_type_no = Convert.ToString(dataRow.Rows[i]["DEST_DOC_TYPE_NO"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOC_TYPE_NO"]);
                        response.dest_doc_id = Convert.ToString(dataRow.Rows[i]["DEST_DOC_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOC_ID"]);
                        response.dest_dob = Convert.ToString(dataRow.Rows[i]["DEST_DOB"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_DOB"]);
                        response.customer_name = Convert.ToString(dataRow.Rows[i]["CUSTOMER_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["CUSTOMER_NAME"]);
                        response.gender = Convert.ToString(dataRow.Rows[i]["GENDER"] == DBNull.Value ? null : dataRow.Rows[i]["GENDER"]);
                        response.flat_number = Convert.ToString(dataRow.Rows[i]["FLAT_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["FLAT_NUMBER"]);
                        response.house_number = Convert.ToString(dataRow.Rows[i]["HOUSE_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["HOUSE_NUMBER"]);
                        response.road_number = Convert.ToString(dataRow.Rows[i]["ROAD_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["ROAD_NUMBER"]);
                        response.village = Convert.ToString(dataRow.Rows[i]["VILLAGE"] == DBNull.Value ? null : dataRow.Rows[i]["VILLAGE"]);
                        response.division_Name = Convert.ToString(dataRow.Rows[i]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_NAME"]);
                        response.district_Name = Convert.ToString(dataRow.Rows[i]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_NAME"]);
                        response.thana_Name = Convert.ToString(dataRow.Rows[i]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["THANA_NAME"]);
                        response.postal_code = Convert.ToString(dataRow.Rows[i]["POSTAL_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["POSTAL_CODE"]);
                        response.user_id = Convert.ToString(dataRow.Rows[i]["USER_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["USER_NAME"]);
                        if (!string.IsNullOrEmpty(dataRow.Rows[i]["PORT_IN_DATE"].ToString()))
                            response.port_in_date = DateTime.Parse(dataRow.Rows[i]["PORT_IN_DATE"].ToString()).ToString("yyyy-MM-dd HH:mm");
                        else
                            response.port_in_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                        response.alt_msisdn = Convert.ToString(dataRow.Rows[i]["ALT_MSISDN"] == DBNull.Value ? null : dataRow.Rows[i]["ALT_MSISDN"]);
                        response.status = Convert.ToInt32(dataRow.Rows[i]["STATUS"] == DBNull.Value ? null : dataRow.Rows[i]["STATUS"]);
                        response.error_id = Convert.ToInt32(dataRow.Rows[i]["ERROR_ID"] == DBNull.Value ? null : dataRow.Rows[i]["ERROR_ID"]);
                        response.error_description = Convert.ToString(dataRow.Rows[i]["ERROR_DESCRIPTION"] == DBNull.Value ? null : dataRow.Rows[i]["ERROR_DESCRIPTION"]);
                        response.create_date = Convert.ToString(dataRow.Rows[i]["CREATE_DATE"] == DBNull.Value ? null : dataRow.Rows[i]["CREATE_DATE"]);
                        response.dest_id_type_exp_time = Convert.ToString(dataRow.Rows[i]["DEST_ID_TYPE_EXP_TIME"] == DBNull.Value ? null : dataRow.Rows[i]["DEST_ID_TYPE_EXP_TIME"]);
                        response.confirmation_code = Convert.ToString(dataRow.Rows[i]["CONFIRMATION_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["CONFIRMATION_CODE"]);
                        response.email = Convert.ToString(dataRow.Rows[i]["EMAIL"] == DBNull.Value ? null : dataRow.Rows[i]["EMAIL"]);
                        response.salesman_code = Convert.ToString(dataRow.Rows[i]["RETAILER_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["RETAILER_CODE"]);
                        response.channel_name = Convert.ToString(dataRow.Rows[i]["CHANNEL_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["CHANNEL_NAME"]);
                        response.center_or_distributor_code = Convert.ToString(dataRow.Rows[i]["CENTER_OR_DISTRIBUTOR_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["CENTER_OR_DISTRIBUTOR_CODE"]);
                        response.sim_replace_reason = Convert.ToString(dataRow.Rows[i]["SIMREPRESON"] == DBNull.Value ? null : dataRow.Rows[i]["SIMREPRESON"]);
                        response.is_paired = Convert.ToInt32(dataRow.Rows[i]["ISPAIRED"] == DBNull.Value ? null : dataRow.Rows[i]["ISPAIRED"]);
                        response.dbss_subscription_id = Convert.ToInt32(dataRow.Rows[i]["DBSS_SUBSCRIPTION_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DBSS_SUBSCRIPTION_ID"]);
                        response.old_sim_number = Convert.ToString(dataRow.Rows[i]["OLD_SIM_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["OLD_SIM_NUMBER"]);
                        response.sim_replacement_type = Convert.ToInt32(dataRow.Rows[i]["SIM_REPLACEMENT_TYPE"] == DBNull.Value ? null : dataRow.Rows[i]["SIM_REPLACEMENT_TYPE"]);
                        response.src_sim_category = Convert.ToInt32(dataRow.Rows[i]["SRC_SIM_CATEGORY"] == DBNull.Value ? null : dataRow.Rows[i]["SRC_SIM_CATEGORY"]);
                        response.port_in_confirmation_code = Convert.ToString(dataRow.Rows[i]["PORT_IN_CONFIRMATION_CODE"] == DBNull.Value ? null : dataRow.Rows[i]["PORT_IN_CONFIRMATION_CODE"]);
                        response.payment_type = Convert.ToString(dataRow.Rows[i]["PAYMENT_TYPE"] == DBNull.Value ? null : dataRow.Rows[i]["PAYMENT_TYPE"]);
                        response.poc_number = Convert.ToString(dataRow.Rows[i]["POC_NUMBER"] == DBNull.Value ? null : dataRow.Rows[i]["POC_NUMBER"]);

                        if ((status == (int)EnumRAOrderStatus.BioVerificationSuccess
                            || status == (int)EnumRAOrderStatus.Failed)
                            && purposeNo != (int)EnumPurposeNumber.NewRegistration)
                        {
                            response.result = true;
                            response.message = MessageCollection.Success;
                            response.is_unreservation_needed = false;
                            break;
                        }
                        else if ((status == (int)EnumRAOrderStatus.BioVerificationSuccess)
                            && purposeNo == (int)EnumPurposeNumber.NewRegistration)
                        {
                            response.result = true;
                            response.message = MessageCollection.Success;
                            response.is_unreservation_needed = false;
                            break;
                        }
                        else if ((status == (int)EnumRAOrderStatus.BioVerificationSuccess
                            || status == (int)EnumRAOrderStatus.Failed)
                            && purposeNo == (int)EnumPurposeNumber.NewRegistration
                            && Convert.ToInt16(dataRow.Rows[i]["ISPAIRED"]) == 1)
                        {
                            response.result = true;
                            response.message = MessageCollection.Success;
                            response.is_unreservation_needed = false;
                            break;
                        }

                        //=====Checking for unreserve msisdn. ======
                        //(Msisdn unreservation is required only for new_connection unpaired.)
                        else if (purposeNo == (int)EnumPurposeNumber.NewRegistration
                            && Convert.ToInt16(dataRow.Rows[i]["ISPAIRED"]) == 0
                            && status == (int)EnumRAOrderStatus.Failed)
                        {
                            if (dataRow.Rows[i]["MSISDNRESERVATIONID"] == DBNull.Value)
                            {
                                response.result = false;
                                response.message = "No msisdn reservation id found against the bio request id" + request.bio_request_id + " to unreserve msisdn!";
                                response.is_unreservation_needed = false;
                                break;
                            }

                            response.is_unreservation_needed = true;
                            response.msisdn_reservation_id = Convert.ToString(dataRow.Rows[i]["MSISDNRESERVATIONID"] == DBNull.Value ? null : dataRow.Rows[i]["MSISDNRESERVATIONID"]);
                            response.result = true;
                            response.message = MessageCollection.Success;
                            break;
                        }
                    }
                }
                else
                {
                    response.result = false;
                    response.message = "No bio request id " + request.bio_request_id + " found or the order is not in 'Biometric verification submitted' state! Status update failed!";
                    response.is_unreservation_needed = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<VMErrorMsg> GetCustomErrorMsg(decimal errorId)
        {
            VMErrorMsg errorMsgObj = new VMErrorMsg();
            try
            {
                var dataRow = await dataManager.GetCustomErrorMsg(errorId);

                if (dataRow.Rows.Count > 0)
                {
                    errorMsgObj.error_msg = Convert.ToString(dataRow.Rows[0]["ERROR_MSG"] == DBNull.Value ? null : dataRow.Rows[0]["ERROR_MSG"]);
                    errorMsgObj.error_code = Convert.ToString(dataRow.Rows[0]["ERROR_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["ERROR_CODE"]);

                }
                return errorMsgObj;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
