using BIA.DAL.Repositories;
using BIA.Entity.DB_Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BllHandleException
    {
        private readonly DALBiometricRepo _dataManager;

        public BllHandleException(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<ErrorDescription> ManageException(string message, int code, string errorSource)
        {
            DataTable dt = await _dataManager.ManageException(message, code, errorSource);
            if (dt.Rows.Count > 0)
                return ExceptionMapping(dt.Rows[0]);
            else return null;
        }

        internal ErrorDescription ExceptionMapping(DataRow row)
        {
            ErrorDescription error = new ErrorDescription();
            error.error_id = Convert.ToInt64(row["ERROR_ID"] == DBNull.Value ? 0 : row["ERROR_ID"]);
            error.error_code = (row["ERROR_CODE"] == DBNull.Value ? null : row["ERROR_CODE"].ToString());
            error.error_description = (row["ERROR_DESCRIPTION"] == DBNull.Value ? null : row["ERROR_DESCRIPTION"].ToString());
            error.error_custom_msg = (row["ERROR_CUSTOM_MSG"] == DBNull.Value ? null : row["ERROR_CUSTOM_MSG"].ToString());
            error.error_source = (row["ERROR_SOURCE"] == DBNull.Value ? null : row["ERROR_SOURCE"].ToString());
            return error;
        }
    }
}
