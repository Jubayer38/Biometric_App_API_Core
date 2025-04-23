using BIA.Entity.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Collections
{
    public class BL_Json : IBL_Json
    {
        public byte[] GetGenericJsonData<T>(T obj)
        {
            string result = "";
            //Convert.FromBase64String();
            var content2 = JsonConvert.SerializeObject(obj);
            result = content2.ToString();
            byte[] bytedata = Encoding.ASCII.GetBytes(result);
            return bytedata;
        }        
    }
    public class XmlToByteConverter
    {
        public byte[] ConvertXmlToByteArray(string xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
                throw new ArgumentNullException(nameof(xmlString));

            return Encoding.UTF8.GetBytes(xmlString);
        }
    }
}
