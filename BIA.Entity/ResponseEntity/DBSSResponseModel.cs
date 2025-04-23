using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.ResponseEntity
{
    public class MSISDNRootData
    {
        public object data { get; set; }
    }

    public class MSISDNRootDataForError
    {
        public object error { get; set; }
    }

    public class SubscriptionTypeRootData
    {
        public object data { get; set; }
    }

    public class UnpairedMSISDNRootData
    {
        public object data { get; set; }
    }
    public class PairedMSISDNRootData
    {
        public object data { get; set; }
    }
    public class PackageRootData
    {
        public object included { get; set; }
    }

    public class Name
    {
        public string en { get; set; }
        public string bn { get; set; }
    }
}
