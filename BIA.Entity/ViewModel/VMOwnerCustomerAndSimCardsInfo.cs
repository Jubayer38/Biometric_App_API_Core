using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ViewModel
{
    public class VMOwnerCustomerAndSimCardsInfo
    {
        /// <summary>
        /// Customer's (parent) NID.
        /// </summary>
        public string doc_id_number { get; set; }
        /// <summary>
        /// Customer's (parent) DOB.
        /// </summary>
        public string dob { get; set; }
        /// <summary>
        /// Customer's () old SIM number.
        /// </summary>
        public string old_sim_number { get; set; }
        /// <summary>
        /// Customer's () old SIM type (i.e. Prepaid = 1, Postpaid = 2).
        /// </summary>
        public string old_sim_type { get; set; }
        /// <summary>
        /// DBSS subacription type.
        /// </summary>
        public long dbss_subscription_id { get; set; }
    }
}
