using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class OtherCDTResModel
    {
        public Object[] data { get; set; }
        public List<Included> included { get; set; }
    }
    public class Included
    {
        public Attributes1 attributes { get; set; }
        public Relationships1 relationships { get; set; }
        public Links31 links { get; set; }
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Attributes1
    {
        public bool changeongoing { get; set; }
        public object upcominglevel { get; set; }
        public string subscriptionid { get; set; }
        public string[] barringlevelids { get; set; }
        public object[] upcomingbarringlevelids { get; set; }
        public string level { get; set; }
    }

    public class Relationships1
    {
        public Barring barring { get; set; }
        public BarringLevels barringlevels { get; set; }
    }

    public class Barring
    {
        public Data77 data { get; set; }
        public Links29 links { get; set; }
    }

    public class Data77
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Links29
    {
        public string related { get; set; }
    }

    public class BarringLevels
    {
        public Links30 links { get; set; }
    }

    public class Links30
    {
        public string related { get; set; }
    }

    public class Links31
    {
        public string self { get; set; }
    }
}
