using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;

namespace BIA.BLL.BLLServices
{
    public class BLLResubmit
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLResubmit(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<ResubmitResponseModel> GetResubmitOrderInfo(ResubmitReqModel model)
        {
            ResubmitResponseModel resModel = new ResubmitResponseModel();
            try
            {
                var dataRow = await _dataManager.GetResubmitData(model);

                if (dataRow.Rows.Count > 0)
                {
                    resModel.isError = false;
                    resModel.message = MessageCollection.Success;
                    resModel.data = new ResubmitResponseModelData()
                    {
                        bi_token_number = Convert.ToString(dataRow.Rows[0]["BI_TOKEN_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["BI_TOKEN_NUMBER"]),
                        bss_request_id = Convert.ToString(dataRow.Rows[0]["BSS_REQUEST_ID"] == DBNull.Value ? null : dataRow.Rows[0]["BSS_REQUEST_ID"]),
                        customer_name = Convert.ToString(dataRow.Rows[0]["CUSTOMER_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["CUSTOMER_NAME"]),
                        purpose_number = Convert.ToString(dataRow.Rows[0]["PURPOSE_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["PURPOSE_NUMBER"]),
                        msisdn = Convert.ToString(dataRow.Rows[0]["MSISDN"] == DBNull.Value ? null : dataRow.Rows[0]["MSISDN"]),
                        dest_sim_category = Convert.ToString(dataRow.Rows[0]["DEST_SIM_CATEGORY"] == DBNull.Value ? null : dataRow.Rows[0]["DEST_SIM_CATEGORY"]),
                        dest_doc_type_no = Convert.ToString(dataRow.Rows[0]["Dest_Doc_Type_No"] == DBNull.Value ? null : dataRow.Rows[0]["Dest_Doc_Type_No"]),
                        dest_doc_id = Convert.ToString(dataRow.Rows[0]["DEST_DOC_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DEST_DOC_ID"]),
                        dest_dob = Convert.ToString(dataRow.Rows[0]["DEST_DOB"] == DBNull.Value ? null : dataRow.Rows[0]["DEST_DOB"]),
                        src_doc_id = Convert.ToString(dataRow.Rows[0]["SRC_DOC_ID"] == DBNull.Value ? null : dataRow.Rows[0]["SRC_DOC_ID"]),
                        src_doc_type_no = Convert.ToString(dataRow.Rows[0]["SRC_DOC_TYPE_NO"] == DBNull.Value ? null : dataRow.Rows[0]["SRC_DOC_TYPE_NO"]),
                        src_dob = Convert.ToString(dataRow.Rows[0]["SRC_DOB"] == DBNull.Value ? null : dataRow.Rows[0]["SRC_DOB"]),
                        platform_id = Convert.ToString(dataRow.Rows[0]["PLATFORM_ID"] == DBNull.Value ? null : dataRow.Rows[0]["PLATFORM_ID"]),
                        payment_type = Convert.ToString(dataRow.Rows[0]["PAYMENT_TYPE"] == DBNull.Value ? null : dataRow.Rows[0]["PAYMENT_TYPE"]),
                        is_paired = Convert.ToInt32(dataRow.Rows[0]["ISPAIRED"] == DBNull.Value ? null : dataRow.Rows[0]["ISPAIRED"]),
                        channel_id = Convert.ToInt32(dataRow.Rows[0]["CHANNEL_ID"] == DBNull.Value ? null : dataRow.Rows[0]["CHANNEL_ID"]),
                        sim_replc_reason = Convert.ToString(dataRow.Rows[0]["SIM_REPLC_REASON"] == DBNull.Value ? null : dataRow.Rows[0]["SIM_REPLC_REASON"]),
                        right_id = Convert.ToInt32(dataRow.Rows[0]["RIGHT_ID"] == DBNull.Value ? null : dataRow.Rows[0]["RIGHT_ID"]),
                        alt_msisdn = Convert.ToString(dataRow.Rows[0]["ALT_MSISDN"] == DBNull.Value ? null : dataRow.Rows[0]["ALT_MSISDN"]),
                        dest_sim_number = Convert.ToString(dataRow.Rows[0]["DEST_SIM_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["DEST_SIM_NUMBER"]),
                        village = Convert.ToString(dataRow.Rows[0]["VILLAGE"] == DBNull.Value ? null : dataRow.Rows[0]["VILLAGE"]),
                        gender = Convert.ToString(dataRow.Rows[0]["GENDER"] == DBNull.Value ? null : dataRow.Rows[0]["GENDER"]),
                        thana_id = Convert.ToInt32(dataRow.Rows[0]["THANA_ID"] == DBNull.Value ? null : dataRow.Rows[0]["THANA_ID"]),
                        thana_name = Convert.ToString(dataRow.Rows[0]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["THANA_NAME"]),
                        road_number = Convert.ToString(dataRow.Rows[0]["ROAD_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["ROAD_NUMBER"]),
                        flat_number = Convert.ToString(dataRow.Rows[0]["FLAT_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["FLAT_NUMBER"]),
                        district_name = Convert.ToString(dataRow.Rows[0]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["DISTRICT_NAME"]),
                        district_id = Convert.ToInt32(dataRow.Rows[0]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DISTRICT_ID"]),
                        division_id = Convert.ToInt32(dataRow.Rows[0]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[0]["DIVISION_ID"]),
                        division_name = Convert.ToString(dataRow.Rows[0]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[0]["DIVISION_NAME"]),
                        house_number = Convert.ToString(dataRow.Rows[0]["HOUSE_NUMBER"] == DBNull.Value ? null : dataRow.Rows[0]["HOUSE_NUMBER"]),
                        email = Convert.ToString(dataRow.Rows[0]["EMAIL"] == DBNull.Value ? null : dataRow.Rows[0]["EMAIL"]),
                        postal_code = Convert.ToString(dataRow.Rows[0]["POSTAL_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["POSTAL_CODE"]),
                        subscription_code = Convert.ToString(dataRow.Rows[0]["SUBSCRIPTION_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["SUBSCRIPTION_CODE"]),
                        subscription_type_id = Convert.ToInt32(dataRow.Rows[0]["SUBSCRIPTION_TYPE_ID"] == DBNull.Value ? null : dataRow.Rows[0]["SUBSCRIPTION_TYPE_ID"]),
                        package_code = Convert.ToString(dataRow.Rows[0]["PACKAGE_CODE"] == DBNull.Value ? null : dataRow.Rows[0]["PACKAGE_CODE"]),
                        package_id = Convert.ToInt32(dataRow.Rows[0]["package_id"] == DBNull.Value ? null : dataRow.Rows[0]["package_id"]),
                        is_urgent = Convert.ToInt32(dataRow.Rows[0]["IS_URGENT"] == DBNull.Value ? null : dataRow.Rows[0]["IS_URGENT"]),
                        port_in_date = Convert.ToString(dataRow.Rows[0]["PORT_IN_DATE"] == DBNull.Value ? null : dataRow.Rows[0]["PORT_IN_DATE"]),
                        order_id = Convert.ToString(dataRow.Rows[0]["ORDER_ID"] == DBNull.Value ? null : dataRow.Rows[0]["ORDER_ID"])
                    };
                }
                else
                {
                    resModel.isError = false;
                    resModel.data = null;
                    resModel.message = MessageCollection.NoDataFound;
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return resModel;
        }

    }
}
