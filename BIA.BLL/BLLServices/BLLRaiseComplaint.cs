using BIA.DAL.Repositories;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;

namespace BIA.BLL.BLLServices
{
    public class BLLRaiseComplaint
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLRaiseComplaint(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<ComplaintResponse> SubmitComplaint(SubmitComplaintModel model)
        {
            ComplaintResponse response = new ComplaintResponse();
            try
            {
                decimal complaintId = await _dataManager.SubmitComplaint(model);
                if (complaintId > 0)
                {
                    response.is_success = true;
                    response.complaint_id = complaintId;
                    response.message = "Successfully Submitted Complaint.";
                }
                else
                {
                    response.is_success = false;
                    response.complaint_id = complaintId;
                    response.message = "Could not submit Complaint.Please try again";

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return response;
        }

        public async Task<decimal> UpdateOrderComplaintStatus(string requestId)
        {
            decimal response = 0;
            try
            {
                decimal bi_token_number = String.IsNullOrEmpty(requestId) ? 0 : Convert.ToDecimal(requestId.Trim());
                response = await _dataManager.UpdateOrderComplaintStatus(bi_token_number);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return response;
        }
    }
}
