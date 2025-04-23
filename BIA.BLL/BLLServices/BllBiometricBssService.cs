using BIA.DAL.Repositories;
using BIA.Entity.DB_Model;
using BIA.Entity.ResponseEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BllBiometricBssService 
    {
        private readonly DALBiometricRepo dalObj;

        public BllBiometricBssService(DALBiometricRepo _dalObj)
        {
            dalObj = _dalObj;
        }
        public List<BiomerticDataModel> BindDataToModel(DataTable dt)
        {
            List<BiomerticDataModel> dataList;
            BiomerticDataModel bssData;
            try
            {
                dataList = new List<BiomerticDataModel>();
                foreach (DataRow dtRow in dt.Rows)
                {
                    bssData = new BiomerticDataModel();

                    bssData.bi_token_number = dtRow["BI_TOKEN_NUMBER"].ToString();
                    bssData.bss_request_id = dtRow["BSS_REQUEST_ID"].ToString();
                    bssData.purpose_number = Convert.ToInt32(dtRow["PURPOSE_NUMBER"]);
                    bssData.msisdn = dtRow["MSISDN"].ToString();
                    bssData.sim_category = Convert.ToInt32(dtRow["DEST_SIM_CATEGORY"]);
                    bssData.sim_number = dtRow["DEST_SIM_NUMBER"].ToString();
                    bssData.dest_doc_type_no = dtRow["DEST_DOC_TYPE_NO"].ToString();
                    bssData.dest_doc_id = dtRow["DEST_DOC_ID"].ToString();
                    bssData.dest_dob = dtRow["DEST_DOB"].ToString();
                    bssData.src_doc_type_no = dtRow["SRC_DOC_TYPE_NO"].ToString();
                    bssData.src_doc_id = dtRow["SRC_DOC_ID"].ToString();
                    bssData.src_dob = dtRow["SRC_DOB"].ToString();
                    try
                    {
                        bssData.dest_left_thumb = (byte[])dtRow["DEST_LEFT_THUMB"];
                    }
                    catch { }
                    try
                    {
                        bssData.dest_left_index = (byte[])dtRow["DEST_LEFT_INDEX"];
                    }
                    catch { }
                    try
                    {
                        bssData.dest_right_thumb = (byte[])dtRow["DEST_RIGHT_THUMB"];
                    }
                    catch { }
                    try
                    {
                        bssData.dest_right_index = (byte[])dtRow["DEST_RIGHT_INDEX"];
                    }
                    catch { }

                    try
                    {
                        bssData.src_left_thumb = (byte[])dtRow["SRC_LEFT_THUMB"];
                    }
                    catch { }
                    try
                    {
                        bssData.src_left_index = (byte[])dtRow["SRC_LEFT_INDEX"];
                    }
                    catch { }
                    try
                    {
                        bssData.src_right_thumb = (byte[])dtRow["SRC_RIGHT_THUMB"];
                    }
                    catch { }
                    try
                    {
                        bssData.src_right_index = (byte[])dtRow["SRC_RIGHT_INDEX"];
                    }
                    catch { }
                    bssData.user_id = dtRow["USER_NAME"].ToString();// change to user_name
                    bssData.poc_number = dtRow["POC_NUMBER"].ToString();
                    bssData.status = Convert.ToInt32(dtRow["STATUS"]);
                    bssData.error_id = Convert.ToInt32(dtRow["ERROR_ID"]);
                    bssData.error_description = dtRow["ERROR_DESCRIPTION"].ToString();
                    string date = DateTime.Parse(dtRow["CREATE_DATE"].ToString()).ToString("yyyy-MM-dd HH:mm");
                    bssData.create_date = date;
                    bssData.dest_imsi = dtRow["DEST_IMSI"].ToString();
                    bssData.dest_id_type_exp_time = dtRow["DEST_ID_TYPE_EXP_TIME"].ToString() == null ? null : dtRow["DEST_ID_TYPE_EXP_TIME"].ToString();
                    bssData.src_id_type_exp_time = dtRow["SRC_ID_TYPE_EXP_TIME"].ToString() == null ? null : dtRow["SRC_ID_TYPE_EXP_TIME"].ToString();
                    bssData.is_paired = Convert.ToInt32(dtRow["ISPAIRED"]);
                    //bssData.msisdn_reservation_id = dtRow["MSISDNRESERVATIONID"].ToString();
                    bssData.dest_ec_verification_required = Convert.ToInt32(dtRow["DEST_EC_VERIFICATION_REQUIRED"]);
                    bssData.src_ec_verification_required = Convert.ToInt32(dtRow["SRC_EC_VERIFICATION_REQUIRED"]);
                    bssData.dest_foreign_flag = Convert.ToInt32(dtRow["DEST_FOREIGN_FLAG"]);
                    bssData.sim_replacement_type = Convert.ToInt32(dtRow["SIM_REPLACEMENT_TYPE"]);
                    bssData.src_sim_category = Convert.ToInt32(dtRow["SRC_SIM_CATEGORY"]);

                    try
                    {
                        bssData.otp_no = dtRow["OTP_NO"].ToString();
                    }
                    catch { }

                    dataList.Add(bssData);
                }
                return dataList;
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> UpdateBioDbForReservation(string bi_token_no, string msisdn_reservation_id)
        {
            try
            {
                //dalObj = new DALBiometricRepo();
                bool rowAffect = await dalObj.UpdateBioDbForReservation(bi_token_no, msisdn_reservation_id);
                return rowAffect;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UpdateStatusandErrorMessage(string bi_token, int status, long error_id, string error_description)
        {
            try
            {
                bool rowAffect = await dalObj.UpdateStatusandErrorMessage(bi_token, status, error_id, error_description);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //===========================Single Source Check==========================
        public async Task<SingleSourceCheckResponseModel> SingleSourceCheckFromBioDB(string msisdn, string sim_number, int purpose_No, string poc_number, int sim_rep_type, string dest_doc_id, string dest_dob, string dest_imsi)
        {
            SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();
            try
            {
                //dalObj = new DALBiometricRepo();
                checkResponseModel = await dalObj.SingleSourceCheckFromBioDB(msisdn, sim_number, purpose_No, poc_number, sim_rep_type, dest_doc_id, dest_dob, dest_imsi);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return checkResponseModel;
        }

    }
}
