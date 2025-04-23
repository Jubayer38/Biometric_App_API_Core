using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class RAOrderRequestUpdate : RACommonRequest
    {
        public string? bss_reqId { get; set; }
        public int? status { get; set; }
        public long? error_id { get; set; }
        public string? err_msg { get; set; }

        public double? bi_token_number { get; set; }
        public string? dest_imsi { get; set; }
        /// <summary>
        /// Msisdn Reservation Id 
        /// </summary>
        public string? msisdnReservationId { get; set; }
        public string? msidn { get; set; }
        public string? user_name { get; set; }

    }
}
