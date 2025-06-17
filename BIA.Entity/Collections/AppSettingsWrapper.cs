using Microsoft.Extensions.Configuration;

namespace BIA.Entity.Collections
{
    public class AppSettingsWrapper
    {
        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public static string ApiBaseUrl
        {
            get
            {
                //IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                //return configuration.GetSection("AppSettings:BDSSApiBaseUrl").Value;

                return SettingsValues.GetDbssBaseUrl();

            }
        }

        public static int FilterAllow
        {
            get
            {
                return Convert.ToInt32(_configuration.GetSection("AppSettings:cyn_cherished_filter_allow").Value);
            }
        }

        public static string BLOTPApiBaseUrl
        {
            get
            {
                return _configuration.GetSection("AppSettings:BLOTPApiBaseUrl").Value;

            }
        }

        public static string DMSApiBaseUrl
        {
            get
            {
                return _configuration.GetSection("AppSettings:dmsBaseUrl").Value;

            }
        }

        public static string RetailerAPPRechargeAPI
        {
            get
            {
                return _configuration.GetSection("AppSettings:RetilerBaseAPI").Value;

            }
        }

        public static string RSOAPPComplaintAPI
        {
            get
            {
                return _configuration.GetSection("AppSettings:RSOBaseAPI").Value;

            }
        }
        public static string SingleSourceAPI
        {
            get
            {
                return _configuration.GetSection("AppSettings:singleSourceAPI").Value;

            }
        }
    }
}
