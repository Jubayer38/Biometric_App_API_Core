using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Interfaces
{
    public interface IBL_Json
    {
        byte[] GetGenericJsonData<T>(T obj);
    }
}
