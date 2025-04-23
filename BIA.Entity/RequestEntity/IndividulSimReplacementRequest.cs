using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    public class IndividulSimReplacementRequest
    {
        /// <summary>
        /// Frequent Number.
        /// </summary>
        public int frequent_number { get; set; }
        /// <summary>
        /// The Matching Score of Finger Print.
        /// </summary>
        public int fingerprint_score { get; set; }
        /// <summary>
        /// SIM munber is an unique number which is always paired with a MSISDN.
        /// </summary>
        public int sim_number { get; set; }
        /// <summary>
        /// Is Last Balance.
        /// </summary>
        public int is_last_balance { get; set; }
        /// <summary>
        /// Is FnF
        /// </summary>
        public int is_fnf { get; set; }
        /// <summary>
        /// Date of Birth.
        /// </summary>
        public DateTime DOB { get; set; }
        /// <summary>
        /// Token Sub Type.
        /// </summary>
        public int token_sub_type { get; set; }
        /// <summary>
        /// Last Recharge.
        /// </summary>
        public int last_recharge { get; set; }
        /// <summary>
        /// Token Type.
        /// </summary>
        public int token_type { get; set; }
        /// <summary>
        /// The Matching Score of Left Thumb Finger Print.
        /// </summary>
        public int left_thumb_score { get; set; }
        /// <summary>
        /// Last Balance.
        /// </summary>
        public int last_balance { get; set; }
        /// <summary>
        /// The Right Thumb Finger Print as Binary Data Converted to String
        /// </summary>
        public string right_thumb { get; set; }
        /// <summary>
        /// Is Frequent Number.
        /// </summary>
        public int is_frequent_number { get; set; }
        /// <summary>
        /// The Left Thumb Finger Print as Binary Data Converted to String
        /// </summary>
        public string left_thumb { get; set; }
        /// <summary>
        /// Is NID Verified.
        /// </summary>
        public int is_nid_verified { get; set; }
        /// <summary>
        /// Is Last Recharge.
        /// </summary>
        public int is_last_recharge { get; set; }
        /// <summary>
        /// The Matching Score of Left Index Finger Print .
        /// </summary>
        public int left_index_score { get; set; }
        /// <summary>
        /// The Matching Score of Right Thumb Finger Print .
        /// </summary>
        public int right_thumb_score { get; set; }
        /// <summary>
        ///The Right Index Finger Print as Binary Data Converted to String
        /// </summary>
        public string right_index { get; set; }
        /// <summary>
        /// Mobile Number(MSISDN).
        /// </summary>
        public string mobile_number { get; set; }
        /// <summary>
        /// FnF
        /// </summary>
        public int fnf { get; set; }
        /// <summary>
        /// The Matching Score of Right Index Finger Print .
        /// </summary>
        public int right_index_score { get; set; }
        /// <summary>
        /// The Left Index Finger Print as Binary Data Converted to String.
        /// </summary>
        public int left_index { get; set; }
        /// <summary>
        /// Customer's NID (Bangladeshi) which is unique and identical for each user.
        /// </summary>
        public int nid { get; set; }

    }
}
