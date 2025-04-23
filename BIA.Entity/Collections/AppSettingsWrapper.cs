using Microsoft.Extensions.Configuration;

namespace BIA.Entity.Collections
{
    public class AppSettingsWrapper
    {
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
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                return Convert.ToInt32(configuration.GetSection("AppSettings:cyn_cherished_filter_allow").Value);
            }
        }

        public static string BLOTPApiBaseUrl
        {
            get
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                return configuration.GetSection("AppSettings:BLOTPApiBaseUrl").Value;

            }
        }

        public static string DMSApiBaseUrl
        {
            get
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                return configuration.GetSection("AppSettings:dmsBaseUrl").Value;

            }
        }

        public static string RetailerAPPRechargeAPI
        {
            get
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                return configuration.GetSection("AppSettings:RetilerBaseAPI").Value;

            }
        }

        public static string RSOAPPComplaintAPI
        {
            get
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                return configuration.GetSection("AppSettings:RSOBaseAPI").Value;

            }
        }
        public static string SingleSourceAPI
        {
            get
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

                return configuration.GetSection("AppSettings:sigleSourceAPI").Value;

            }
        }
    }
}
