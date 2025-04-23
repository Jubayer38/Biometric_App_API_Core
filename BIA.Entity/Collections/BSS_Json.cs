using BIA.Entity.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Collections
{
    public class BSS_Json : IBSS_Json
    {
        public byte[] GetGenericJsonData<T>(T obj)
        {
            string result = "";
            var content2 = JsonConvert.SerializeObject(obj);
            result = content2.ToString();
            byte[] bytedata = Encoding.ASCII.GetBytes(result);
            return bytedata;
        }
    }
}
