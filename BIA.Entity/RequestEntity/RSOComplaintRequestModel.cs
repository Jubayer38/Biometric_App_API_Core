namespace BIA.Entity.RequestEntity
{
    public class RSOComplaintRequestModel
    {
        public string userName { get; set; } = "";
        public string password { get; set; } = "";
        public string complaintType { get; set; } = "";
        public string complaintTitle { get; set; } = "";
        public string description { get; set; } = "";
        public string preferredLevel { get; set; } = "";
        public string preferredLevelName { get; set; } = "";
        public string preferredLevelContact { get; set; } = "";

        public decimal raiseComplaintID { get; set; } = 0;
        public string retailerCode { get; set; } = "";

    }
}
