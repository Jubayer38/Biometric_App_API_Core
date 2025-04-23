using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.RequestEntity
{
    /// <summary>
    /// Biometric status update request type.
    /// </summary>
    public class BIAFinishNotiRequest : IValidatableObject
    {
        /// <summary>
        /// Security token
        /// </summary>
        [Required]
        public string session_token { get; set; }
        /// <summary>
        /// DBSS bio request id / DBSS request id.
        /// </summary>
        [Required]
        public string bio_request_id { get; set; }//in DB it is named as BSS_REQUEST_ID.
        /// <summary>
        /// Is biometric verification succeed or not.
        /// </summary>
        [Required, Range(0, 1, ErrorMessage = "Only 0 or 1 is acceptable. 1 for success and 0 for failure.")]
        public int? is_Success { get; set; }
        /// <summary>
        /// Bio verification failure error code.
        /// </summary>
        public string? error_code { get; set; } = "";
        /// <summary>
        /// Bio verification failure error details.
        /// </summary>
        public string? description { get; set; } = "";
        /// <summary>
        /// Source of bio failure error.
        /// </summary>
        public string? error_source { get; set; } = "";


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (is_Success == 0
                && String.IsNullOrEmpty(description))
                yield return new ValidationResult("'description' field is required when the result of 'is_Success' is 0.");
        }
    }
}
