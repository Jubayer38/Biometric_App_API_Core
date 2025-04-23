using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.DB_Model
{
    public class DivisionModel
    {
        public int DIVISIONID { get; set; }
        public string DIVISIONNAME { get; set; }
        public IEnumerable<DistrictModel> DistrictModel { get; set; }
    }


    public class DistrictModel
    {
        public int DISTRICTID { get; set; }
        public string DISTRICTNAME { get; set; }
        public int DIVISIONID { get; set; }
        public IEnumerable<ThanaModel> ThanaModel { get; set; }
    }


    public class ThanaModel
    {
        public int THANAID { get; set; }
        public string THANANAME { get; set; }
        public int DISTRICTID { get; set; }
    }

    public class DivisionModelV2
    {
        public int DIVISIONID { get; set; }
        public string DIVISIONNAME { get; set; }
        public IEnumerable<DistrictModelV2> DistrictModel { get; set; }
    }
    public class DistrictModelV2
    { 
        public int DISTRICTID { get; set; }
        public string DISTRICTNAME { get; set; }
        public int DIVISIONID { get; set; }
        public IEnumerable<ThanaModelV2> ThanaModel { get; set; }
    }
    public class ThanaModelV2
    {
        public int THANAID { get; set; }
        public string THANANAME { get; set; }
        public int DISTRICTID { get; set; }
    }
}
