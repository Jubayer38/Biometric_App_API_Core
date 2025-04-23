using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class CorporateSIMReplacementResponseRootobject
    {
        public List<CorporateSIMReplacementResponseData> data { get; set; }
    }

    public class CorporateSIMReplacementResponseData
    {
        public CorporateSIMReplacementResponseAttributes attributes { get; set; }
        public CorporateSIMReplacementResponseRelationships relationships { get; set; }
        public CorporateSIMReplacementResponseLinks28 links { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class CorporateSIMReplacementResponseAttributes
    {
        public float monthlycosts { get; set; }
        public bool allowreactivation { get; set; }
        public string contractstatus { get; set; }
        public object firstcalldate { get; set; }
        public object terminationtime { get; set; }
        public string contractid { get; set; }
        public string msisdn { get; set; }
        public DateTime activationtime { get; set; }
        public string status { get; set; }
        public DateTime latestcontractterminationtime { get; set; }
        public string directorylisting { get; set; }
        public string paymenttype { get; set; }
        public string originalcontractconfirmationcode { get; set; }
    }

    public class CorporateSIMReplacementResponseRelationships
    {
        public CorporateSIMReplacementResponseSimCards simcards { get; set; }
        public CorporateSIMReplacementResponseBillingAccounts billingaccounts { get; set; }
        public CorporateSIMReplacementResponseServices services { get; set; }
        public CorporateSIMReplacementResponseSubscriptionDiscounts subscriptiondiscounts { get; set; }
        public CorporateSIMReplacementResponseNetworkServices networkservices { get; set; }
        public CorporateSIMReplacementResponseAvailableLoanProducts availableloanproducts { get; set; }
        public CorporateSIMReplacementResponseOwnerCustomer ownercustomer { get; set; }
        public CorporateSIMReplacementResponseProducts products { get; set; }
        public CorporateSIMReplacementResponsePayerCustomer payercustomer { get; set; }
        public CorporateSIMReplacementResponseAvailableSubscriptionTypes availablesubscriptiontypes { get; set; }
        public CorporateSIMReplacementResponseDocumentValidations documentvalidations { get; set; }
        [JsonProperty(PropertyName = "coordinator-customer")]
        public CorporateSIMReplacementResponseCoordinatorCustomer coordinatorcustomer { get; set; }
        public CorporateSIMReplacementResponseProductUsages productusages { get; set; }
        public CorporateSIMReplacementResponsePortingRequests portingrequests { get; set; }
        public CorporateSIMReplacementResponseBillingRatePlan billingrateplan { get; set; }
        public CorporateSIMReplacementResponseCombinedUsageReport combinedusagereport { get; set; }
        public CorporateSIMReplacementResponseUserCustomer usercustomer { get; set; }
        public CorporateSIMReplacementResponseGsmServiceUsages gsmserviceusages { get; set; }
        public CorporateSIMReplacementResponseBalances balances { get; set; }
        public CorporateSIMReplacementResponseBillingUsages billingusages { get; set; }
        public CorporateSIMReplacementResponseBarrings barrings { get; set; }
        public CorporateSIMReplacementResponseSubscriptionType subscriptiontype { get; set; }
        public CorporateSIMReplacementResponseAvailableProducts availableproducts { get; set; }
        public CorporateSIMReplacementResponseCatalogSimCards catalogsimcards { get; set; }
        public CorporateSIMReplacementResponseConnectedProducts connectedproducts { get; set; }
        public CorporateSIMReplacementResponseConnectionType connectiontype { get; set; }
        public CorporateSIMReplacementResponseAvailableChildProducts availablechildproducts { get; set; }
        public CorporateSIMReplacementResponseSimCardOrders simcardorders { get; set; }
    }

    public class CorporateSIMReplacementResponseSimCards
    {
        public CorporateSIMReplacementResponseLinks links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseBillingAccounts
    {
        public CorporateSIMReplacementResponseLinks1 links { get; set; }
        public List<CorporateSIMReplacementResponseDatum> data { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks1
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseDatum
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseServices
    {
        public CorporateSIMReplacementResponseLinks2 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks2
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseSubscriptionDiscounts
    {
        public CorporateSIMReplacementResponseLinks3 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks3
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseNetworkServices
    {
        public CorporateSIMReplacementResponseLinks4 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks4
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseAvailableLoanProducts
    {
        public CorporateSIMReplacementResponseLinks5 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks5
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseOwnerCustomer
    {
        public CorporateSIMReplacementResponseData1 data { get; set; }
        public CorporateSIMReplacementResponseLinks6 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData1
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks6
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseProducts
    {
        public CorporateSIMReplacementResponseLinks7 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks7
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponsePayerCustomer
    {
        public CorporateSIMReplacementResponseData2 data { get; set; }
        public CorporateSIMReplacementResponseLinks8 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData2
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks8
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseAvailableSubscriptionTypes
    {
        public CorporateSIMReplacementResponseLinks9 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks9
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseDocumentValidations
    {
        public CorporateSIMReplacementResponseLinks10 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks10
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseCoordinatorCustomer
    {
        public CorporateSIMReplacementResponseData3 data { get; set; }
        public CorporateSIMReplacementResponseLinks11 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData3
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks11
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseProductUsages
    {
        public CorporateSIMReplacementResponseLinks12 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks12
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponsePortingRequests
    {
        public CorporateSIMReplacementResponseLinks13 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks13
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseBillingRatePlan
    {
        public CorporateSIMReplacementResponseData4 data { get; set; }
        public CorporateSIMReplacementResponseLinks14 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData4
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks14
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseCombinedUsageReport
    {
        public CorporateSIMReplacementResponseData5 data { get; set; }
        public CorporateSIMReplacementResponseLinks15 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData5
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks15
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseUserCustomer
    {
        public CorporateSIMReplacementResponseData6 data { get; set; }
        public CorporateSIMReplacementResponseLinks16 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData6
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks16
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseGsmServiceUsages
    {
        public CorporateSIMReplacementResponseLinks17 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks17
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseBalances
    {
        public CorporateSIMReplacementResponseLinks18 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks18
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseBillingUsages
    {
        public CorporateSIMReplacementResponseLinks19 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks19
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseBarrings
    {
        public CorporateSIMReplacementResponseLinks20 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks20
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseSubscriptionType
    {
        public CorporateSIMReplacementResponseData7 data { get; set; }
        public CorporateSIMReplacementResponseLinks21 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData7
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks21
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseAvailableProducts
    {
        public CorporateSIMReplacementResponseLinks22 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks22
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseCatalogSimCards
    {
        public CorporateSIMReplacementResponseLinks23 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks23
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseConnectedProducts
    {
        public CorporateSIMReplacementResponseLinks24 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks24
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseConnectionType
    {
        public CorporateSIMReplacementResponseData8 data { get; set; }
        public CorporateSIMReplacementResponseLinks25 links { get; set; }
    }

    public class CorporateSIMReplacementResponseData8
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks25
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseAvailableChildProducts
    {
        public CorporateSIMReplacementResponseLinks26 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks26
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseSimCardOrders
    {
        public CorporateSIMReplacementResponseLinks27 links { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks27
    {
        public string related { get; set; }
    }

    public class CorporateSIMReplacementResponseLinks28
    {
        public string self { get; set; }
    }
}
