using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class PackagesResponse
    {
        //public List<PackagesReponseData> data { get; set; 
        public object data { get; set; }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class PackagesReponseData
    {
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string package_id { get; set; }
        public string package_name { get; set; } // this filed is basically "package_code" For send order purpose we send package code.
        //public string package_code { get; set; }
    }

    public class PackagesResponseRev
    {
        //public List<PackagesReponseData> data { get; set; 
        public List<PackagesReponseDataRev> data { get; set; }
        public bool isError { get; set; }
        public string message { get; set; }
    }

    public class PackagesReponseDataRev
    {
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string package_id { get; set; }
        public string package_name { get; set; } // this filed is basically "package_code" For send order purpose we send package code.
        //public string package_code { get; set; }
    }
}
