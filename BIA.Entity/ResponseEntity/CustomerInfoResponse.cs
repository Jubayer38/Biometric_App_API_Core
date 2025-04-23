using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{

    public class CustomerInfoResponse
    {
        public CustomerInfoResponseAttributes CustomerInfo { get; set; }
        public CustomerAddressResponseAttributes CustomerAddressInfo { get; set; }
    }


    public class CustomerInfoResponseRootobject
    {
        public CustomerInfoResponseData data { get; set; }
    }

    public class CustomerInfoResponseData
    {
        public CustomerInfoResponseAttributes attributes { get; set; }
        public CustomerInfoResponseRelationships relationships { get; set; }
        public CustomerInfoResponseLinks5 links { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class CustomerInfoResponseAttributes
    {
        public string idexpiry { get; set; }
        public string email { get; set; }
        public object bankaccountnumber { get; set; }
        public object accounttype { get; set; }
        public string dateofbirth { get; set; }
        public object ban { get; set; }
        public string iddocumenttype { get; set; }
        public bool iscompany { get; set; }
        public object onlineid { get; set; }
        public object frameagreementendedat { get; set; }
        public string paymentmethod { get; set; }
        public object agreementstartdate { get; set; }
        public string language { get; set; }
        public bool isloyaltymanager { get; set; }
        public string iddocumentnumber { get; set; }
        public string invoicedeliverytype { get; set; }
        public object frameagreementstartedat { get; set; }
        public string nationality { get; set; }
        public object traderegisterid { get; set; }
        public object businessuid { get; set; }
        public bool marketingown { get; set; }
        [JsonProperty(PropertyName = "alt-contact-phone")]
        public string altcontactphone { get; set; }
        public string category { get; set; }
        [JsonProperty(PropertyName = "first-name")]
        public string firstname { get; set; }
        public bool iscoordinator { get; set; }
        public object occupation { get; set; }
        public object middlename { get; set; }
        public string segmentationcategory { get; set; }
        public bool isfleetmanager { get; set; }
        public bool marketingthirdparty { get; set; }
        public string lastname { get; set; }
        public string contactphone { get; set; }
        public string gender { get; set; }
    }

    public class CustomerInfoResponseRelationships
    {
        public CustomerInfoResponseInventory inventory { get; set; }
        public CustomerInfoResponseCompanyPeople companypeople { get; set; }
        public CustomerInfoResponseCustomerEditPermission customereditpermission { get; set; }
        public CustomerInfoResponseOrders orders { get; set; }
        public CustomerInfoResponseAddresses addresses { get; set; }
    }

    public class CustomerInfoResponseInventory
    {
        public CustomerInfoResponseData1 data { get; set; }
        public CustomerInfoResponseLinks links { get; set; }
    }

    public class CustomerInfoResponseData1
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CustomerInfoResponseLinks
    {
        public string related { get; set; }
    }

    public class CustomerInfoResponseCompanyPeople
    {
        public CustomerInfoResponseLinks1 links { get; set; }
    }

    public class CustomerInfoResponseLinks1
    {
        public string related { get; set; }
    }

    public class CustomerInfoResponseCustomerEditPermission
    {
        public CustomerInfoResponseData2 data { get; set; }
        public CustomerInfoResponseLinks2 links { get; set; }
    }

    public class CustomerInfoResponseData2
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CustomerInfoResponseLinks2
    {
        public string related { get; set; }
    }

    public class CustomerInfoResponseOrders
    {
        public CustomerInfoResponseLinks3 links { get; set; }
    }

    public class CustomerInfoResponseLinks3
    {
        public string related { get; set; }
    }

    public class CustomerInfoResponseAddresses
    {
        public CustomerInfoResponseLinks4 links { get; set; }
    }

    public class CustomerInfoResponseLinks4
    {
        public string related { get; set; }
    }

    public class CustomerInfoResponseLinks5
    {
        public string self { get; set; }
    }

}
