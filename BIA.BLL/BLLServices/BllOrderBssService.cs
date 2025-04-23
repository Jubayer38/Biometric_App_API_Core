using BIA.DAL.Repositories;
using BIA.Entity.DB_Model;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BllOrderBssService
    {
        private readonly DALBiometricRepo _dataManager;

        public BllOrderBssService(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<List<OrderDataModel>> GetBssDataList(OrderListReqModel reqModel)
        {
            try
            {
                DataTable dt = await _dataManager.GetBssDataList(reqModel);
                return BindDataToModel(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<OrderDataModel> BindDataToModel(DataTable dt)
        {
            List<OrderDataModel> dataList;
            OrderDataModel bssData;
            try
            {
                dataList = new List<OrderDataModel>();
                foreach (DataRow dtRow in dt.Rows)
                {
                    bssData = new OrderDataModel();

                    bssData.bi_token_number = dtRow["BI_TOKEN_NUMBER"].ToString();
                    bssData.bss_request_id = dtRow["BSS_REQUEST_ID"].ToString();
                    bssData.purpose_number = Convert.ToInt32(dtRow["PURPOSE_NUMBER"]);
                    bssData.msisdn = dtRow["MSISDN"].ToString();
                    bssData.sim_category = Convert.ToInt32(dtRow["DEST_SIM_CATEGORY"]);
                    bssData.sim_number = dtRow["DEST_SIM_NUMBER"].ToString();
                    bssData.subscription_code = dtRow["SUBSCRIPTION_CODE"].ToString();
                    bssData.package_code = dtRow["PACKAGE_CODE"].ToString();
                    bssData.dest_doc_type_no = dtRow["DEST_DOC_TYPE_NO"].ToString();
                    bssData.dest_doc_id = dtRow["DEST_DOC_ID"].ToString();
                    bssData.dest_dob = dtRow["DEST_DOB"].ToString();
                    bssData.customer_name = dtRow["CUSTOMER_NAME"].ToString();
                    bssData.gender = dtRow["GENDER"].ToString();
                    bssData.flat_number = dtRow["FLAT_NUMBER"].ToString();
                    bssData.house_number = dtRow["HOUSE_NUMBER"].ToString();
                    bssData.road_number = dtRow["ROAD_NUMBER"].ToString();
                    bssData.village = dtRow["VILLAGE"].ToString();
                    bssData.postal_code = dtRow["POSTAL_CODE"].ToString();
                    bssData.division_Name = dtRow["DIVISION_NAME"].ToString();
                    bssData.district_Name = dtRow["DISTRICT_NAME"].ToString();
                    bssData.thana_Name = dtRow["THANA_NAME"].ToString();
                    bssData.user_id = dtRow["USER_NAME"].ToString();// change to user_name
                    if (!string.IsNullOrEmpty(dtRow["PORT_IN_DATE"].ToString()))
                        bssData.port_in_date = DateTime.Parse(dtRow["PORT_IN_DATE"].ToString()).ToString("yyyy-MM-dd HH:mm");
                    else
                        bssData.port_in_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    bssData.alt_msisdn = dtRow["ALT_MSISDN"].ToString();
                    bssData.status = Convert.ToInt32(dtRow["STATUS"]);
                    bssData.error_id = Convert.ToInt32(dtRow["ERROR_ID"]);
                    bssData.error_description = dtRow["ERROR_DESCRIPTION"].ToString();
                    string date = DateTime.Parse(dtRow["CREATE_DATE"].ToString()).ToString("yyyy-MM-dd HH:mm");
                    bssData.create_date = date;
                    bssData.dest_id_type_exp_time = dtRow["DEST_ID_TYPE_EXP_TIME"].ToString() == null ? null : dtRow["DEST_ID_TYPE_EXP_TIME"].ToString();
                    bssData.confirmation_code = dtRow["CONFIRMATION_CODE"].ToString();
                    bssData.sim_replace_reason = dtRow["SIMREPRESON"].ToString();
                    bssData.channel_name = dtRow["CHANNEL_NAME"].ToString();
                    bssData.email = dtRow["EMAIL"].ToString();
                    bssData.salesman_code = dtRow["RETAILER_CODE"].ToString();// change to retailer_code
                    bssData.is_paired = Convert.ToInt32(dtRow["ISPAIRED"]);
                    bssData.dbss_subscription_id = Convert.ToInt32(dtRow["DBSS_SUBSCRIPTION_ID"]);
                    bssData.old_sim_number = dtRow["OLD_SIM_NUMBER"].ToString();
                    bssData.sim_replacement_type = Convert.ToInt32(dtRow["SIM_REPLACEMENT_TYPE"]);
                    bssData.src_sim_category = Convert.ToInt32(dtRow["SRC_SIM_CATEGORY"]);
                    bssData.port_in_confirmation_code = dtRow["PORT_IN_CONFIRMATION_CODE"].ToString();
                    bssData.center_or_distributor_code = dtRow["CENTER_OR_DISTRIBUTOR_CODE"].ToString();
                    bssData.payment_type = dtRow["PAYMENT_TYPE"].ToString();
                    bssData.poc_number = dtRow["POC_NUMBER"].ToString();


                    dataList.Add(bssData);
                }
                return dataList;
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<bool> UpdateBioDbForOrderReq(string bi_token_no, string order_conframtion_code)
        {
            try
            {
                bool rowAffect = await _dataManager.UpdateBioDbForOrderReq(bi_token_no, order_conframtion_code);
                return rowAffect;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<bool> UpdateBioDbForCreateCustomerReq(string bi_token_no, string owner_customer_id)
        {
            try
            {
                bool rowAffect = await _dataManager.UpdateBioDbForCreateCustomerReq(bi_token_no, owner_customer_id);
                return rowAffect;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> ClearBookingFlagForOrderReq(int order_booking_flag)
        {
            try
            {
                bool rowAffect = await _dataManager.ClearBookingFlagForOrderReq(order_booking_flag);
                return rowAffect;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<string> GetBTSInfoByLacCid(int lac, int cid)
        {
            try
            {
                DataTable dt = await _dataManager.GetBTSInformationByLacCid(lac, cid);

                return BindBTSInfoModelByLacCid(dt);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string BindBTSInfoModelByLacCid(DataTable dt)
        {
            string BtsCode = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                BtsCode = dr["BTS_CODE"].ToString();
            }
            return BtsCode;
        }
    }
}
