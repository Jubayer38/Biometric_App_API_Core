using BIA.Entity.Collections;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.PopulateModel
{
    public class FTR_RestrictionPopulateModel
    {
        public string GetMethodUrl(string msisdn)
        {
            string meathodUrl = "";
            if (msisdn.Substring(0, 2) == "88")
            {
                meathodUrl = $"/api/v1/subscriptions/{msisdn}/relationships/products?identifier=msisdn";
            }
            else
            {
                meathodUrl = $"/api/v1/subscriptions/88{msisdn}/relationships/products?identifier=msisdn";
            }
            return meathodUrl;
        }

        public FTRRestrictionReqModel PopulateFTRRestrictionReq()
        {
            FTRRestrictionReqModel requestModel = new FTRRestrictionReqModel
            {
                data = new FTRData()
                {
                    type = SettingsValues.GetFTRRequestType(),
                    //id = SettingsValues.GetFTRRequestID(),
                    meta = new FTRMeta()
                    {
                        services = new Dictionary<string, object>(),
                        channel = SettingsValues.GetFTRRequestChannel()
                    }
                }
            };
            return requestModel;
        }
    }
}
