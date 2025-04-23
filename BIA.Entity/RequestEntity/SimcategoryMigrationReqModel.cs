using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class SimcategoryMigrationReqModel
    {
        public SimcategoryMigrationData data { get; set; }
    }
    public class SimcategoryMigrationData
    {
        public string type { get; set; }
        public string id { get; set; }
        [JsonProperty(PropertyName = "biometric-request")]
        public string biometric_request { get; set; }
        public SimcategoryMigrationMeta meta { get; set; }
    }
    public class SimcategoryMigrationMeta
    {
        [JsonProperty(PropertyName = "change-date")]
        public string change_date { get; set; }
        [JsonProperty(PropertyName = "send-sms")]
        public bool send_sms { get; set; }
        public string channel { get; set; }
        public List<Packages> packages { get; set; }
    }
    public class Packages
    {
        public string name { get; set; }
    }


    public class SimcategoryMigrationReqModelWithoutPackage
    {
        public SimcategoryMigrationWithoutPackageData data { get; set; }
    }
    public class SimcategoryMigrationWithoutPackageData
    {
        public string type { get; set; }
        public string id { get; set; }
        [JsonProperty(PropertyName = "biometric-request")]
        public string biometric_request { get; set; }
        public SimcategoryMigrationWithoutPackageMeta meta { get; set; }
    }
    public class SimcategoryMigrationWithoutPackageMeta
    {
        [JsonProperty(PropertyName = "change-date")]
        public string change_date { get; set; }
        [JsonProperty(PropertyName = "send-sms")]
        public bool send_sms { get; set; }
        public string channel { get; set; } = "";
        public string[] packages { get; set; }
    }
}
