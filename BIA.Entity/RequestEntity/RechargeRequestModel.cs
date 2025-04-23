namespace BIA.Entity.RequestEntity
{
    public class RechargeRequestModel
    {
        public string session_token { get; set; }
        public string? sessionToken { get; set; }
        public string retailerCode { get; set; }
        public string subscriberNo { get; set; }
        public string amount { get; set; }
        public string userPin { get; set; }
        public string deviceId { get; set; }
        public int? paymentType { get; set; }
        public double? lat { get; set; }
        public double? lng { get; set; }
        public string? lan { get; set; }
        public string? userId { get; set; } = "0";
        public string bi_token_number { get; set; }
    }

    public class RechargeReqModel
    {
        public string sessionToken { get; set; }
        public string retailerCode { get; set; }
        public string subscriberNo { get; set; }
        public string amount { get; set; }
        public string userPin { get; set; }
        public string deviceId { get; set; }
        public int? paymentType { get; set; }
        public double? lat { get; set; }
        public double? lng { get; set; }
        public string? lan { get; set; }
    }
}
