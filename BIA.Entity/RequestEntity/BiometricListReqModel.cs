using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class BiometricListReqModel
    {
        public int bio_flag { get; set; }
        public int row_no { get; set; }
        public int bio_status { get; set; }
        public int max_thread_count { get; set; }
        public int max_thread_sleep_time { get; set; }
        public int runnung_count { get; set; }
    }
}
