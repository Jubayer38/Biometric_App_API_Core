using BIA.DAL.Repositories;
using BIA.Entity.RequestEntity;
using BIA.Entity.ResponseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLRetailerUserSync
    {
        private readonly DALBiometricRepo _dALBiometric;

        public BLLRetailerUserSync(DALBiometricRepo dALBiometric)
        {
            _dALBiometric = dALBiometric;
        }
        public async Task<decimal?> UpdateRetailerUserByDMS(DMSRetailerReqModel model)
        {
            decimal? result = 0;
            try
			{
                result = await _dALBiometric.UpdateRetailerUserByDMS(model);
			}
			catch (Exception)
			{
				throw;
			}
            return result;
        }
    }
}
