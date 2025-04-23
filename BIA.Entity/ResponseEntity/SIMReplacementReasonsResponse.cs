using BIA.Entity.CommonEntity;

namespace BIA.Entity.ResponseEntity
{
    public class SIMReplacementReasonsResponse
    {
        public List<SIMReplacementReasonModel> data { get; set; } //= new List<SIMReplacementReasonModel>();
        public bool result { get; set; }
        public string message { get; set; }
    }

    public class SIMReplacementReasonsResponseRevamp
    {
        // public List<SIMReplacementReasonModel> data { get; set; } //= new List<SIMReplacementReasonModel>();
        public bool isError { get; set; }
        /// <summary>
        /// Data contains api request result's message (i.e. "Success", "Security token invalid!")
        /// </summary>
        public string message { get; set; }

        public List<SIMReplacementReasonModel> data { get; set; }
    }

}
