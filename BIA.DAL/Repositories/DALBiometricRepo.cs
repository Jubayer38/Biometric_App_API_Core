using BIA.DAL.DBManager;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using BIA.Entity.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Reflection;
using System.Reflection.Metadata;

namespace BIA.DAL.Repositories
{
    public class DALBiometricRepo
    {
        private readonly LogWriter _logWriter;
        private readonly OracleDataManagerV2 _oracleDataManagerV2;
        private readonly IConfiguration _configuration;

        public DALBiometricRepo(LogWriter logWriter, OracleDataManagerV2 oracleDataManagerV2, IConfiguration configuration)
        {
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _oracleDataManagerV2 = oracleDataManagerV2;
            _configuration = configuration;
        }
        #region ===================| Reservation Part |==================
        public async Task<bool> UpdateBioDbForReservation(string bi_token_no, string msisdn_reservation_id)
        {
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_BI_TOKEN_NO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bi_token_no },
            new OracleParameter("P_MSISDN_RESERVATION_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = msisdn_reservation_id }
                };

                bool result = await _oracleDataManagerV2.CallUpdateProcedure("BSS_UPDFORRESERVATION", parameters);
                return result;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "UpdateBioDbForReservation",
                    procedure_name = "BSS_UPDFORRESERVATION",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
        }

        #endregion

        #region ===================| Error Message Part |================      

        public async Task<bool> UpdateStatusandErrorMessage(string bi_token, int status, long error_id, string error_description)
        {
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_BI_TOKEN_NO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bi_token },
            new OracleParameter("P_STATUS", OracleDbType.Int32, ParameterDirection.Input) { Value = status },
            new OracleParameter("P_ERROR_ID", OracleDbType.Int64, ParameterDirection.Input) { Value = error_id },
            new OracleParameter("P_ERROR_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = error_description }
                };

