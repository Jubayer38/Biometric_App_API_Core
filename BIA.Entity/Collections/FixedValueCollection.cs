using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Collections
{
    public static class FixedValueCollection
    {
        public static string ResellerChannel
        {
            get
            {
                return "RESELLER";
            }
        }

        public static string MonobrandChannel
        {
            get
            {
                return "MonoBrand";
            }
        }

        public static string EshopChannel
        {
            get
            {
                return "eshop";
            }
        }

        public static string SMEChannel
        {
            get
            {
                return "SME";
            }
        }

        public static string CorporateChannel
        {
            get
            {
                return "Corporate";
            }
        }

        public static string SIMCode
        {
            get
            {
                return "898803";
            }
        }

        public static string ResellerCodeText
        {
            get
            {
                return "R";
            }
        }

        public static string MSISDNCountryCode
        {
            get
            {
                return "88";
            }
        }
        public static string MSISDNFixedValue
        {
            get
            {
                return "8801";
            }
        }
        public static string ValidPairedMSISDNStatus
        {
            get
            {
                return "ordered";
            }
        }

        public static int MaxFingerPrintScore
        {
            get
            {
                return 65;
            }
        }


        public static string QCStatusRejected
        {
            get
            {
                return "rejected";
            }
        }

        public static string QCStatusUnverified
        {
            get
            {
                return "unverified";
            }
        }


        public static string DBSSError
        {
            get
            {
                return "DBSS Error: ";
            }
        }
        public static string DMSError
        {
            get
            {
                return "DMS Error: ";
            }
        }
        public static string SIMError
        {
            get
            {
                return "SIM: ";
            }
        }


        public static string PaymentTypePrepaid
        {
            get
            {
                return "prepaid";
            }
        }

        public static string PaymentTypePrepaidStarTrek
        {
            get
            {
                return "ryz-prepaid";
            } 
        }

        public static string PaymentTypeESim
        {
            get
            {
                return "E-SIM";
            }
        }

        public static string PaymentTypeESimStarTrek
        {
            get
            {
                return "ryz-esim";
            }
        }

        public static string PaymentTypeE_SIM_SWAP
        {
            get
            {
                return "E_SIM_SWAP";
            }
        }

        public static string PaymentTypePostpaid
        {
            get
            {
                return "postpaid";
            }
        }

        public static string SIMTypeSIM
        {
            get
            {
                return "sim";
            }
        }

        public static string SIMTypeUSIM
        {
            get
            {
                return "usim";
            }
        }

        public static string SIMTypePLI
        {
            get
            {
                return "pli";
            }
        }

        public static string PhycalInventorySIMTypeSIM_SWAP
        {
            get
            {
                return "sim_swap";
            }
        }

        public static string PhycalInventorySIMTypeEV_SWAP
        {
            get
            {
                return "ev_swap";
            }
        }

        public static string PairedMSISDN
        {
            get
            {
                return "paired";
            }
        }

        public static string UnairedMSISDN
        {
            get
            {
                return "unpaired";
            }
        }

        public static string MSISDNError
        {
            get
            {
                return "MSISDN: ";
            }
        }

        #region Cherish Msisdn
        public static string CherishMsisdn
        {
            get
            {
                return "N";
            }
        }
        #endregion
    }
}
