using BIA.Entity.CommonEntity;
using BIA.Entity.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class ActivityLogResponse : RACommonResponse
    {
        public ActivityLogResponse()
        {
            this.data = new List<VMActivityLog>();
        }
        public List<VMActivityLog> data { get; set; }
    }
    public class ActivityLogResponseRevamp
    {
        /// <summary>
        /// Data contains if api request success or not!
        /// </summary>  
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }
        public List<VMActivityLogRevamp> data { get; set; }
    }

}
