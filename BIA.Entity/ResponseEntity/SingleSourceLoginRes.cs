namespace BIA.Entity.ResponseEntity
{
    public class SingleSourceLoginRes
    {
        public bool is_success { get; set; }
        public string session_token { get; set; }
        public string message { get; set; }
    }
    public class SingleSourceRes
    {
        public bool is_success { get; set; }
        public string message { get; set; }
        public SingleSourceData Data { get; set; }
    }
    public class SingleSourceData
    {
        public string msisdn { get; set; }
        public string imsi { get; set; }
        public string nid { get; set; }
        public string dob { get; set; }
        public string reseller_code { get; set; }
        public bool is_active { get; set; }

    }
}
