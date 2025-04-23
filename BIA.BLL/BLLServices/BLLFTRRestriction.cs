using BIA.DAL.Repositories;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLFTRRestriction
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLFTRRestriction(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<string> GetRetailerItopUpNumber(string userName)
        {
            try
            {
                return await _dataManager.GetRetailerItopUpNumber(userName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void FTR_UPdateData(FTRDBUpdateModel model)
        {
            _dataManager.FTR_UpdateData(model);
        }
    }    
}
