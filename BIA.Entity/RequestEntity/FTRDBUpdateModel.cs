using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class FTRDBUpdateModel
    {
        public long bi_token_no { get; set; }
        public int is_ftr_restricted { get; set; }
        public string ftr_message { get; set; }
    }
}
