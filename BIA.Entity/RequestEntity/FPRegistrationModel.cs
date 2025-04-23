using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class FPRegistrationModel
    {
        public string user_name { get; set; }
        public string left_thumb { get; set; }
        public int left_thumb_score { get; set; }
        public string left_index { get; set; }
        public int left_index_score { get; set; }
        public string right_thumb { get; set; }
        public int right_thumb_score { get; set; }
        public string right_index { get; set; }
        public int right_index_score { get; set; }
        public string mobile_no { get; set; }
        public string session_token { get; set; }
    }
}