                bool rowAffected = await _oracleDataManagerV2.CallUpdateProcedure("BSS_UPDSTATUSANDERROREMESSES", parameters);
                return rowAffected;
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "UpdateStatusandErrorMessage",
                    procedure_name = "BSS_UPDSTATUSANDERROREMESSES",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);
                throw new Exception("Error in UpdateStatusandErrorMessage: " + ex.Message, ex);
            }
        }

        #endregion

        #region ===================| Single Source Part |================       

        public async Task<SingleSourceCheckResponseModel> SingleSourceCheckFromBioDB(string msisdn, string sim_number, int purpose_No, string poc_number,int sim_rep_type, string dest_doc_id, string dest_dob, string dest_imsi)
        {
            DataTable dt = new DataTable();
            SingleSourceCheckResponseModel checkResponseModel = new SingleSourceCheckResponseModel();

            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = msisdn },
            new OracleParameter("P_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = sim_number },
            new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Int32, ParameterDirection.Input) { Value = purpose_No },
            new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = poc_number },
            new OracleParameter("P_SIM_REP_TYPE", OracleDbType.Int32, ParameterDirection.Input) { Value = sim_rep_type },
            new OracleParameter("P_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = dest_doc_id },
            new OracleParameter("P_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = dest_dob },
            new OracleParameter("P_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = dest_imsi },
            new OracleParameter("P_SINGLESOURCE", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                dt = await _oracleDataManagerV2.SelectProcedure("BSS_CHECKSINGLESOURCE", parameters);

                if (dt.Rows.Count > 0)
                {
                    var item = dt.Rows[0];
                    checkResponseModel.Status = Convert.ToInt16(item["STATUS"]);
                    checkResponseModel.Message = item["MESSAGE"].ToString();
                }
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "SingleSourceCheckFromBioDB",
                    procedure_name = "BSS_CHECKSINGLESOURCE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);
                throw new Exception("Error in SingleSourceCheckFromBioDB: " + ex.Message, ex);
            }

            return checkResponseModel;
        }

        #endregion

        #region ===================| Common Part |=======================
        public async Task<object> IsStockAvailable(int stock_id, int channel_id)
        {
            object data = null;

            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_STOCK_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = stock_id },
            new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = channel_id },
            //new OracleParameter("PO_IS_STOCK_AVAILABLE", OracleDbType.Decimal, ParameterDirection.Output)
                };

                data = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("BIA_CHECKSTOCKCHANNELMAPPING", "PO_IS_STOCK_AVAILABLE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsStockAvailable",
                    procedure_name = "BIA_CHECKSTOCKCHANNELMAPPING",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);
                throw new Exception("Error in IsStockAvailable: " + ex.Message, ex);
            }

            return data;
        }

        public async Task<DataTable> GetActivityLogData(int activity_type_id, string user_id)
        {
            DataTable result = new DataTable();
            try
            {
                // Adding parameters to be used in the stored procedure call
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = user_id },
            new OracleParameter("P_ORDER_ACTIVITY_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = activity_type_id },
            new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                result = await _oracleDataManagerV2.SelectProcedure("GETACTIVITYLOGDATA_1", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetActivityLogData",
                    procedure_name = "GETACTIVITYLOGDATA_1",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                // Logging the error
                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetActivityLogDataV2(int activity_type_id, string user_id)
        {
            DataTable result = new DataTable();
            try
            {
                // Adding parameters to be used in the stored procedure call
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = user_id },
            new OracleParameter("P_ORDER_ACTIVITY_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = activity_type_id },
            new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                result = await _oracleDataManagerV2.SelectProcedure("GETACTIVITYLOGDATAV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetActivityLogDataV2",
                    procedure_name = "GETACTIVITYLOGDATAV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                // Logging the error
                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetActivityLogDataV3(int activity_type_id, string user_id)
        {
            int isFtrFeatureOn = Convert.ToInt32(_configuration.GetSection("AppSettings:isFtrFeatureOn").Value);

            DataTable result = null;
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = user_id },
            new OracleParameter("P_ORDER_ACTIVITY_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = activity_type_id },
            new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                string storedProcedureName = isFtrFeatureOn == 1 ? "GETACTIVITYLOGDATAV5" : "GETACTIVITYLOGDATAV6";

                result = await _oracleDataManagerV2.SelectProcedure(storedProcedureName, parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetActivityLogDataV3",
                    procedure_name = isFtrFeatureOn == 1 ? "GETACTIVITYLOGDATAV5" : "GETACTIVITYLOGDATAV6",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<DataTable> GetPurposeNumbers(RAGetPurposeRequest model)
        {
            DataTable result = null;
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_CASEID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.case_id },
            new OracleParameter("PO_PURS", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETB2BPURPOSES", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetPurposeNumbers",
                    procedure_name = "BIA_GETB2BPURPOSES",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<long> GetTokenNo(string msisdn)
        {
            long apiVersion = 0; 
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = msisdn }
                };

                var result = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("BI_GETTOKENNO", "PO_TOKENNO", parameters.ToArray());

                if (result != DBNull.Value && result != null)
                {
                    apiVersion = Convert.ToInt64(result);
                }
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetTokenNoAsync",
                    procedure_name = "BI_GETTOKENNO",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }

            return apiVersion;
        }

        #endregion

        #region ===================| Notification Part |==================       
         
        public async Task<DataTable> VarificationFinishNotification(BIAFinishNotiRequest model)
        {
            DataTable msisdnReservationIdList = new DataTable();
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_BSS_REQUEST_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bio_request_id },
            new OracleParameter("P_IS_SUCCESS", OracleDbType.Decimal, ParameterDirection.Input) { Value = Convert.ToDecimal(model.is_Success) },
            new OracleParameter("P_ERROR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_code },
            new OracleParameter("P_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.description },
            new OracleParameter("P_ERROR_SOURCE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_source },
            new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                msisdnReservationIdList = await _oracleDataManagerV2.SelectProcedure("BIA_UPDVARIFICATIONBISTATUSV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "VarificationFinishNotificationAsync",
                    procedure_name = "BIA_UPDVARIFICATIONBISTATUSV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw;
            }
            return msisdnReservationIdList; 
        }

        public async Task<DataTable> GetCustomErrorMsg(decimal errorId)
        {
            DataTable errorMessageData = new DataTable();
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_ERRORID", OracleDbType.Decimal, ParameterDirection.Input) { Value = errorId },
            new OracleParameter("PO_ERROR_MSG", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                errorMessageData = await _oracleDataManagerV2.SelectProcedure("BIA_GTECUSTOMERRORMSG", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetCustomErrorMsg",
                    procedure_name = "BIA_GTECUSTOMERRORMSG",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw;
            }
            return errorMessageData;
        }
        #endregion

        #region ===================| Division Thana Area |=================

        public async Task<DataTable> GetDivision()
        {
            DataTable result = new DataTable();
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("PO_DIVS", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                result = await _oracleDataManagerV2.SelectProcedure("GETDIVISIONS", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetDivisionAsync",
                    procedure_name = "GETDIVISIONS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetDistrict()
        {
            DataTable result = new DataTable();
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("PO_DISS", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                result = await _oracleDataManagerV2.SelectProcedure("GETDISTRICTS", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetDistrict",
                    procedure_name = "GETDISTRICTS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetThana()
        {
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("PO_THA", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                return await _oracleDataManagerV2.SelectProcedure("GETTHANA", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetThana",
                    procedure_name = "GETTHANA",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);
                throw;
            }
        }

        public async Task<DataTable> GetDivDisThana()
        {
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };

                return await _oracleDataManagerV2.SelectProcedure("GETDIVDISTHANA", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetDivDisThanaAsync",
                    procedure_name = "GETDIVDISTHANA",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region ===================| Log Area |=======================
        public async Task RAToDBSSLog(VMBIAToDBSSLog model, string requestTxt, string responseTxt)
        {
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bi_token_number },
            new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
            new OracleParameter("P_BSS_REQUEST_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dbss_request_id },
            new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number },
            new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username },
            new OracleParameter("P_REQ_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.req_blob },
            new OracleParameter("P_RES_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.res_blob },
            new OracleParameter("P_REQ_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = model.req_time },
            new OracleParameter("P_RES_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = model.res_time },
            new OracleParameter("P_IS_SUCCESS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_success },
            new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.message },
            new OracleParameter("P_ERROR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_code },
            new OracleParameter("P_ERROR_SOURCE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_source },
            new OracleParameter("P_METHOD_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.method_name },
            new OracleParameter("P_INTEGRATION_POINT_FROM", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.integration_point_from },
            new OracleParameter("P_INTEGRATION_POINT_TO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.integration_point_to },
            new OracleParameter("P_REMARKS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.remarks },
            new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name }
                };

                long result = await _oracleDataManagerV2.CallInsertProcedure("BIA_LOGBIATODBSSV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                var errorLogObj = new VMErrorLog()
                {
                    bi_token_number = model.bi_token_number,
                    error_code = model.error_code,
                    error_source = model.error_source,
                    integration_point_from = model.integration_point_from,
                    integration_point_to = model.integration_point_to,
                    is_success = model.is_success,
                    method_name = model.method_name,
                    message = model.message,
                    msisdn = model.msisdn,
                    purpose_number = Convert.ToString(model.purpose_number),
                    remarks = model.remarks,
                    user_id = model.username,
                    req_txt = requestTxt,
                    req_time = model.req_time,
                    res_txt = responseTxt,
                    res_time = model.res_time
                };

                string logTxt = JsonConvert.SerializeObject(errorLogObj) + "//DBErrorMsg: " + ex.Message + "//DBErrorSrc: " + ex.Source;
                _logWriter.WriteDailyLog2(logTxt);
                throw;
            }
        }

        public async Task RaiseCoplainLog(VMBIAToDBSSLog model)
        {
            try
            {
                var parameters = new OracleParameter[]
                {
            new OracleParameter("P_COMPLAIN_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.complain_id },
            new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username },
            new OracleParameter("P_REQ_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.req_blob },
            new OracleParameter("P_RES_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.res_blob },
            new OracleParameter("P_REQ_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = model.req_time },
            new OracleParameter("P_RES_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = model.res_time },
            new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name }
                };

                var result = await _oracleDataManagerV2.CallInsertProcedure("BIA_COMPLAIN_LOG", parameters.ToArray());

            }
            catch (Exception ex)
            {
                VMErrorLog errorLogObj = new VMErrorLog()
                {
                    bi_token_number = model.bi_token_number,
                    error_code = model.error_code,
                    error_source = model.error_source,
                    integration_point_from = model.integration_point_from,
                    integration_point_to = model.integration_point_to,
                    is_success = model.is_success,
                    method_name = model.method_name,
                    message = model.message,
                    msisdn = model.msisdn,
                    purpose_number = Convert.ToString(model.purpose_number),
                    remarks = model.remarks,
                    user_id = model.username,
                    req_time = model.req_time,
                    res_time = model.res_time
                };
                string logTxt = JsonConvert.SerializeObject(errorLogObj) + "//DBErrorMsg: " + ex.Message + "//DBErrorSrc: " + ex.Source;

                _logWriter.WriteDailyLog2(logTxt);
                throw;
            }
        }

        public async Task RETtoBiometricLog(VMBIAToDBSSLog model, string requestTxt, string responseTxt)
        {
            try
            {
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
            new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username },
            new OracleParameter("P_REQ_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.req_blob },
            new OracleParameter("P_RES_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.res_blob },
            new OracleParameter("P_REQ_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = model.req_time },
            new OracleParameter("P_RES_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = model.res_time },
            new OracleParameter("P_IS_SUCCESS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_success },
            new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.message },
            new OracleParameter("P_ERROR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_code },
            new OracleParameter("P_ERROR_SOURCE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_source },
            new OracleParameter("P_METHOD_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.method_name },
            new OracleParameter("P_REMARKS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.remarks },
            new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name }
        };

                var result = await _oracleDataManagerV2.CallInsertProcedure("RET_STATUSUPDLOG", parameters.ToArray());
            }
            catch (Exception ex)
            {
                VMErrorLog errorLogObj = new VMErrorLog()
                {
                    bi_token_number = model.bi_token_number,
                    error_code = model.error_code,
                    error_source = model.error_source,
                    integration_point_from = model.integration_point_from,
                    integration_point_to = model.integration_point_to,
                    is_success = model.is_success,
                    method_name = model.method_name,
                    message = model.message,
                    msisdn = model.msisdn,
                    remarks = model.remarks,
                    user_id = model.username,
                    req_txt = requestTxt,
                    req_time = model.req_time,
                    res_txt = responseTxt,
                    res_time = model.res_time
                };
                string logTxt = JsonConvert.SerializeObject(errorLogObj) + "//DBErrorMsg: " + ex.Message + "//DBErrorSrc: " + ex.Source;

                _logWriter.WriteDailyLog2(logTxt);
                throw;
            }
        }

        public async Task<DataTable> ManageException(string message, int code, string errorSource)
        {
            DataTable dt = null;
            try
            {
                var parameters = new List<OracleParameter>
        {
            new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = message },
            new OracleParameter("P_CODE", OracleDbType.Int32, ParameterDirection.Input) { Value = code },
            new OracleParameter("P_ERROR_SOURCE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = errorSource },
            new OracleParameter("po_Cursor", OracleDbType.RefCursor, ParameterDirection.Output)
        };

                dt = await _oracleDataManagerV2.SelectProcedure("MANAGEEXCEPTION_BAMODULE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string logText = JsonConvert.SerializeObject(new
                {
                    request_time = DateTime.Now,
                    method_name = nameof(ManageException),
                    procedure_name = "MANAGEEXCEPTION_BAMODULE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });

                _logWriter.WriteDailyLog2(logText);
                throw;
            }
            return dt;
        }

        public async Task BALogInsert(LogModel log)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
        {
            new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.bi_token_number },
            new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.msisdn },
            new OracleParameter("P_BSS_REQUEST_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.bss_request_id },
            new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = log.purpose_number },
            new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.user_id },
            new OracleParameter("P_REQ_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = log.req_blob },
            new OracleParameter("P_RES_BLOB", OracleDbType.Blob, ParameterDirection.Input) { Value = log.res_blob },
            new OracleParameter("P_REQ_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = log.req_time },
            new OracleParameter("P_RES_TIME", OracleDbType.Date, ParameterDirection.Input) { Value = log.res_time },
            new OracleParameter("P_IS_SUCCESS", OracleDbType.Decimal, ParameterDirection.Input) { Value = log.is_success },
            new OracleParameter("P_MESSAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.message },
            new OracleParameter("P_ERROR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.error_code },
            new OracleParameter("P_ERROR_SOURCE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.error_source },
            new OracleParameter("P_METHOD_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.method_name },
            new OracleParameter("P_INTEGRATION_POINT_FROM", OracleDbType.Decimal, ParameterDirection.Input) { Value = log.integration_point_from },
            new OracleParameter("P_INTEGRATION_POINT_TO", OracleDbType.Decimal, ParameterDirection.Input) { Value = log.integration_point_to },
            new OracleParameter("P_REMARKS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = log.remarks },
            new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = Environment.MachineName }
        };

                long result = await _oracleDataManagerV2.CallInsertProcedure("BIA_LOGBIATODBSSV2", parameters.ToArray());

            }
            catch (Exception ex)
            {
                VMErrorLog errorLogObj = new VMErrorLog()
                {
                    bi_token_number = log.bi_token_number,
                    error_code = log.error_code,
                    error_source = log.error_source,
                    integration_point_from = log.integration_point_from,
                    integration_point_to = log.integration_point_to,
                    is_success = log.is_success,
                    method_name = log.method_name,
                    message = log.message,
                    msisdn = log.msisdn,
                    remarks = log.remarks,
                    user_id = log.user_id,
                    req_txt = log.req_string,
                    req_time = log.req_time,
                    res_txt = log.res_string,
                    res_time = log.res_time
                };

                string logTxt = JsonConvert.SerializeObject(errorLogObj) + "//DBErrorMsg: " + ex.Message + "//DBErrorSrc: " + ex.Source;
                _logWriter.WriteDailyLog2(logTxt);
                throw;
            }
        }
        #endregion

        #region ===================| Order Req Log Area |=================
        /// <summary>
        /// FP as byte[]
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<decimal> SubmitOrder2(OrderRequest2 model)
        {
            decimal BIAReqsTokenId;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },

                };
                var result = await _oracleDataManagerV2.CallInsertProcedure("SUBMITORDER5", parameters.ToArray());

                BIAReqsTokenId = Convert.ToDecimal(result);
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SubmitOrder2",
                    procedure_name = "SUBMITORDER5",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());

            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SubmitOrder2",
                    procedure_name = "SUBMITORDER5",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return BIAReqsTokenId;
        }

        public async Task<decimal> SubmitOrderV3(OrderRequest3 model, string loginProviderId)
        {
            decimal BIAReqsTokenId;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                };
                
                var result = await _oracleDataManagerV2.CallInsertProcedure("SUBMITORDER6", parameters.ToArray());

                BIAReqsTokenId = Convert.ToDecimal(result);
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SubmitOrderV3",
                    procedure_name = "SUBMITORDER6",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SubmitOrderV3",
                    procedure_name = "SUBMITORDER6",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return BIAReqsTokenId;
        }

        public async Task<decimal> SubmitOrderV4(OrderRequest3 model, string loginProviderId)
        {
            //_oracleDataManager = new OracleDataManager();
            decimal BIAReqsTokenId;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                new OracleParameter("P_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                };
                var result = await _oracleDataManagerV2.CallInsertProcedure("SUBMITORDER7", parameters.ToArray());

                BIAReqsTokenId = Convert.ToDecimal(result);
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    retailer_id = model.retailer_id,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV4",
                    procedure_name = "SUBMITORDER7",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    retailer_id = model.retailer_id,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV4",
                    procedure_name = "SUBMITORDER7",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return BIAReqsTokenId;
        }

        public async Task<decimal> SubmitOrderV5(OrderRequest3 model, string loginProviderId)
        {
            //_oracleDataManager = new OracleDataManager();
            decimal BIAReqsTokenId;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                   new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                   new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                   new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                   new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                   new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                   new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                   new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                   new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                   new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                   new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                   new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                   new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                   new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                   new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                   new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                   new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                   new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                   new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                   new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                   new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                   new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                   new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                   new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                   new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                   new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                   new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                   new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                   new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                   new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                   new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                   new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                   new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                   new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                   new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                   new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                   new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                   new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                   new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                   new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                   new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                   new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                   new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                   new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                   new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                   new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                   new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                   new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                   new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                   new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                   new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                   new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                   new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                   new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                   new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                   new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                   new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                   new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                   new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                   new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                   new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                   new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                   new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                   new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                   new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                   new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                   new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                   new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                   new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                   new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                   new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                   new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                   new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                   new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                   new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                   new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                   new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                   new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                   new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                   new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                   new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                   new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                   new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                   new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                   new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                   new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                   new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                   new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                   new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                   new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                   new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                   new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                   new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                   new OracleParameter("P_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                }; 
                var result = await _oracleDataManagerV2.CallInsertProcedure("SUBMITORDER7", parameters.ToArray());

                BIAReqsTokenId = Convert.ToDecimal(result);
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    retailer_id = model.retailer_id,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV5",
                    procedure_name = "SUBMITORDER7",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    retailer_id = model.retailer_id,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV5",
                    procedure_name = "SUBMITORDER7",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return BIAReqsTokenId;
        }

        public async Task<DataTable> SubmitOrderRegistrationReq(OrderRequest3 model, string loginProviderId, int isregrequest)
        {
            DataTable orderResponse = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                    new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                    new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                    new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                    new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                    new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                    new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                    new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                    new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                    new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                    new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                    new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                    new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                    new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                    new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                    new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                    new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                    new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                    new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                    new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                    new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                    new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                    new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                    new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                    new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                    new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                    new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                    new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                    new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                    new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                    new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                    new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                    new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                    new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                    new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                    new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                    new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                    new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                    new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                    new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                    new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                    new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                    new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                    new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                    new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                    new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                    new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                    new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                    new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                    new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                    new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                    new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                    new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                    new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                    new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                    new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                    new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                    new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                    new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                    new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                    new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                    new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                    new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                    new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                    new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                    new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                    new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                    new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                    new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                    new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                    new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                    new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                    new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                    new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                    new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                    new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                    new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                    new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                    new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                    new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                    new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                    new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                    new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                    new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                    new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                    new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                    new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                    new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                    new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                    new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                    new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                    new OracleParameter("P_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                    new OracleParameter("P_ORDER_BOOKING_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.order_booking_flag },
                    new OracleParameter("P_IS_ESIM", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_esim },
                    new OracleParameter("P_IS_REGREQUEST", OracleDbType.Decimal, ParameterDirection.Input) { Value = isregrequest },
                };
                orderResponse = await _oracleDataManagerV2.SelectProcedureV2("SUBMITORDER14", parameters.ToArray());

            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV6",
                    procedure_name = "SUBMITORDER12",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());

            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderRegistrationReq",
                    procedure_name = "SUBMITORDER15",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return orderResponse;
        }


        public async Task<DataTable> SubmitOrderV6(OrderRequest3 model, string loginProviderId)
        {
            OracleDataManager _odm = new OracleDataManager();

            DataTable orderResponse = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                new OracleParameter("P_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                new OracleParameter("P_ORDER_BOOKING_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.order_booking_flag },
                new OracleParameter("P_IS_ESIM", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_esim },
                };

                orderResponse = await _oracleDataManagerV2.SelectProcedureV2("SUBMITORDER12", parameters.ToArray());
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV6",
                    procedure_name = "SUBMITORDER12",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());

            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV6",
                    procedure_name = "SUBMITORDER12",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return orderResponse;
        }

        public async Task<DataTable> SubmitOrderV7(OrderRequest3 model, string loginProviderId)
        {
            DataTable orderResponse = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                new OracleParameter("P_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                new OracleParameter("P_ORDER_BOOKING_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.order_booking_flag },
                new OracleParameter("P_IS_ESIM", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_esim },
                new OracleParameter("P_IS_STARTREK", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_starTrek },
                new OracleParameter("P_ORDER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_id },
                new OracleParameter("P_IS_ONLINE_SALE", OracleDbType.Int32, ParameterDirection.Input) { Value = model.is_online_sale },
                };

                orderResponse = await _oracleDataManagerV2.SelectProcedureV2("SUBMITORDER13", parameters.ToArray());

            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV7",
                    procedure_name = "SUBMITORDER13",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());

            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV7",
                    procedure_name = "SUBMITORDER13",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return orderResponse;
        }

        public async Task<DataTable> SubmitOrderV8(OrderRequest4 model, string loginProviderId)
        {
            OracleDataManager _odm = new OracleDataManager();

            DataTable orderResponse = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                   new OracleParameter("P_BSS_ReqId", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                   new OracleParameter("P_Status", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                   new OracleParameter("P_error_id", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                   new OracleParameter("p_error_description", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                   new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                   new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                   new OracleParameter("P_DEST_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                   new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                   new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                   new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                   new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                   new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                   new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                   new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                   new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                   new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                   new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                   new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                   new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                   new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                   new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                   new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                   new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                   new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                   new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                   new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                   new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                   new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                   new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                   new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                   new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_code },
                   new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                   new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_thumb },
                   new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                   new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_left_index },
                   new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                   new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_thumb },
                   new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                   new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.dest_right_index },
                   new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                   new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_thumb },
                   new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                   new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_left_index },
                   new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                   new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_thumb },
                   new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                   new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Blob, ParameterDirection.Input) { Value = model.src_right_index },
                   new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                   new OracleParameter("P_PORT_IN_DATE", OracleDbType.Date, ParameterDirection.Input) { Value = model.port_in_date },
                   new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                   new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                   new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                   new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                   new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                   new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                   new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                   new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                   new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                   new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                   new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                   new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                   new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                   new OracleParameter("P_MSISDNRESERVATIONID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                   new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                   new OracleParameter("P_DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                   new OracleParameter("P_DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                   new OracleParameter("P_THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                   new OracleParameter("P_CENTER_OR_DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                   new OracleParameter("P_SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                   new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                   new OracleParameter("P_RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id },
                   new OracleParameter("P_SIM_REPLACEMENT_TYPE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_replacement_type },
                   new OracleParameter("P_OLD_SIM_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.old_sim_number },
                   new OracleParameter("P_SRC_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_sim_category },
                   new OracleParameter("P_PORT_IN_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_confirmation_code },
                   new OracleParameter("P_DEST_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_ec_verifi_reqrd },
                   new OracleParameter("P_SRC_EC_VERIFI_REQRD", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_ec_verifi_reqrd },
                   new OracleParameter("P_DEST_FOREIGN_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_foreign_flag },
                   new OracleParameter("P_DBSS_SUBSCRIPTION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dbss_subscription_id },
                   new OracleParameter("P_SAF_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.saf_status },
                   new OracleParameter("P_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_id },
                   new OracleParameter("P_CONFIRMATION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.order_confirmation_code },
                   new OracleParameter("P_SERVER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.server_name },
                   new OracleParameter("P_SRC_OWNER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_owner_customer_id },
                   new OracleParameter("P_SRC_USER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_user_customer_id },
                   new OracleParameter("P_SRC_PAYER_CUSTOMER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_payer_customer_id },
                   new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi },
                   new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProviderId },
                   new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                   new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                   new OracleParameter("P_LAC_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.lac },
                   new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = model.cid },
                   new OracleParameter("P_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                   new OracleParameter("P_ORDER_BOOKING_FLAG", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.order_booking_flag },
                   new OracleParameter("P_IS_ESIM", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_esim },
                   new OracleParameter("P_SELECTED_CATEGORY", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.selected_category },
                };
                
                orderResponse = await _oracleDataManagerV2.SelectProcedureV2("SUBMITORDER16", parameters.ToArray());

            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV8",
                    procedure_name = "SUBMITORDER16",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());

            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrderV8",
                    procedure_name = "SUBMITORDER16",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return orderResponse;
        }
        public async Task<decimal> SubmitOrder(OrderRequest model)
        {
            decimal BIAReqsTokenId;
            try
            {
                var dateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number.HasValue ? model.purpose_number : null },
                new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                new OracleParameter("P_SIM_CATEGORY", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_category.HasValue ? model.sim_category : null },
                new OracleParameter("P_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                new OracleParameter("P_SUBSCRIPTION_TYPE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.subscription_type_id.HasValue ? model.subscription_type_id : null },
                new OracleParameter("P_SUBSCRIPTION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.subscription_code },
                new OracleParameter("P_PACKAGE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.package_id.HasValue ? model.package_id : null },
                new OracleParameter("P_PACKAGE_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.package_code },
                new OracleParameter("P_DEST_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_doc_type_no.HasValue ? model.dest_doc_type_no : null },
                new OracleParameter("P_DEST_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_nid },
                new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                new OracleParameter("P_SRC_DOC_TYPE_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_doc_type_no.HasValue ? model.src_doc_type_no : null },
                new OracleParameter("P_SRC_NID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_nid },
                new OracleParameter("P_SRC_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_dob },
                new OracleParameter("P_PLATFORM_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.platform_id },
                new OracleParameter("P_CUSTOMER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.customer_name },
                new OracleParameter("P_GENDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.gender },
                new OracleParameter("P_FLAT_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.flat_number },
                new OracleParameter("P_HOUSE_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.house_number },
                new OracleParameter("P_ROAD_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.road_number },
                new OracleParameter("P_VILLAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.village },
                new OracleParameter("P_DIVISION_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.division_id.HasValue ? model.division_id : null },
                new OracleParameter("P_DISTRICT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.district_id.HasValue ? model.district_id : null },
                new OracleParameter("P_THANA_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.thana_id.HasValue ? model.thana_id : null },
                new OracleParameter("P_POSTAL_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.postal_code },
                new OracleParameter("P_EMAIL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.email },
                new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.salesman_code },
                new OracleParameter("P_DEST_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_thumb_score.HasValue ? model.dest_left_thumb_score : null },
                new OracleParameter("P_DEST_LEFT_THUMB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_left_thumb },
                new OracleParameter("P_DEST_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_left_index_score.HasValue ? model.dest_left_index_score : null },
                new OracleParameter("P_DEST_LEFT_INDEX", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_left_index },
                new OracleParameter("P_DEST_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_thumb_score.HasValue ? model.dest_right_thumb_score : null },
                new OracleParameter("P_DEST_RIGHT_THUMB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_right_thumb },
                new OracleParameter("P_DEST_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.dest_right_index_score.HasValue ? model.dest_right_index_score : null },
                new OracleParameter("P_DEST_RIGHT_INDEX", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_right_index },
                new OracleParameter("P_SRC_LEFT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_thumb_score.HasValue ? model.src_left_thumb_score : null },
                new OracleParameter("P_SRC_LEFT_THUMB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_left_thumb },
                new OracleParameter("P_SRC_LEFT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_left_index_score.HasValue ? model.src_left_index_score : null },
                new OracleParameter("P_SRC_LEFT_INDEX", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_left_index },
                new OracleParameter("P_SRC_RIGHT_THUMB_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_thumb_score.HasValue ? model.src_right_thumb_score : null },
                new OracleParameter("P_SRC_RIGHT_THUMB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_right_thumb },
                new OracleParameter("P_SRC_RIGHT_INDEX_SCORE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.src_right_index_score.HasValue ? model.src_right_index_score : null },
                new OracleParameter("P_SRC_RIGHT_INDEX", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.src_right_index },
                new OracleParameter("P_USER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                new OracleParameter("P_PORT_IN_DATE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.port_in_date },
                new OracleParameter("P_ALT_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.alt_msisdn },
                new OracleParameter("P_POC_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.poc_number },
                new OracleParameter("P_IS_URGENT", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_urgent.HasValue ? model.is_urgent : null },
                new OracleParameter("P_OPTIONAL1", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional1 },
                new OracleParameter("P_OPTIONAL2", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional2 },
                new OracleParameter("P_OPTIONAL3", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.optional3 },
                new OracleParameter("P_OPTIONAL4", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional4.HasValue ? model.optional4 : null },
                new OracleParameter("P_OPTIONAL5", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional5.HasValue ? model.optional5 : null },
                new OracleParameter("P_OPTIONAL6", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.optional6.HasValue ? model.optional6 : null },
                new OracleParameter("P_CREATE_DATE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = dateTime },
                new OracleParameter("P_UPDATE_DATE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = dateTime },
                new OracleParameter("P_NOTE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.note },
                new OracleParameter("P_SIM_REP_REASON_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.sim_rep_reason_id.HasValue ? model.sim_rep_reason_id : null },
                new OracleParameter("P_PAYMENT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.payment_type },
                new OracleParameter("P_ISPAIRED", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.is_paired : null },
                new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_paired.HasValue ? model.cahnnel_id : null },
                new OracleParameter("DIVISION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.division_name },
                new OracleParameter("DISTRICT_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.district_name },
                new OracleParameter("THANA_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.thana_name },
                new OracleParameter("CENTER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.center_code },
                new OracleParameter("DISTRIBUTOR_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.distributor_code },
                new OracleParameter("SIM_REPLC_REASON", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_replc_reason },
                new OracleParameter("CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                new OracleParameter("RIGHT_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.right_id }
            };

                var result = await _oracleDataManagerV2.CallInsertProcedure("SUBMITORDER",parameters.ToArray());
                BIAReqsTokenId = Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    retailer_id = model.retailer_id,
                    request_model = Convert.ToString(model),
                    method_name = "SubmitOrder",
                    procedure_name = "SUBMITORDER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return BIAReqsTokenId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> GetStatus(StatusRequest model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = Convert.ToDecimal(model.request_id) },
                    new OracleParameter("PO_STASUS", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETSTATUS", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "GetStatus",
                    procedure_name = "BIA_GETSTATUS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return result;
        }

        /// <summary>
        /// This method is used for Activity Log Long press.
        /// </summary>
        /// <param name="token_id"></param>
        /// <returns></returns>
        public async Task<long> CheckBIAToken(string token_id)
        {
            long tokenNo = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_TOKEN_NO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = token_id },
                    new OracleParameter("PO_RESULT", OracleDbType.Decimal, ParameterDirection.Output)
                };
                var result = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("BIA_CHECKBIATOKENID", "PO_RESULT", parameters.ToArray());
                tokenNo = Convert.ToInt64(result.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "CheckBIAToken",
                    procedure_name = "BIA_CHECKBIATOKENID",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return tokenNo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token_id"></param>
        /// <returns></returns>
        public async Task<DataTable> GetOrderInfoByTokenNo(decimal token_id)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_TOKEN_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = token_id },
                    new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETORDERINFO",parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetOrderInfoByTokenNo",
                    procedure_name = "BIA_GETORDERINFO",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> GetPortInOrderConfirmCode(int purposeId, string msisdn)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("P_PURPOSE_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = Convert.ToDecimal(purposeId) },
                   new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = msisdn },
                   new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output),
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETORDERCONFIRMCODE",parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetTOSPortInOrderConfirmCode",
                    procedure_name = "BIA_GETORDERCONFIRMCODE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return result;
        }

        public async Task<DataTable> ValidateOrder(VMValidateOrder model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                    new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                    new OracleParameter("P_PURPOSE_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.purpose_number == null ? DBNull.Value : model.purpose_number },
                    new OracleParameter("P_IS_CORPORATE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_corporate == null ? DBNull.Value : model.is_corporate },
                    new OracleParameter("P_RETAILER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailer_id },
                    new OracleParameter("P_DEST_DOB", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_dob },
                    new OracleParameter("PO_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_VALIDATEORDERV3",parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateOrder",
                    procedure_name = "BIA_VALIDATEORDERV3",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return result;
        }

        /// <summary>
        /// Checks if submitted order is in process or not.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateOrder_(VMValidateOrder model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("P_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdn },
                   new OracleParameter("P_DEST_SIM_NUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.sim_number },
                   new OracleParameter("PO_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output),
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_VALIDATEORDER", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateOrder_",
                    procedure_name = "BIA_VALIDATEORDER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> GetInventoryIdByChannelName(string channelName)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("P_CHENNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = channelName },
                   new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output),
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETINVENTORYIDBYCHANNEL", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetInventoryIdByChannelName",
                    procedure_name = "BIA_GETINVENTORYIDBYCHANNEL",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> GetCenterCodeByUserName(string userName)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                   new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETCENTERCODEBYUSERNAME", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetCenterCodeByUserName",
                    procedure_name = "BIA_GETCENTERCODEBYUSERNAME",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }
            return result;
        }
        #endregion

        #region ===================| Order BSS Service |======================
        public async Task<DataTable> GetBssDataList(OrderListReqModel reqModel)
        {
            DataTable dt;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_ORDER_STATUS", OracleDbType.Int32, ParameterDirection.Input) { Value = reqModel.order_staus },
                    new OracleParameter("P_BSS_FLAG", OracleDbType.Int32, ParameterDirection.Input) { Value = reqModel.order_flag },// this is static for booking data
                    new OracleParameter("P_MAX_ROW", OracleDbType.Int32, ParameterDirection.Input) { Value = reqModel.max_row },
                    new OracleParameter("PO_BSSDATALIST", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                dt = await _oracleDataManagerV2.SelectProcedure("BSS_GETORDERDATALIST", parameters.ToArray());
                return dt;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetBssDataList",
                    procedure_name = "BSS_GETORDERDATALIST",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<bool> UpdateBioDbForOrderReq(string bi_token_no, string order_conframtion_code)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NO", bi_token_no),
                    new OracleParameter("P_ORDER_CONFRAMTION_CODE", order_conframtion_code)
                };
                
                bool rowAffect = await _oracleDataManagerV2.CallUpdateProcedure("BSS_UPDBIREQUESTFORORDER", parameters.ToArray());
                return rowAffect;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "UpdateBioDbForOrderReq",
                    procedure_name = "BSS_UPDBIREQUESTFORORDER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateBioDbForCreateCustomerReq(string bi_token_no, string owner_customer_id)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NO", bi_token_no),
                    new OracleParameter("P_DEST_CUSTOMER_ID", owner_customer_id)
                };
                bool rowAffect = await _oracleDataManagerV2.CallUpdateProcedure("BSS_UPDBIREQFORCREATECUSTOMER");
                return rowAffect;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "UpdateBioDbForCreateCustomerReq",
                    procedure_name = "BSS_UPDBIREQFORCREATECUSTOMER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> ClearBookingFlagForOrderReq(int order_booking_flag)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_ORDER_BOOKING_FLAG", order_booking_flag)
                };                

                bool rowAffect = await _oracleDataManagerV2.CallUpdateProcedure("BSS_CLEARORDERBOOKING", parameters.ToArray());
                return rowAffect;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ClearBookingFlagForOrderReq",
                    procedure_name = "BSS_CLEARORDERBOOKING",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<DataTable> GetBTSInformationByLacCid(int lac, int cid)
        {
            DataTable dt;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_LAC", OracleDbType.Int32, ParameterDirection.Input) { Value = lac },
                    new OracleParameter("P_CELL_ID", OracleDbType.Int32, ParameterDirection.Input) { Value = cid },
                    new OracleParameter("PO_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                dt = await _oracleDataManagerV2.SelectProcedure("BSS_GETBTSINFOBYLACCID", parameters.ToArray());
                return dt;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetBTSInformationByLacCid",
                    procedure_name = "BSS_GETBTSINFOBYLACCID",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region ====================| SIM Replacement Area |================
        public async Task<DataTable> GetSIMReplacementReasons()
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_REASON", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETSIMREPLACEMENTREASONS", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetSIMReplacementReasons",
                    procedure_name = "BIA_GETSIMREPLACEMENTREASONS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return result;
        }
        #endregion

        #region ====================| Authentication and Authorization |=================
        public async Task<DataTable> ValidateUser(vmUserInfo model)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.user_name },
                    new OracleParameter("PI_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.password },
                    new OracleParameter("PO_QC_USER", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_USER", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateUser",
                    procedure_name = "VALIDATE_USER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<DataTable> ValidateUserV2(LoginRequestsV2 userModel, vmUserInfo model)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.user_name },
                    new OracleParameter("PI_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.password },
                    new OracleParameter("PI_BP_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.BPMSISDN },
                    new OracleParameter("PO_QC_USER", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_USERV3", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateUser",
                    procedure_name = "VALIDATE_USER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<DataTable> ValidateUserV3(FPValidationReqModel userModel)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.user_name },
                    new OracleParameter("PI_BP_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.BPMSISDN },
                    new OracleParameter("PO_QC_USER", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_USERV4", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateUserV3",
                    procedure_name = "VALIDATE_USERV4",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }


        /// <summary>
        /// ValidateUser without password for reseller. 
        /// </summary>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateUser(string user_name)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                   new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = user_name },
                   new OracleParameter("PO_QC_USER", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("RESELLER_VALIDATE_USER", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateUser",
                    procedure_name = "VALIDATE_USER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }
        public async Task<int> GetUserAPIVersion(APIVersionRequest model)
        {
            int apiVersion = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username },
                    new OracleParameter("P_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = "" }
                    //new OracleParameter("PO_APIVERSION", OracleDbType.Int32, ParameterDirection.Output)
                };
                
                var result = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("USERAPIVERSION", "PO_APIVERSION", parameters.ToArray());
                apiVersion = Convert.ToInt32(result.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUserAPIVersion",
                    procedure_name = "USERAPIVERSION",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return Convert.ToInt32(apiVersion);
        }
        public async Task<DataTable> GetUserAPIVersionWithAppUpdateCheck(VMAPIVersionRequestWithAppUpdateCheck model)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                var result = await _oracleDataManagerV2.SelectProcedure("GETAPPUPDATEINFO", parameters.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUserAPIVersionWithAppUpdateCheck",
                    model = Convert.ToString(model),
                    procedure_name = "GETAPPUPDATEINFO",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<long> SaveLoginAtmInfo(UserLogInAttempt model)
        {
            long result = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                     new OracleParameter("P_USERID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.userid },
                     new OracleParameter("P_IS_SUCCESS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_success },
                     new OracleParameter("P_IP_ADDRESS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.ip_address },
                     new OracleParameter("P_MACHINE_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.machine_name },
                     new OracleParameter("P_LOGINPROVIDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.loginprovider },
                     new OracleParameter("P_DEVICEID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.deviceid },
                     new OracleParameter("P_LAN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.lan },
                     new OracleParameter("P_VERSIONCODE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.versioncode },
                     new OracleParameter("P_VERSIONNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.versionname },
                     new OracleParameter("P_OSVERSION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.osversion },
                     new OracleParameter("P_KERNELVERSION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.kernelversion },
                     new OracleParameter("P_FERMWAREVIRSION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.fermwarevirsion }
                };
                
                result = await _oracleDataManagerV2.CallInsertProcedure("USERLOGINATTEMPTINSERT", parameters.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SaveLoginAtmInfo",
                    model = Convert.ToString(model),
                    procedure_name = "USERLOGINATTEMPTINSERT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<long> SaveLoginAtmInfoV2(UserLogInAttemptV2 model)
        {
            long result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.userid },
                    new OracleParameter("P_IS_SUCCESS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_success },
                    new OracleParameter("P_IP_ADDRESS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.ip_address },
                    new OracleParameter("P_MACHINE_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.machine_name },
                    new OracleParameter("P_LOGINPROVIDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.loginprovider },
                    new OracleParameter("P_DEVICEID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.deviceid },
                    new OracleParameter("P_LAN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.lan },
                    new OracleParameter("P_VERSIONCODE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.versioncode },
                    new OracleParameter("P_VERSIONNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.versionname },
                    new OracleParameter("P_OSVERSION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.osversion },
                    new OracleParameter("P_KERNELVERSION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.kernelversion },
                    new OracleParameter("P_FERMWAREVIRSION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.fermwarevirsion },
                    new OracleParameter("P_LATITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.latitude },
                    new OracleParameter("P_LONGITUDE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.longitude },
                    new OracleParameter("P_LAC", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.lac },
                    new OracleParameter("P_CID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.cid },
                    new OracleParameter("P_IS_BP", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.is_bp },
                    new OracleParameter("P_BP_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bp_msisdn },
                    new OracleParameter("P_DEVICE_MODEL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.device_model }
                };                
                result = await _oracleDataManagerV2.CallInsertProcedure("USERLOGINATTEMPTINSERTV2", parameters.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SaveLoginAtmInfoV2",
                    model = Convert.ToString(model),
                    procedure_name = "USERLOGINATTEMPTINSERTV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<int> IsSecurityTokenValid(string loginProvider)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "p_Login_Provider", Value = loginProvider }
                };

                status = await _oracleDataManagerV2.CallInsertProcedureV3("LOGINPROVIDERVALIDORNOT", parameters.ToArray());
                
                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsSecurityTokenValid",
                    procedure_name = "LOGINPROVIDERVALIDORNOT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<int> IsAESEligibleUser(string retailer)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_RETAILER_ID", Value = retailer }
                };

                status = await _oracleDataManagerV2.CallInsertProcedureV3("BSSCHECKELIGIBLEUSER", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsAESEligibleUser",
                    procedure_name = "BSSCHECKELIGIBLEUSER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<int> ChangePassword(VMChangePassword model)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_OLD_PASS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.old_password },
                    new OracleParameter("P_NEW_PASS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_password },
                    new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username }
                };

                var data = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("CHANGEPASSWORD", "po_PKValue", parameters.ToArray());

                return Convert.ToInt32(data.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ChangePassword",
                    procedure_name = "CHANGEPASSWORD",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<int> ChangePasswordV2(VMChangePassword model)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_OLD_PASS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.old_password },
                    new OracleParameter("P_NEW_PASS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_password },
                    new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username }                    
                };               

                var data = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("CHANGEPASSWORDV2", "po_PKValue", parameters.ToArray());

                return Convert.ToInt32(data.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ChangePasswordV2",
                    procedure_name = "CHANGEPASSWORDV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<int> ChangePasswordV3(VMChangePassword model)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_OLD_PASS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.old_password },
                    new OracleParameter("P_NEW_PASS", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_password },
                    new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.username }                    
                };               

                var data = await _oracleDataManagerV2.CallSelectDataWithObjectReturn("CHANGEPASSWORDV3", "po_PKValue", parameters.ToArray());

                return Convert.ToInt32(data.ToString());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ChangePasswordV3",
                    procedure_name = "CHANGEPASSWORDV3",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<DataTable> GetPasswordLength()
        {
            DataTable dataRows;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("po_PKValue", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                return dataRows = await _oracleDataManagerV2.SelectProcedure("GETPASSWORDLENGTH", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetPasswordLength",
                    procedure_name = "GETPASSWORDLENGTH",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<DataTable> GetPasswordLengthV2()
        {
            DataTable dataRows;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("po_PKValue", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                return dataRows = await _oracleDataManagerV2.SelectProcedure("GETPASSWORDLENGTHV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetPasswordLengthV2",
                    procedure_name = "GETPASSWORDLENGTHV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }


        //================New Forget PWD =================
        public async Task<DataTable> GetUserMobileNoAndOTP(string userName)
        {
            DataTable dataTable;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("PO_USERINFO", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                dataTable = await _oracleDataManagerV2.SelectProcedure("GETUSERMOBILENO", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUserMobileNoAndOTP",
                    procedure_name = "GETUSERMOBILEANDOTP",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        //================New Forget PWD =================
        public async Task<DataTable> GetUserMobileNoAndOTPV2(string userName)
        {
            //_oracleDataManager = new OracleDataManager();
            DataTable dataTable;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("PO_USERINFO", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                dataTable = await _oracleDataManagerV2.SelectProcedure("GETUSERMOBILENOV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUserMobileNoAndOTPV2",
                    procedure_name = "GETUSERMOBILENOV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        public async Task<int> FORGETPWD(VMForgetPWD model)
        {
            int result = 0;
            long? values = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.user_id },
                    new OracleParameter("P_MOBILENO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.mobile_no },
                    new OracleParameter("P_NEW_PWD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_pwd },
                    new OracleParameter("P_NEW_HASHPWD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_hashed_pwd },
                };

                values = await _oracleDataManagerV2.CallInsertProcedure("FORGETPWD", parameters.ToArray());
                result = Convert.ToInt32(values);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "FORGETPWD",
                    procedure_name = "FORGETPWD",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);

            }
            return result;
        }

        public async Task<int> FORGETPWDV2(VMForgetPWD model)
        {
            //_oracleDataManager = new OracleDataManager();
            int result;
            long? values = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.user_id },
                    new OracleParameter("P_MOBILENO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.mobile_no },
                    new OracleParameter("P_NEW_PWD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_pwd },
                    new OracleParameter("P_NEW_HASHPWD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_hashed_pwd }
                };                
                values = await _oracleDataManagerV2.CallInsertProcedure("FORGETPWDV2", parameters.ToArray());
                result = Convert.ToInt32(values);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "FORGETPWDV2",
                    procedure_name = "FORGETPWDV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<int> FORGETPWDV3(VMForgetPWD model)
        {
            //_oracleDataManager = new OracleDataManager();
            int result;
            long? values = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.user_id },
                    new OracleParameter("P_MOBILENO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.mobile_no },
                    new OracleParameter("P_NEW_PWD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_pwd },
                    new OracleParameter("P_NEW_HASHPWD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.new_hashed_pwd }
                };
                
                values = await _oracleDataManagerV2.CallInsertProcedure("FORGETPWDV3", parameters.ToArray());
                result = Convert.ToInt32(values);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "FORGETPWDV2",
                    procedure_name = "FORGETPWDV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }
        //============x==================

        public async Task< DataTable> IsUserCurrentlyLoggedIn(decimal userId)
        {
            //_oracleDataManager = new OracleDataManager();
            DataTable dataTable;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USER_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = userId },
                    new OracleParameter("PO_LOGIN_PROVIDER", OracleDbType.RefCursor, ParameterDirection.Output)
                };               

                dataTable = await _oracleDataManagerV2.SelectProcedure("ISUSERCURRENTLYLOGGEDIN", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsUserCurrentlyLoggedIn",
                    procedure_name = "ISUSERCURRENTLYLOGGEDIN",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);

            }
            return dataTable;
        }


        public async Task<int> IsSecurityTokenValid2(string loginProvider, string deviceId)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_DEVICEID", Value = deviceId },
                    new OracleParameter() { ParameterName = "P_LOGIN_PROVIDER", Value = loginProvider }
                };  
                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_LOGINPROVIDERVALIDORNOT", parameters.ToArray());
                
                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsSecurityTokenValid2",
                    procedure_name = "BIA_LOGINPROVIDERVALIDORNOT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<long> IsSecurityTokenValidV3(string loginProvider, string deviceId)
        {
            long status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_DEVICEID", Value = deviceId },
                    new OracleParameter() { ParameterName = "P_LOGIN_PROVIDER", Value = loginProvider }
                };

                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_LOGINPROVIDERVALIDORNOTV3", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsSecurityTokenValidV3",
                    procedure_name = "BIA_LOGINPROVIDERVALIDORNOTV3",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<long> IsSecurityTokenValidForBPLogin(string loginProvider, string deviceId)
        {

            long status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_DEVICEID", Value = deviceId },
                    new OracleParameter() { ParameterName = "P_LOGIN_PROVIDER", Value = loginProvider }
                };

                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_CHKLOGINPROVIDFORBPLOGIN", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "IsSecurityTokenValidV3",
                    procedure_name = "BIA_LOGINPROVIDERVALIDORNOTV3",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<DataTable> GetChangePasswordGlobalSettingsData()
        {
            DataTable dataRows;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("po_PKValue", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                return dataRows = await _oracleDataManagerV2.SelectProcedure("GETDATAFORPWDCHANGE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetChangePasswordGlobalSettingsData",
                    procedure_name = "GETDATAFORPWDCHANGE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        public async Task<DataTable> GetChangePasswordGlobalSettingsDataV2()
        {
            DataTable dataRows;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("po_PKValue", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                return dataRows = await _oracleDataManagerV2.SelectProcedure("GETDATAFORPWDCHANGEV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetChangePasswordGlobalSettingsDataV2",
                    procedure_name = "GETDATAFORPWDCHANGE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateDbssUser(vmUserInfo model)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.user_name },
                    new OracleParameter("PI_PASSWORD", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.password },
                    new OracleParameter("PO_QC_USER", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_DBSS_USER", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateDbssUser",
                    procedure_name = "VALIDATE_DBSS_USER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateBPUser(string bp_msisdn, string user_name)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = user_name },
                    new OracleParameter("P_BP_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bp_msisdn },
                    new OracleParameter("PO_CURR", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_BP_USER", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateBPUser",
                    procedure_name = "VALIDATE_BP_USER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);

            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateBPUserV1(string bp_msisdn, string user_name)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = user_name },
                    new OracleParameter("P_BP_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = bp_msisdn },
                    new OracleParameter("PO_CURR", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_BP_USERV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateBPUserV1",
                    procedure_name = "VALIDATE_BP_USERV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);

            }
            return result;
        }

        public async Task<int> GenerateBPLoginOTP(string loginProvider)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_LOGIN_PROVIDER", Value = loginProvider }
                };

                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_GENERATEBPLOGINOTP", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GenerateBPLoginOTP",
                    procedure_name = "BIA_GENERATEBPLOGINOTP",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GenerateBPLoginOTPV2(string loginProvider)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_LOGIN_PROVIDER", Value = loginProvider }
                };
                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_GENERATEBPLOGINOTPV2", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GenerateBPLoginOTPV2",
                    procedure_name = "BIA_GENERATEBPLOGINOTPV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateBPOtp(decimal bp_otp, decimal retailer_otp, string sessionToken)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = sessionToken },
                    new OracleParameter("P_BP_OTP", OracleDbType.Decimal, ParameterDirection.Input) { Value = bp_otp },
                    new OracleParameter("P_RETAILER_OTP", OracleDbType.Decimal, ParameterDirection.Input) { Value = retailer_otp },
                    new OracleParameter("PO_CURR", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_BP_OTP", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateBPOtp",
                    procedure_name = "VALIDATE_BP_OTP",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<DataTable> ValidateBPOtpV2(decimal bp_otp, decimal retailer_otp, string sessionToken)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_SESSION_TOKEN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = sessionToken },
                    new OracleParameter("P_BP_OTP", OracleDbType.Decimal, ParameterDirection.Input) { Value = bp_otp },
                    new OracleParameter("P_RETAILER_OTP", OracleDbType.Decimal, ParameterDirection.Input) { Value = retailer_otp },
                    new OracleParameter("PO_CURR", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("VALIDATE_BP_OTPV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ValidateBPOtpV2",
                    procedure_name = "VALIDATE_BP_OTPV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<int> ResendBPOTP(string loginProviderId)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_SESSION_TOKEN", Value = loginProviderId }
                };
                
                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_RESENDBPOTP", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ResendBPOTP",
                    procedure_name = "BIA_RESENDBPOTP",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<int> ResendBPOTPV2(string loginProviderId)
        {
            int status = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter() { ParameterName = "P_SESSION_TOKEN", Value = loginProviderId }
                };
                status = await _oracleDataManagerV2.CallInsertProcedureV3("BIA_RESENDBPOTPV2", parameters.ToArray());

                return status;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "ResendBPOTPV2",
                    procedure_name = "BIA_RESENDBPOTPV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }

        public async Task<long> Logout(string loginProvider)
        {
            long result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_LOGIN_PROVIDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProvider }
                };
                
                result =await _oracleDataManagerV2.CallInsertProcedure("BIA_USER_LOGOUT", parameters.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "Logout",
                    procedure_name = "BSS_USER_LOGOUT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        #endregion

        public async Task<DataTable> GetUnpairedMSISDNSearchDefaultValue(UnpairedMSISDNListReqModel model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("PO_PURS", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETMSISDNDEFAULTVALUE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUnpairedMSISDNSearchDefaultValue",
                    procedure_name = "BIA_GETMSISDNDEFAULTVALUE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetUnpairedMSISDNSearchDefaultValueV2(UnpairedMSISDNListReqModel model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("PO_PURS", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETMSISDNDEFAULTVALUEV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUnpairedMSISDNSearchDefaultValueV2",
                    procedure_name = "BIA_GETMSISDNDEFAULTVALUEV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetUnpairedMSISDNSearchDefaultValueCherished(UnpairedMSISDNListReqModelV2 model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("P_CATEGORY", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.Selected_category },
                    new OracleParameter("PO_PURS", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETMSISDNDEFAULTVALCHER", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUnpairedMSISDNSearchDefaultValue",
                    procedure_name = "BIA_GETMSISDNDEFAULTVALCHER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return result;
        }
        public async Task<DataTable> GetStockAvailable(string channel_Name)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = channel_Name },
                    new OracleParameter("PO_CHANNEL_ID", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETSTOCKAVAILABLE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetStockAvailable",
                    procedure_name = "BIA_GETSTOCKAVAILABLE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return result;
        }
        public async Task<decimal> UpdateOrder(OrderRequest3 model)
        {
            OracleDataManager _odm = new OracleDataManager();
            decimal BIAReqsTokenId = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number.HasValue ? model.bi_token_number : null },
                    new OracleParameter("P_BSS_REQ_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.bss_reqId },
                    new OracleParameter("P_STATUS", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.status },
                    new OracleParameter("P_ERROR_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.error_id },
                    new OracleParameter("P_ERROR_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.error_description },
                    new OracleParameter("P_MSISDN_RESERVATION_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.msisdnReservationId },
                    new OracleParameter("P_DEST_IMSI", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.dest_imsi }
                };               

                var result = await _oracleDataManagerV2.CallInsertProcedure("UPDATEORDER", parameters.ToArray());

                BIAReqsTokenId = Convert.ToDecimal(result);
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "UpdateOrder",
                    procedure_name = "UPDATEORDER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailer_id,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "UpdateOrder",
                    procedure_name = "UPDATEORDER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                    server_name = model.server_name
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
            }
            return BIAReqsTokenId;
        }

        public async Task<DataTable> GetPaymentMethod(RAGetPaymentMehtodRequest model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.channel_id },
                    new OracleParameter("PO_PAYMENT_METHOD", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETPAYMENTMETHOD", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "GetPaymentMethod",
                    procedure_name = "BIA_GETPAYMENTMETHOD",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetPaymentMethodV2(RAGetPaymentMehtodRequest model, string userName)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_ID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.channel_id },
                    ///new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName });
                    new OracleParameter("PO_PAYMENT_METHOD", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETPAYMENTMETHOD", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "GetPaymentMethod",
                    procedure_name = "BIA_GETPAYMENTMETHODV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        #region Geofencing
        public async Task<DataTable> GetLoggedinRetLatLon(string retailerCode)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = retailerCode },
                    new OracleParameter("PO_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETRETLATLON", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(retailerCode),
                    method_name = "GetLoggedinRetLatLon",
                    procedure_name = "BIA_GETRETLATLON",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }
        #endregion
        #region Retailer user synchronization
        public async Task<decimal?> UpdateRetailerUserByDMS(DMSRetailerReqModel model)
        {
            OracleDataManager _odm = new OracleDataManager();
            decimal? successNumber = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailerCode },
                    new OracleParameter("P_IS_ACTIVE", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.isActive },
                    new OracleParameter("P_ITOPUPNUMBER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.iTopUpNumber }
                };               

                var result = await _oracleDataManagerV2.CallInsertProcedure("BIA_UPDATE_DMS_USER", parameters.ToArray());

                successNumber = Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "UpdateRetailerUserByDMS",
                    procedure_name = "BIA_UPDATE_DMS_USER",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return successNumber;
        }
        #endregion

        #region First Recharge
        public async Task<DataTable> GetRechargeAmount(RechargeAmountReqModel model, string userName)
        {
            DataTable result;
            OracleDataManager _odm = new OracleDataManager();
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("PO_AMOUNT", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETRCHRGAMOUNTV2", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetRechargeAmount",
                    procedure_name = "BIA_GETRCHRGAMOUNTV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetRechargeAmountV2(RechargeAmountReqModelRev model, string userName)
        {
            DataTable result;
            OracleDataManager _odm = new OracleDataManager();
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("PO_AMOUNT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETRCHRGAMOUNTV3", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    //request_model = Convert.ToString(model),
                    method_name = "GetRechargeAmount",
                    procedure_name = "BIA_GETRCHRGAMOUNTV2",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public decimal UpdateOrderFirstRechargeStatus(decimal RequestId)
        {
            decimal response = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = RequestId }
                };                

                var result = _oracleDataManagerV2.CallInsertProcedure("UPDATEORDERFIRSTRECHARGESTATUS", parameters.ToArray());

                response = Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SUBMITCOMPLAINT",
                    procedure_name = "UPDATEORDERCOMPLAINTSTATUS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,

                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return response;
        }

        #endregion
        #region Help button Area
        public async Task<DataTable> GetUserTypeDropdownValu()
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_USERTYPE", OracleDbType.RefCursor, ParameterDirection.Output)
                };                
                result = await _oracleDataManagerV2.SelectProcedure("GETUSERTYPEVALUE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUserTypeDropdownValu",
                    procedure_name = "GETUSERTYPEVALUE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }

        public async Task<DataTable> GetContentTypeDropdownValue()
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_CONTENT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GETCONTENTTYPEVALUE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetContentTypeDropdownValue",
                    procedure_name = "GETCONTENTTYPEVALUE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }

        public async Task<DataTable> GetContentURL()
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_CONTENTURL", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GETCONTENTURL", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetContentURL",
                    procedure_name = "GETCONTENTURL",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }


        #endregion

        #region Raise Complaint
        public async Task<decimal> SubmitComplaint(SubmitComplaintModel model)
        {
            decimal ComplaintId = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_COMPLAINT_TYPE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.complaintType },
                    new OracleParameter("P_COMPLAINT_TITLE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.complaintTitle },
                    new OracleParameter("P_DESCRIPTION", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.description },
                    new OracleParameter("P_PREFERRED_LEVEL", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.preferredLevel },
                    new OracleParameter("P_PREFERRED_LEVEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.preferredLevelName },
                    new OracleParameter("P_PREFERRED_LEVEL_CONTACT", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.preferredLevelContact },
                    new OracleParameter("P_RETAILER_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.retailerCode }
                };
                
                var result = await _oracleDataManagerV2.CallInsertProcedure("SUBMITCOMPLAINT",parameters.ToArray());

                ComplaintId = Convert.ToDecimal(result);
            }
            catch (OracleException ex)
            {
                string? text = Convert.ToString(new
                {
                    retailer_id = model.retailerCode,
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "SUBMITCOMPLAINT",
                    procedure_name = "SUBMITCOMPLAINT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,

                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }

            return ComplaintId;
        }

        public async Task<decimal> UpdateOrderComplaintStatus(decimal RequestId)
        {
            OracleDataManager _odm = new OracleDataManager();
            decimal response = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Decimal, ParameterDirection.Input) { Value = RequestId }
                };
                var result = await _oracleDataManagerV2.CallInsertProcedure("UPDATEORDERCOMPLAINTSTATUS", parameters.ToArray());

                response = Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {

                    request_time = DateTime.Now,
                    method_name = "SUBMITCOMPLAINT",
                    procedure_name = "UPDATEORDERCOMPLAINTSTATUS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,

                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return response;
        }


        #endregion

        #region Resubmit
        public async Task<DataTable> GetResubmitData(ResubmitReqModel model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_TOKEN_NO", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.bi_token_number },
                    new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETRESUBMITINFO", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    request_model = Convert.ToString(model),
                    method_name = "GetResubmitData",
                    procedure_name = "BIA_GETRESUBMITINFO",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }
        #endregion
        #region App info Update from Retailer
        public async Task<long> AppInfoUpdate(AppInfoUpdateReqModel model, string loginProvider)
        {
            long result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_LOGIN_PROVIDER", OracleDbType.Varchar2, ParameterDirection.Input) { Value = loginProvider },
                    new OracleParameter("P_VERSION_CODE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.app_version_code },
                    new OracleParameter("P_VERSION_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.app_version_name }
                };
                
                result = await _oracleDataManagerV2.CallInsertProcedure("BIA_APP_INFO_UPD", parameters.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "AppInfoUpdate",
                    procedure_name = "BIA_APP_INFO_UPD",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
        }
        #endregion

        public async Task<DataTable> GetBTSCode(SiteIdRequestModel model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_LAC", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.lac },
                    new OracleParameter("P_CID", OracleDbType.Decimal, ParameterDirection.Input) { Value = model.cid },
                    new OracleParameter("PO_BTSCODE", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETGETBTS_CODE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetBTSCode",
                    procedure_name = "BIA_GETGETBTS_CODE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetOfferId(string channelName, string userName, string bi_token_number)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = channelName },
                    new OracleParameter("P_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("P_BI_TOKEN", OracleDbType.Double, ParameterDirection.Input) { Value = Convert.ToDouble(bi_token_number) },
                    new OracleParameter("PO_OFFERID", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GET_OFFERIDBYCHANNEL", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetOfferId",
                    procedure_name = "GET_OFFERIDBYCHANNEL",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }


        #region ====================| FTR Restriction | ==================
        public async Task<string> GetRetailerItopUpNumber(string userName)
        {
            string msisdn = string.Empty;
            DataTable dataTable = new DataTable();

            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("PO_MSISDN", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                dataTable = await _oracleDataManagerV2.SelectProcedure("BIA_GETRETAILERMSISDN", parameters.ToArray());

                foreach (DataRow dtRow in dataTable.Rows)
                {
                    msisdn = dtRow["MOBILE_NUMBER"].ToString();
                }

                return msisdn;
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetRetailerItopUpNumber",
                    procedure_name = "BIA_GETRETAILERMSISDN",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }
        }

        public async Task FTR_UpdateData(FTRDBUpdateModel model)
        {
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_BI_TOKEN_NUMBER", OracleDbType.Long, ParameterDirection.Input) { Value = model.bi_token_no },
                    new OracleParameter("P_ISFTR_RESTRICTED", OracleDbType.Int32, ParameterDirection.Input) { Value = model.is_ftr_restricted },
                    new OracleParameter("P_FTR_MESSAGE", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.ftr_message }
                };
                
                long? log_id = await _oracleDataManagerV2.CallInsertProcedure("BIA_FTR_RESTRICTIONUPD", parameters.ToArray());
                if (log_id < 1)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "FTR_UpdateData",
                    procedure_name = "BIA_FTR_RESTRICTIONUPD",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }
        }
        #endregion

        public async Task<DataTable> GetBlackListedWordForAddress()
        {
            DataTable result;
            OracleDataManager _odm = new OracleDataManager();
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_BLACKLISTEDWORD", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETBLACKLISTED_ADDR", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    //request_model = Convert.ToString(model),
                    method_name = "GetBlackListedWordForAddress",
                    procedure_name = "BIA_GETBLACKLISTED_ADDR",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetBlackListedWordForName()
        {
            DataTable result;
            OracleDataManager _odm = new OracleDataManager();
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_BLACKLISTEDWORD", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETBLACKLISTED_NAME", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetBlackListedWordForName",
                    procedure_name = "BIA_GETBLACKLISTED_NAME",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> CheckUser(UserCheckModel userModel)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.user_name },
                    new OracleParameter("PI_BP_MSISDN", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.bpmsisdn },
                    new OracleParameter("PO_USER", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("CHECK_USER_STATUS", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "CheckUser",
                    procedure_name = "CHECK_USER_STATUS",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<DataTable> FetchFingerPrint(FPValidationReqModel userModel)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.user_name },
                    new OracleParameter("PO_FINGERPRINT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GET_FINGERPRINT", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "FetchFingerPrint",
                    procedure_name = "GET_FINGERPRINT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<long?> SaveFingerPrint(FPRegistrationModel userModel)
        {
            long? result = 0;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.user_name },
                    new OracleParameter("PI_RIGHT_THUMB", OracleDbType.Clob, ParameterDirection.Input) { Value = userModel.right_thumb },
                    new OracleParameter("PI_RIGHT_THUMB_SCORE", OracleDbType.Int32, ParameterDirection.Input) { Value = userModel.right_thumb_score },
                    new OracleParameter("PI_RIGHT_INDEX", OracleDbType.Clob, ParameterDirection.Input) { Value = userModel.right_index },
                    new OracleParameter("PI_RIGHT_INDEX_SCORE", OracleDbType.Int32, ParameterDirection.Input) { Value = userModel.right_index_score },
                    new OracleParameter("PI_LEFT_THUMB", OracleDbType.Clob, ParameterDirection.Input) { Value = userModel.left_thumb },
                    new OracleParameter("PI_LEFT_THUMB_SCORE", OracleDbType.Int32, ParameterDirection.Input) { Value = userModel.left_thumb_score },
                    new OracleParameter("PI_LEFT_INDEX", OracleDbType.Clob, ParameterDirection.Input) { Value = userModel.left_index },
                    new OracleParameter("PI_LEFT_INDEX_SCORE", OracleDbType.Int32, ParameterDirection.Input) { Value = userModel.left_index_score },
                    new OracleParameter("PI_MOBILE_NO", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userModel.mobile_no }
                };
                
                result = await _oracleDataManagerV2.CallInsertProcedure("SAVE_FINGERPRINT", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "SaveFingerPrint",
                    procedure_name = "SAVE_FINGERPRINT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<DataTable> GetFingerPrintResult(double? bi_token)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_BI_TOKEN", OracleDbType.Decimal, ParameterDirection.Input) { Value = bi_token },
                    new OracleParameter("PO_FINGERPRINT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GET_FINGERPRINT_RESULT", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetFingerPrintResult",
                    procedure_name = "GET_FINGERPRINT_RESULT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<DataTable> GetScannerInfo(ScannerInfoReqModel model)
        {
            DataTable result;
            OracleDataManager _odm = new OracleDataManager();
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_SCANNER_ID", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.scanner_id },
                    new OracleParameter("PO_CURSOR", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETSCANNERINFO", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetScannerInfo",
                    procedure_name = "BIA_GETSCANNERINFO",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        public async Task<DataTable> GetRetailerNIDDOB(string username)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USERNAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = username },
                    new OracleParameter("PO_NID_DOB", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GET_RETAILER_NIDDOB", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetRetailerNIDDOB",
                    procedure_name = "GET_RETAILER_NIDDOB",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<DataTable> GetIsRegistered(string userName)
        {
            DataTable result = null;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PI_USER_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = userName },
                    new OracleParameter("PO_ISREGISTERED", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("GET_ISREGISTERED", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetIsRegistered",
                    procedure_name = "GET_ISREGISTERED",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message);
            }
            return result;
        }

        public async Task<DataTable> GetUpdateAPKVersion()
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("PO_APPVERSION", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETAPPVERSION", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetUpdateAPKVersion",
                    procedure_name = "BIA_GETGETBTS_CODE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message,
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);
                throw new Exception(ex.Message);
            }

            return result;
        }

        #region Cherish Number Sell
        public async Task<DataTable> GetCherishCategoryData(string channelName)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = channelName },
                    new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETCATEGORIES", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetCherishCategoryData",
                    procedure_name = "BIA_GETCATEGORIES",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }
        public async Task<DataTable> GetDesiredCatMessage(string CategoryName, string channel_name)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CATEGORY_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = CategoryName },
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = channel_name },
                    new OracleParameter("PO_RESULT", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETCATEGORYMESSAGE", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetDesiredCatMessage",
                    procedure_name = "BIA_GETCATEGORYMESSAGE",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }

        public async Task<DataTable> GetSubscriptionsTypes(RASubscriptionTypeReq model)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CHANNEL_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = model.channel_name },
                    new OracleParameter("PO_PURS", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETSUBSCRIPTIONTYPES", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetSubscriptionsTypes",
                    procedure_name = "BIA_GETSUBSCRIPTIONTYPES",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());
            }

            return result;
        }

        public async Task<DataTable> GetCategoryMinAmount(string category)
        {
            DataTable result;
            try
            {
                List<OracleParameter> parameters = new List<OracleParameter>
                {
                    new OracleParameter("P_CATEGORY_NAME", OracleDbType.Varchar2, ParameterDirection.Input) { Value = category },
                    new OracleParameter("PO_PURS", OracleDbType.RefCursor, ParameterDirection.Output)
                };
                
                result = await _oracleDataManagerV2.SelectProcedure("BIA_GETCATEGORYAMOUNT", parameters.ToArray());
            }
            catch (Exception ex)
            {
                string? text = Convert.ToString(new
                {
                    request_time = DateTime.Now,
                    method_name = "GetCategoryMinAmount",
                    procedure_name = "BIA_GETCATEGORYAMOUNT",
                    error_source = ex.Source,
                    error_code = ex.HResult,
                    error_description = ex.Message
                });
                _logWriter.WriteDailyLog2(text == null ? "" : text);

                throw new Exception(ex.Message.ToString());

            }

            return result;
        }
        #endregion
    }
}