using BIA.Entity.CommonEntity;
using System.ComponentModel.DataAnnotations;

namespace BIA.Entity.RequestEntity
{
    public class ComplaintReqModel //: RACommonRequest
    {
        [Required]
        public string description { get; set; }
        [Required]
        public string retailerCode { get; set; }

        public string? bi_token_number { get; set; } = "";
        public string? session_token { get; set; } = "";
    }

    public class SubmitComplaintModel : ComplaintReqModel
    {
        public string userName { get; set; } = "";
        public string password { get; set; } = "";
        public string complaintType { get; set; } = "";
        public string complaintTitle { get; set; } = "";
        public string preferredLevel { get; set; } = "";
        public string preferredLevelName { get; set; } = "";
        public string preferredLevelContact { get; set; } = "";
        public decimal raiseComplaintID { get; set; } = 0;

    }
}
