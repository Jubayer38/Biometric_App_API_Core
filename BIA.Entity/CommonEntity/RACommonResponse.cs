using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.CommonEntity
{
    public class RACommonResponse //: ResponseData
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }

    public class RACommonResponseRevamp //: ResponseData
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>  
        public bool isError { get; set; } 
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public Datas data { get; set; }
    }

    public class RACommonResponseRevampResp2 //: ResponseData
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>  
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
        public string reservationId { get; set; }

        public Datas data { get; set; }
    }

    public class RACommonResponseRevampV3 
    {/// <summary>
     /// Data contains if api request success or not!
     /// </summary>  
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
        public DesiredCategoryData data { get; set; } = new DesiredCategoryData();
    }

    public class DesiredCategoryData
    {
        public string message { get; set; }
        public bool isDesiredCategory { get; set; }
        public string category { get; set; }
    }

    public class Datas
    {
        public int isEsim { get; set; }
        /// <summary>
        /// Order submit request Id. 
        /// </summary>
        public string request_id { get; set; }
        public string reservation_id { get; set; }

    }

    public class RACommonResponseRetailLoginUpdateToken //: ResponseData
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>  
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public SessionForRetailToBiometric data { get; set; }
    }
    public class SessionForRetailToBiometric
    {
        public string session_token { get; set; }
    } 
}
