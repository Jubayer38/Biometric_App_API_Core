using BIA.Entity.DB_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    /// <summary>
    /// Division-district-thana api response type.
    /// </summary>
    public class DivDisThanaResponse
    {
        /// <summary>
        /// Contains a list type division, district, thana data. 
        /// </summary>
        public object data { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>
        public bool result { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }


    public class DivDisThanaResponse2
    {
        public List<DivisionModel> data { get; set; }
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class DivDisThanaResponseRevamp
    { 
        /// <summary>
        /// Contains a list type division, district, thana data. 
        /// </summary>
        public List<DivisionModelV2> data { get; set; }
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary> 
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
    }
}
