using BIA.BLL.BLLServices;
using BIA.BLL.Utility;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.Entity.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BIA.Controllers
{
    [Route("api/RetailerSync")]
    [ApiController]
    public class RetailerSyncController : ControllerBase
    {
        private readonly BLLRetailerUserSync _userSync;
        private readonly BLLLog _bllLog;

        public RetailerSyncController(BLLRetailerUserSync userSync, BLLLog bllLog)
        {
            _userSync = userSync;
            _bllLog = bllLog;
        }

        [HttpPost]
        [Route("PostRetailerStatus")]
        public async Task<IActionResult> PostRetailerStatus([FromBody] DMSRetailerReqModel model)
        {
            AESCryptography aESCryptography = new AESCryptography();
            BIAToDBSSLog log = new BIAToDBSSLog();
            BL_Json bL_Json = new BL_Json();
            DMSRetailerResponseModel respModel = new DMSRetailerResponseModel();
            try
            {
                string biometricUserName = string.Empty;
                string biometricPassword = string.Empty;
                string userName = string.Empty;
                string password = string.Empty;
                decimal? result = 0;
                log.req_blob = bL_Json.GetGenericJsonData(model);
                log.req_time = DateTime.Now;

                try
                {
                    var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                    biometricUserName = SettingsValues.GetUserStatusUpdateUserName();
                    biometricPassword = SettingsValues.GetUserStatusUpdatePassword(); 
                }
                catch
                { }

                userName = AESCryptography.Encrypt(model.userName);
                password = AESCryptography.Encrypt(model.password);


                if (userName == biometricUserName && password == biometricPassword)
                {
                    result = await _userSync.UpdateRetailerUserByDMS(model);

                    log.res_blob = bL_Json.GetGenericJsonData(result);
                    log.res_time = DateTime.Now;

                    if (result > 0)
                    {
                        log.is_success = 1;
                        respModel.is_success = true;
                        respModel.message = "Successfully Updated " + model.retailerCode + " !";
                        return Ok(respModel);
                    }
                    else
                    {
                        log.is_success = 0;
                        respModel.is_success = false;
                        respModel.message = "Error occured!";
                        return Ok(respModel);
                    }
                }
                else
                {
                    log.is_success = 0;
                    respModel.is_success = false;
                    respModel.message = "Invalid User credentials!";
                    return Ok(respModel);
                }
            }
            catch (Exception ex)
            {
                log.res_blob = bL_Json.GetGenericJsonData(ex.Message.ToString());
                log.res_time = DateTime.Now;
                return Ok(new DMSRetailerResponseModel()
                {
                    is_success = false,
                    message = ex.Message
                });                
            }
            finally
            {
                log.res_blob = bL_Json.GetGenericJsonData(respModel);
                log.msisdn = _bllLog.FormatMSISDN(model.iTopUpNumber);

                string retailerId = string.Empty;
                if (model.retailerCode.Substring(0, 1) == "R")
                {
                    retailerId = model.retailerCode.Substring(1);
                }                
                log.user_id = retailerId;
                log.method_name = "PostRetailerStatus";

                await _bllLog.RAToDBSSLogV2(log, "", "");

            }
        }
    }
}
