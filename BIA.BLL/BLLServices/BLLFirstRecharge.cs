using BIA.DAL.Repositories;

namespace BIA.BLL.BLLServices
{
    public class BLLFirstRecharge
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLFirstRecharge(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public decimal UpdateOrderFirstRechargeStatus(string requestId)
        {
            decimal response = 0;
            try
            {
                decimal bi_token_number = String.IsNullOrEmpty(requestId) ? 0 : Convert.ToDecimal(requestId.Trim());
                response = _dataManager.UpdateOrderFirstRechargeStatus(bi_token_number);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return response;
        }
    }
}
