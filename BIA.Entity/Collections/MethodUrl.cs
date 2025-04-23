using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Collections
{
    public class MethodUrl
    {
        public string GetMethodUrl(OrderDataModel item)
        {
            string methodUrl = "";

            if (item.status == (int)StatusNo.order_request && item.purpose_number == (int)EnumPurposeNumber.mnp_port_in_cancel)
            {
                methodUrl = "/api/v1/order-cancellations";
            }
            else if (item.status == (int)StatusNo.order_request && item.purpose_number == (int)EnumPurposeNumber.DeRegistration)
            {
                methodUrl = "/api/v1/subscriptions/" + item.dbss_subscription_id;
            }
            else if (item.status == (int)StatusNo.order_request && item.purpose_number == (int)EnumPurposeNumber.SIMReplacement)
            {
                methodUrl = $"/api/v1/subscriptions/{item.dbss_subscription_id}/sim-changes";
            }
            else if (item.status == (int)StatusNo.order_request && item.purpose_number == (int)EnumPurposeNumber.SIMTransfer)
            {
                methodUrl = $"/api/v1/subscriptions/{item.dbss_subscription_id}/";
            }
            else if (item.status == (int)StatusNo.order_request && item.purpose_number == (int)EnumPurposeNumber.SIMCategoryMigration)
            {
                methodUrl = $"/api/v1/subscriptions/{item.dbss_subscription_id}/relationships/subscription-type/";
            }
            else if (item.status == (int)StatusNo.order_request)
            {
                methodUrl = "/api/v1/orders";
            }
            else
            {
                methodUrl = "";
            }
            return methodUrl;
        }
    }
}
