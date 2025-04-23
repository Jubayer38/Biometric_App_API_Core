namespace BIA.Entity.ResponseEntity
{
    public class ComplaintResponseModel
    {
        public bool isError { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
    public class ComplaintResp
    {
        public bool result { get; set; }
        public string message { get; set; }

    }
}
