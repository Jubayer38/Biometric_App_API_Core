using BIA.DAL.Repositories;
using BIA.Entity.Collections;
using BIA.Entity.DB_Model;
using BIA.Entity.ResponseEntity;
using BIA.Entity.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.BLLServices
{
    public class BLLDivDisThana
    {
        private readonly DALBiometricRepo _dataManager;

        public BLLDivDisThana(DALBiometricRepo dataManager)
        {
            _dataManager = dataManager;
        }

        public async Task<List<DivisionModel>> GetDivision()
        {

            List<DivisionModel> divs = new List<DivisionModel>();
            try
            {
                var dataRow = await _dataManager.GetDivision();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {

                    DivisionModel div = new DivisionModel();
                    div.DIVISIONID = Convert.ToInt32(dataRow.Rows[i]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_ID"]);
                    div.DIVISIONNAME = Convert.ToString(dataRow.Rows[i]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_NAME"]);
                    divs.Add(div);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return divs;
        }

        public async Task<List<DivisionModelV2>> GetDivisionV2()
        {
            List<DivisionModelV2> divs = new List<DivisionModelV2>();
            try
            {
                var dataRow = await _dataManager.GetDivision();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {

                    DivisionModelV2 div = new DivisionModelV2();
                    div.DIVISIONID = Convert.ToInt32(dataRow.Rows[i]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_ID"]);
                    div.DIVISIONNAME = Convert.ToString(dataRow.Rows[i]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_NAME"]);
                    divs.Add(div);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return divs;
        }

        public async Task<List<DistrictModel>> GetDistrict()
        {
            List<DistrictModel> diss = new List<DistrictModel>();
            try
            {
                var dataRow = await _dataManager.GetDistrict();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {

                    DistrictModel dis = new DistrictModel();
                    dis.DISTRICTID = Convert.ToInt32(dataRow.Rows[i]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_ID"]);
                    dis.DISTRICTNAME = Convert.ToString(dataRow.Rows[i]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_NAME"]);
                    dis.DIVISIONID = Convert.ToInt32(dataRow.Rows[i]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_ID"]);
                    diss.Add(dis);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return diss;
        }

        public async Task<List<DistrictModelV2>> GetDistrictV2()
        {
            List<DistrictModelV2> diss = new List<DistrictModelV2>();
            try
            {
                var dataRow = await _dataManager.GetDistrict();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {

                    DistrictModelV2 dis = new DistrictModelV2();
                    dis.DISTRICTID = Convert.ToInt32(dataRow.Rows[i]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_ID"]);
                    dis.DISTRICTNAME = Convert.ToString(dataRow.Rows[i]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_NAME"]);
                    dis.DIVISIONID = Convert.ToInt32(dataRow.Rows[i]["DIVISION_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_ID"]);
                    diss.Add(dis);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return diss;
        }

        public async Task<List<ThanaModel>> GetThana()
        {
            List<ThanaModel> thanas = new List<ThanaModel>();
            try
            {
                var dataRow = await _dataManager.GetThana();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {

                    ThanaModel tha = new ThanaModel();
                    tha.THANAID = Convert.ToInt32(dataRow.Rows[i]["THANA_ID"] == DBNull.Value ? null : dataRow.Rows[i]["THANA_ID"]);
                    tha.THANANAME = Convert.ToString(dataRow.Rows[i]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["THANA_NAME"]);
                    tha.DISTRICTID = Convert.ToInt32(dataRow.Rows[i]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_ID"]);
                    thanas.Add(tha);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thanas;
        }

        public async Task<List<ThanaModelV2>> GetThanaV2()
        {
            List<ThanaModelV2> thanas = new List<ThanaModelV2>();
            try
            {
                var dataRow = await _dataManager.GetThana();
                for (int i = 0; i < dataRow.Rows.Count; i++)
                {
                    ThanaModelV2 tha = new ThanaModelV2();
                    tha.THANAID = Convert.ToInt32(dataRow.Rows[i]["THANA_ID"] == DBNull.Value ? null : dataRow.Rows[i]["THANA_ID"]);
                    tha.THANANAME = Convert.ToString(dataRow.Rows[i]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["THANA_NAME"]);
                    tha.DISTRICTID = Convert.ToInt32(dataRow.Rows[i]["DISTRICT_ID"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_ID"]);
                    thanas.Add(tha);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thanas;
        }

        public async Task<DivDisThanaResponse2> GetDivDisThana()
        {
            DivDisThanaResponse2 divDisThanaResponse = new DivDisThanaResponse2();
            List<VMDivDisThana> divDisThanaObjList = new List<VMDivDisThana>();

            try
            {
                var dataRow = await _dataManager.GetDivDisThana();
                if (dataRow.Rows.Count > 0)
                {
                    for (int i = 0; i < dataRow.Rows.Count; i++)
                    {
                        VMDivDisThana vmObj = new VMDivDisThana();
                        vmObj.division_id = Convert.ToInt32(dataRow.Rows[i]["DIVISION_ID"] == DBNull.Value ? 0 : dataRow.Rows[i]["DIVISION_ID"]);
                        vmObj.division_name = Convert.ToString(dataRow.Rows[i]["DIVISION_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DIVISION_NAME"]);
                        vmObj.district_id = Convert.ToInt32(dataRow.Rows[i]["DISTRICT_ID"] == DBNull.Value ? 0 : dataRow.Rows[i]["DISTRICT_ID"]);
                        vmObj.district_name = Convert.ToString(dataRow.Rows[i]["DISTRICT_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["DISTRICT_NAME"]);
                        vmObj.div_dis_id = Convert.ToInt32(dataRow.Rows[i]["DIS_DIV_ID"] == DBNull.Value ? 0 : dataRow.Rows[i]["DIS_DIV_ID"]);
                        vmObj.thana_id = Convert.ToInt32(dataRow.Rows[i]["THANA_ID"] == DBNull.Value ? 0 : dataRow.Rows[i]["THANA_ID"]);
                        vmObj.thana_name = Convert.ToString(dataRow.Rows[i]["THANA_NAME"] == DBNull.Value ? null : dataRow.Rows[i]["THANA_NAME"]);
                        vmObj.dis_thana_id = Convert.ToInt32(dataRow.Rows[i]["THANA_DIS_ID"] == DBNull.Value ? 0 : dataRow.Rows[i]["THANA_DIS_ID"]);
                        divDisThanaObjList.Add(vmObj);
                    }

                    divDisThanaResponse.data = DivDisThaneObjBuinding(divDisThanaObjList);
                    divDisThanaResponse.result = true;
                    divDisThanaResponse.message = MessageCollection.Success;
                    return divDisThanaResponse;
                }
                else
                {
                    divDisThanaResponse.data = new List<DivisionModel>();
                    divDisThanaResponse.result = false;
                    divDisThanaResponse.message = MessageCollection.NoDataFound;
                    return divDisThanaResponse;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private List<DivisionModel> DivDisThaneObjBuinding(List<VMDivDisThana> divDisThanaDataList)
        {
            try
            {
                return divDisThanaDataList.Select(s => new DivisionModel
                {
                    DIVISIONID = s.division_id,
                    DIVISIONNAME = s.division_name,
                    DistrictModel = divDisThanaDataList.Where(dis => dis.division_id == s.division_id)
                       .Select(dis => new DistrictModel
                       {
                           DISTRICTID = dis.district_id,
                           DISTRICTNAME = dis.district_name,
                           ThanaModel = divDisThanaDataList.Where(tha => tha.thana_id == s.thana_id)
                           .Select(tha => new ThanaModel
                           {
                               THANAID = tha.thana_id,
                               THANANAME = tha.thana_name,
                               DISTRICTID = tha.dis_thana_id
                           }).ToList()
                       }).ToList()
                }).OrderBy(s => s.DIVISIONNAME).ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
