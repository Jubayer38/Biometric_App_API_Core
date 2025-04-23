using BIA.BLL.BLLServices;
using BIA.Common;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using BIA.JWT;
using Microsoft.AspNetCore.Mvc;
using static BIA.Common.ModelValidation;

namespace BIA.Controllers 
{
    [Route("api/Resubmit")]
    [ApiController]
    public class ResubmitController : ControllerBase
    {
        private readonly BLLResubmit _resubmitManager;
        private readonly BLLLog _bllLog;

        public ResubmitController(BLLResubmit resubmitManager, BLLLog bllLog)
        {
            _resubmitManager = resubmitManager;
            _bllLog = bllLog;
        }

        [HttpPost]
        [Route("ReSubmitOrder")]
        public async Task<IActionResult> ReSubmitOrder(ResubmitReqModel model)
        {
            ModelValidation modelValidation = new ModelValidation();
            ValidTokenResponse security = new ValidTokenResponse();
            ResubmitResponseModel response = new ResubmitResponseModel();
            try
            {
                string secreteKey = string.Empty;
                string loginProviderId = string.Empty;

                secreteKey = SettingsValues.GetJWTSequrityKey();

                TokenValidationService token = new TokenValidationService(secreteKey);

                security = token.ValidateToken(model.session_token);

                if (security != null)
                {
                    if (security.IsVallid == true)
                    {
                        loginProviderId = security.LoginProviderId;
                        model.distributor_code = security.DistributorCode;
                    }
                    else
                    {
                        throw new Exception(security.Message);
                    }
                }

                var validateResponse = modelValidation.OrderReSubmitModelValidation(new ValidationPropertiesResubmitModel
                {
                    bi_token_number = model.bi_token_number,
                });

                if (!validateResponse.result)
                {
                    return Ok(new RACommonResponseRevamp()
                    {
                        isError = true,
                        message = validateResponse.message,
                        data = new Datas()
                        {
                            isEsim = 0,
                            request_id = " "
                        }
                    });
                }

                response = await _resubmitManager.GetResubmitOrderInfo(model);

            }
            catch (Exception ex)
            {

                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    response.data = new ResubmitResponseModelData()
                    {

                    };
                    response.isError = true;
                    response.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    response.data = new ResubmitResponseModelData()
                    {

                    };
                    response.isError = true;
                    response.message = ex.Message;
                }

            }

            return Ok(response);

        }
    }
}
