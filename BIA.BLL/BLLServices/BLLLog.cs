using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLLog
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLLog(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task BALogInsert(LogModel log)
        {
           await _dataManager.BALogInsert(log);
        }
        public async Task RAToDBSSLog(BIAToDBSSLog model, string requestTxt, string responseTxt)
        {

            VMBIAToDBSSLog logObj = new VMBIAToDBSSLog()
            {
                dbss_request_id = model.dbss_request_id,
                bi_token_number = model.bi_token_number,
                error_code = model.error_code,
                error_source = model.error_source,
                integration_point_from = model.integration_point_from,
                integration_point_to = model.integration_point_to,
                is_success = model.is_success,
                message = model.message,
                method_name = model.method_name,
                msisdn = model.msisdn,
                purpose_number = String.IsNullOrEmpty(model.purpose_number) ? 0 : Convert.ToInt16(model.purpose_number),
                remarks = model.remarks,
                req_blob = model.req_blob,
                req_time = model.req_time,
                res_blob = model.res_blob,
                res_time = model.res_time,
                username = model.user_id,
                server_name = Environment.MachineName
        };

            await _dataManager.RAToDBSSLog(logObj, requestTxt, responseTxt);

        }

        public async Task RaiseCoplainLog(BIAToRaiseComplainLog model)
        { 
            VMBIAToDBSSLog logObj = new VMBIAToDBSSLog()
            {
                req_blob = model.req_blob,
                req_time = model.req_time,
                res_blob = model.res_blob,
                res_time = model.res_time,
                username = model.user_id,
                complain_id = model.complaint_id,
                server_name = Environment.MachineName
            };

            await _dataManager.RaiseCoplainLog(logObj);

        }

        ///RaiseCoplainLog

        public async Task RAToDBSSLogV2(BIAToDBSSLog model, string requestTxt, string responseTxt)
        {

            VMBIAToDBSSLog logObj = new VMBIAToDBSSLog()
            {
                error_code = model.error_code,
                error_source = model.error_source,
                integration_point_from = model.integration_point_from,
                integration_point_to = model.integration_point_to,
                is_success = model.is_success,
                message = model.message,
                method_name = model.method_name,
                msisdn = model.msisdn,
                remarks = model.remarks,
                req_blob = model.req_blob,
                req_time = model.req_time,
                res_blob = model.res_blob,
                res_time = model.res_time,
                username = model.user_id,
                server_name = Environment.MachineName 
            };

            await _dataManager.RETtoBiometricLog(logObj, requestTxt, responseTxt);

        }

        public async Task<ErrorDescription> ManageException(string message, int code, string errorSource)
        {
            ErrorDescription error;
            try
            {
                DataTable dt = await _dataManager.ManageException(message, code, errorSource);
                error = ExceptionMapping(dt);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return error;
        }

        private ErrorDescription ExceptionMapping(DataTable dt)
        {
            ErrorDescription error = new ErrorDescription();
            try
            {
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    error.error_id = Convert.ToInt64(row["ERROR_ID"] == DBNull.Value ? 0 : row["ERROR_ID"]);
                    error.error_code = (row["ERROR_CODE"] == DBNull.Value ? null : row["ERROR_CODE"].ToString());
                    error.error_description = (row["ERROR_DESCRIPTION"] == DBNull.Value ? null : row["ERROR_DESCRIPTION"].ToString());
                    error.error_custom_msg = (row["ERROR_CUSTOM_MSG"] == DBNull.Value ? null : row["ERROR_CUSTOM_MSG"].ToString());
                    error.error_source = (row["ERROR_SOURCE"] == DBNull.Value ? null : row["ERROR_SOURCE"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            return error;
        }

        public string FormatMSISDN(string msisdn)
        {
            string formattedMsisdn = "";
            try
            {
                if (string.IsNullOrEmpty(msisdn)) return "";
                formattedMsisdn = msisdn.Substring(0, 2) == FixedValueCollection.MSISDNCountryCode ? msisdn
                                                            : FixedValueCollection.MSISDNCountryCode + msisdn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return formattedMsisdn;
        }
    }

    
}
