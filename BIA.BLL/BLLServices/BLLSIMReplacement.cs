using BIA.DAL.Repositories;
using BIA.Entity.CommonEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLSIMReplacement
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLSIMReplacement(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<List<SIMReplacementReasonModel>> GetSIMReplacementReasons()
        {
            var dataRow = await _dataManager.GetSIMReplacementReasons();
            List<SIMReplacementReasonModel> srrs = new List<SIMReplacementReasonModel>();

            for (int i = 0; i < dataRow.Rows.Count; i++)
            {

                SIMReplacementReasonModel srr = new SIMReplacementReasonModel();
                srr.id = Convert.ToInt32(dataRow.Rows[i]["ID"] == DBNull.Value ? null : dataRow.Rows[i]["ID"]);
                srr.reason = Convert.ToString(dataRow.Rows[i]["REASON"] == DBNull.Value ? null : dataRow.Rows[i]["REASON"]);
                srrs.Add(srr);
            }
            return srrs;
        }
    }
}
