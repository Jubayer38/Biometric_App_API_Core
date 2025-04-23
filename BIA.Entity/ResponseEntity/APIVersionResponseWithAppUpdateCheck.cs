using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class APIVersionResponse : RACommonResponse
    {
        public int api_version { get; set; }
    }

    public class APIVersionResponseRev
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
        public APIVersionData data { get; set; }
    }

    public class APIVersionData
    {
        public int api_version { get; set; }

    }

    public class APIVersionResponseWithAppUpdateCheckRev     
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// Type contains reseller app apk info.
        /// </summary>
        public AppUpdateInfoV2 data { get; set; }
    }


    /// <summary>
    /// API version and reseller app apk version return type.
    /// </summary>
    public class APIVersionResponseWithAppUpdateCheck : RACommonResponse
    {
        /// <summary>
        /// Deafult constructor 
        /// </summary>
        public APIVersionResponseWithAppUpdateCheck()
        {
            this.app_update_info = new AppUpdateInfo();
        }

        /// <summary>
        /// Reseller app api server version. (i.e. 2) 
        /// </summary>
        public int api_version { get; set; }
        /// <summary>
        /// Type contains reseller app apk info.
        /// </summary>
        public AppUpdateInfo app_update_info { get; set; }
    }


    /// <summary>
    /// Type contains reseller app apk info.
    /// </summary>
    public class AppUpdateInfoV2
    {
        /// <summary>
        /// Reseller app api server version. (i.e. 2) 
        /// </summary>
        public int api_version { get; set; }

        /// <summary>
        /// Contains true or false. If there exist apk update then contains true, other wise false. 
        /// </summary>
        public bool is_update_exists { get; set; }
        /// <summary>
        /// Contains true or false. If the apk update is mendatory for all user(reseller) then in contains 
        /// true, other wise false.
        /// </summary>
        public int is_update_mandatory { get; set; }
        /// <summary>
        /// Contains URL, from updated apk can be downloaded.
        /// </summary>
        public string update_url { get; set; }
    }
    public class AppUpdateInfo
    {
        /// <summary>
        /// Contains true or false. If there exist apk update then contains true, other wise false. 
        /// </summary>
        public bool is_update_exists { get; set; }
        /// <summary>
        /// Contains true or false. If the apk update is mendatory for all user(reseller) then in contains 
        /// true, other wise false.
        /// </summary>
        public int is_update_mandatory { get; set; }
        /// <summary>
        /// Contains URL, from updated apk can be downloaded.
        /// </summary>
        public string update_url { get; set; }
    }
}
