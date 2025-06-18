using BIA.BLL.BLLServices;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.JWT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BIA.Controllers
{
    [Route("api/RaiseComplaint")]
    [ApiController] 
    public class RaiseComplaintController : ControllerBase
    {
        private readonly BLLRaiseComplaint _complaintManager;
        private readonly BaseController _bio;
        private readonly BLLLog _bllLog;
        private readonly IConfiguration _configuration;

        public RaiseComplaintController(BLLRaiseComplaint complaintManager, BaseController bio, BLLLog bllLog, IConfiguration configuration)
        {
            _complaintManager = complaintManager;
            _bio = bio;
            _bllLog = bllLog;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("ComplaintSubmit")]
        public async Task<IActionResult> RaiseComplaintSubmit(ComplaintReqModel reqModel)
        {
            ValidTokenResponse security = new ValidTokenResponse();
            BLLRAToDBSSParse dBSSParse = new BLLRAToDBSSParse();
            ComplaintResponseModel? apiResponse = new ComplaintResponseModel();
            BL_Json _blJson = new BL_Json();
            BIAToRaiseComplainLog log = new BIAToRaiseComplainLog();
            string apiUrl = RSOAPI.ComplaintAPI;
            string responseContent = String.Empty;
            ErrorDescription error = new ErrorDescription();
            ComplaintResponseModel complaintResponse = new ComplaintResponseModel();
            try
            {
                SubmitComplaintModel model = new SubmitComplaintModel
                {
                    session_token = reqModel.session_token,
                    retailerCode = reqModel.retailerCode,
                    description = reqModel.description,
                    userName = SettingsValues.GetRSOAppUserName(),
                    password = _configuration.GetSection("AppSettings:rso_app_pas").Value,
                    complaintType = _configuration.GetSection("AppSettings:complaint_type").Value,
                    complaintTitle = _configuration.GetSection("AppSettings:complaint_title").Value,
                    preferredLevel = _configuration.GetSection("AppSettings:preferred_level").Value,
                    preferredLevelName = _configuration.GetSection("AppSettings:preferred_level_name").Value,
                    preferredLevelContact = _configuration.GetSection("AppSettings:preferrred_level_contact").Value
                };

                #region Insert_Complaint_In_DB

                var res = await _complaintManager.SubmitComplaint(model);

                if (!res.is_success)
                {
                    return Ok(new ComplaintResponseModel()
                    {
                        isError = true,
                        message = res.message
                    });
                }
                #endregion

                #region Submit_Complaint_To_RSO

                model.raiseComplaintID = res.complaint_id;

                log.complaint_id = res.complaint_id;
                log.user_id = reqModel.retailerCode;

                RSOComplaintRequestModel rsoReqModel = dBSSParse.ComplaintReqPargeModel(model);

                log.req_blob = _blJson.GetGenericJsonData(rsoReqModel);

                string jsonData = JsonConvert.SerializeObject(rsoReqModel);

                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;
                    responseContent = response.Content.ReadAsStringAsync().Result;
                }

                apiResponse = JsonConvert.DeserializeObject<ComplaintResponseModel>(responseContent);
                
                if (apiResponse != null && apiResponse.isError)
                {
                    return Ok(new ComplaintResponseModel()
                    {
                        isError = true,
                        message = apiResponse.message
                    });
                }

                log.res_blob = _blJson.GetGenericJsonData(apiResponse);

                #region Update_Bi_Request_Raise_Complaint_Flag
                decimal responseId = 0;
                if (!String.IsNullOrEmpty(reqModel.bi_token_number)){
                    responseId = await _complaintManager.UpdateOrderComplaintStatus(reqModel.bi_token_number);
                }    
                #endregion
                return Ok(new ComplaintResponseModel()
                {
                    isError = false,
                    message = apiResponse != null ? apiResponse.message : " "
                });
                #endregion
            }
            catch (Exception ex)
            {
                log.res_time = DateTime.Now;
                log.is_success = 0;
                log.res_blob = _blJson.GetGenericJsonData(ex.Message);
                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    complaintResponse.isError = true;

                    complaintResponse.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                    log.error_code = error.error_code ?? String.Empty;
                    log.error_source = error.error_source ?? String.Empty;
                    log.message = error.error_description ?? String.Empty;

                    return Ok(complaintResponse);
                }
                catch (Exception)
                {
                    complaintResponse.isError = true;
                    complaintResponse.message = ex.Message;

                    return Ok(complaintResponse);
                }
            }
            finally
            {
                await _bllLog.RaiseCoplainLog(log);
            }
        }
    }
}
