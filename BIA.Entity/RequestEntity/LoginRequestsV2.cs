using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class LoginRequestsV2
    {
        /// <summary>
        /// User name for request
        /// </summary>
        [Required]
        public string UserName { get; set; }

        /// <summary>
        /// The authentication password
        /// </summary>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// Reseller's device(TAB) IMEI number.
        /// </summary>

        //[Required]
        public string? Lan { get; set; } = "en";
        /// <summary>
        /// Reseller app Apk version Code (i.e. 209). 
        /// </summary>
        [Required]
        public int VersionCode { get; set; } = 0;
        /// <summary>
        /// Reseller app Apk version name (i.e. "14.0.7").
        /// </summary>
        [Required]
        public string VersionName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //[Required]
        public int? Type { get; set; } = 0;
        /// <summary>
        /// Reseller app andriod operating sysytem's version (i.e. "4.4.2").
        /// </summary>
        //[Required]
        public string? OSVersion { get; set; } = "";
        /// <summary>
        /// Reseller app andriod operating sysytem's kernel version (i.e. "19").
        /// </summary>
        //[Required]
        public string? KernelVersion { get; set; } = "";
        /// <summary>
        /// Reseller app andriod operating sysytem's ferware version (i.e. "215.90.172.87").
        /// </summary>
        public string? FermwareVersion { get; set; }
        //public string InstalledApps { get; set; }
        public decimal? latitude { get; set; } = 0;
        public decimal? longitude { get; set; } = 0;
        public int? lac { get; set; } = 0;
        public int? cid { get; set; } = 0;
        public string? BPMSISDN { get; set; } = "";
        public string? DeviceModel { get; set; } = "";
        
        [CustomValidation(allowString: true, allowInt: true)]
        public object? DeviceId { get; set; }    
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CustomValidationAttribute : Attribute
    {
        public bool AllowString { get; }
        public bool AllowInt { get; }

        public CustomValidationAttribute(bool allowString = true, bool allowInt = true)
        {
            AllowString = allowString;
            AllowInt = allowInt;
        }
    }
}
