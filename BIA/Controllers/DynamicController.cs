using BIA.BLL.BLLServices;
using BIA.Entity.Collections;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.ResponseEntity;
using BIA.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BIA.Controllers
{
    [Route("api/Dynamic")]
    [ApiController]
    public class DynamicController : ControllerBase
    {
        private readonly BLLDynamic _bLLDynamic;
        private readonly BLLLog _bllLog;

        public DynamicController(BLLDynamic bLLDynamic, BLLLog bllLog)
        {
            _bLLDynamic = bLLDynamic;
            _bllLog = bllLog;
        }

        [HttpPost]
        [Route("GetHelpData")]
        public async Task<IActionResult> GetHelpData()
        {
            HelpButtonRespModel respModel = new HelpButtonRespModel(); 
            try
            {                
                List<UserType> userTypeList = await _bLLDynamic.GetUserTypeDropdownValu();
                List<ContentType> ContentList = await _bLLDynamic.GetContentTypeDropdownValue();
                List<ContentUrl> contentUrls = await _bLLDynamic.GetContentURL();

                foreach (UserType item in userTypeList) 
                {
                    item.contentTypes = ContentList.Where(a => a.UserTypeId == item.UserTypeId);

                    foreach (ContentType item2 in item.contentTypes)
                    {
                        item2.contentUrl = contentUrls.Where(a => a.userTypeId == item2.UserTypeId);
                    }
                }
                respModel.data = userTypeList;
                respModel.isError = false;
                respModel.message = MessageCollection.Success;
            }
            catch (Exception ex)
            {
                ErrorDescription error;

                try
                {
                    error = await _bllLog.ManageException(ex.Message, ex.HResult, "BIA");

                    respModel.data = null;
                    respModel.isError = true;
                    respModel.message = String.IsNullOrEmpty(error.error_custom_msg) ? error.error_description : error.error_custom_msg;
                }
                catch (Exception)
                {
                    respModel.isError = true;
                    respModel.message = ex.Message;
                }
            }
            return Ok(respModel);
        }

    }
}
