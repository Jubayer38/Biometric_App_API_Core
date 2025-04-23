using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{

    public class CorporateSIMReplacemnetCustomerInfoRootobject
    {
        public CorporateSIMReplacemnetCustomerInfoData data { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoData
    {
        public CorporateSIMReplacemnetCustomerInfoAttributes attributes { get; set; }
        public CorporateSIMReplacemnetCustomerInfoRelationships relationships { get; set; }
        public CorporateSIMReplacemnetCustomerInfoLinks5 links { get; set; }
        public string id { get; set; }
        //[JsonPropertyName("type")]
        [JsonProperty("type")]
        public string type { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoAttributes
    {
        //[JsonPropertyName("id-expiry")]
        [JsonProperty(PropertyName = "id-expiry")]
        public string idexpiry { get; set; }
        public string email { get; set; }
        [JsonProperty(PropertyName = "bank-account-number")]
        public string bankaccountnumber { get; set; }
        [JsonProperty(PropertyName = "account-type")]
        public string accounttype { get; set; }
        [JsonProperty(PropertyName = "date-of-birth")]
        public string dateofbirth { get; set; }
        public string ban { get; set; }
        [JsonProperty(PropertyName = "id-document-type")]
        public string iddocumenttype { get; set; }
        [JsonProperty(PropertyName = "is-company")]
        public bool iscompany { get; set; }
        [JsonProperty(PropertyName = "online-id")]
        public string onlineid { get; set; }
        [JsonProperty(PropertyName = "vat-usage-code")]
        public string vatusagecode { get; set; }
        [JsonProperty(PropertyName = "coordinator-id")]
        public string coordinatorid { get; set; }
        [JsonProperty(PropertyName = "frame-agreement-ended-at")]
        public string frameagreementendedat { get; set; }
        [JsonProperty(PropertyName = "payment-method")]
        public string paymentmethod { get; set; }
        [JsonProperty(PropertyName = "agreement-start-date")]
        public string agreementstartdate { get; set; }
        public string language { get; set; }
        [JsonProperty(PropertyName = "is-loyalty-manager")]
        public bool isloyaltymanager { get; set; }
        [JsonProperty(PropertyName = "id-document-number")]
        public string iddocumentnumber { get; set; }
        [JsonProperty(PropertyName = "invoice-delivery-type")]
        public string invoicedeliverytype { get; set; }
        [JsonProperty(PropertyName = "frame-agreement-started-at")]
        public string frameagreementstartedat { get; set; }
        public string nationality { get; set; }
        [JsonProperty(PropertyName = "trade-register-id")]
        public string traderegisterid { get; set; }
        [JsonProperty(PropertyName = "business-uid")]
        public string businessuid { get; set; }
        [JsonProperty(PropertyName = "marketing-own")]
        public bool marketingown { get; set; }
        [JsonProperty(PropertyName = "alt-contact-phone")]
        public string altcontactphone { get; set; }
        public string category { get; set; }
        [JsonProperty(PropertyName = "first-name")]
        public string firstname { get; set; }
        [JsonProperty(PropertyName = "is-coordinator")]
        public bool iscoordinator { get; set; }
        public string occupation { get; set; }
        [JsonProperty(PropertyName = "middle-name")]
        public string middlename { get; set; }
        [JsonProperty(PropertyName = "segmentation-category")]
        public string segmentationcategory { get; set; }
        [JsonProperty(PropertyName = "is-fleet-manager")]
        public bool isfleetmanager { get; set; }
        [JsonProperty(PropertyName = "marketing-third-party")]
        public bool marketingthirdparty { get; set; }
        [JsonProperty(PropertyName = "last-name")]
        public string lastname { get; set; }
        [JsonProperty(PropertyName = "contact-phone")]
        public string contactphone { get; set; }
        public string gender { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoRelationships
    {
        public CorporateSIMReplacemnetCustomerInfoInventory inventory { get; set; }
        [JsonProperty(PropertyName = "company-people")]
        public CorporateSIMReplacemnetCustomerInfoCompanyPeople companypeople { get; set; }
        [JsonProperty(PropertyName = "coordinator-customer")]
        public CorporateSIMReplacemnetCustomerInfoInventory coordinatorcustomer { get; set; }

        [JsonProperty(PropertyName = "customer-edit-permission")]
        public CorporateSIMReplacemnetCustomerInfoCustomerEditPermission customereditpermission { get; set; }
        [JsonProperty(PropertyName = "contact-companies")]
        public CorporateSIMReplacemnetCustomerInfoCompanyPeople contactcompanies { get; set; }
        public CorporateSIMReplacemnetCustomerInfoOrders orders { get; set; }
        public CorporateSIMReplacemnetCustomerInfoAddresses addresses { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoInventory
    {
        public CorporateSIMReplacemnetCustomerInfoData1 data { get; set; }
        public CorporateSIMReplacemnetCustomerInfoLinks links { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoData1
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoLinks
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoCompanyPeople
    {
        public CorporateSIMReplacemnetCustomerInfoLinks1 links { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoLinks1
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoCustomerEditPermission
    {
        public CorporateSIMReplacemnetCustomerInfoData2 data { get; set; }
        public CorporateSIMReplacemnetCustomerInfoLinks2 links { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoData2
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoLinks2
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoOrders
    {
        public CorporateSIMReplacemnetCustomerInfoLinks3 links { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoLinks3
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoAddresses
    {
        public CorporateSIMReplacemnetCustomerInfoLinks4 links { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoLinks4
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacemnetCustomerInfoLinks5
    {
        public string self { get; set; }
    }
}
