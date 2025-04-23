using BIA.DAL.Repositories;
using BIA.Entity.CommonEntity;
using BIA.Entity.DB_Model;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLGeofencing
    {
        #region Geofencing
        private readonly DALBiometricRepo _dataManager;
        public BLLGeofencing(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }
        public async Task<GeofenceReqModel> GetLoggedinRetLatLon(string retailerCode)
        {
            try
            {
                DataTable dt = await _dataManager.GetLoggedinRetLatLon(retailerCode);

                GeofenceReqModel resModel = new GeofenceReqModel();

                foreach (DataRow dr in dt.Rows)
                {
                    resModel.retilerLat = Convert.ToDouble(dr["RET_LATIRUDE"].ToString());
                    resModel.retilerLon = Convert.ToDouble(dr["RET_LONGITUDE"].ToString());
                }
                return resModel;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
} 
