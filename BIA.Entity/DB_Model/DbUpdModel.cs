using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class DbUpdModel
    {
        public string bi_token_number { get; set; }
        public string bss_req_id { get; set; }
        public int status { get; set; }
        public DateTime uddate_date { get; set; }
        public string confirmation_code { get; set; }
    }
}
