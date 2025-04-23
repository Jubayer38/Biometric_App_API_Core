namespace BIA.Entity.ResponseEntity
{
    public class ComplaintResponse
    {
        public decimal complaint_id { get; set; }
        public bool is_success { get; set; }
        public string message { get; set; }
    }
}
