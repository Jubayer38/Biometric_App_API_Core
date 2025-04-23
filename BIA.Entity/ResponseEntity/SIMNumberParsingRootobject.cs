using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class SIMNumberParsingRootobject
    {
        public List<SIMNumberParsingDatum> data { get; set; }
    }

    public class SIMNumberParsingDatum
    {
        public SIMNumberParsingAttributes attributes { get; set; }
        public SIMNumberParsingRelationships relationships { get; set; }
        public SIMNumberParsingLinks1 links { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class SIMNumberParsingAttributes
    {
        public string puk1 { get; set; }
        public bool ismultisurf { get; set; }
        public string pin1 { get; set; }
        public string icc { get; set; }
        public string puk2 { get; set; }
        public string pin2 { get; set; }
        public string simtype { get; set; }
        public string status { get; set; }
    }

    public class SIMNumberParsingRelationships
    {
        public SIMNumberParsingSubscription subscription { get; set; }
    }

    public class SIMNumberParsingSubscription
    {
        public SIMNumberParsingData data { get; set; }
        public SIMNumberParsingLinks links { get; set; }
    }

    public class SIMNumberParsingData
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class SIMNumberParsingLinks
    {
        public string related { get; set; }
    }

    public class SIMNumberParsingLinks1
    {
        public string self { get; set; }
    }
}
