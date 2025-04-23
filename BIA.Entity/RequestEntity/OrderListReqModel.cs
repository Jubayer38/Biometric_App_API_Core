using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class OrderListReqModel
    {
        public int order_flag { get; set; }
        public int max_row { get; set; }
        public int order_staus { get; set; }
        public int max_thread_count { get; set; }
        public int max_thread_sleep_time { get; set; }
        public int runnung_count { get; set; }
        public int create_customer_retry { get; set; }
    }
}
