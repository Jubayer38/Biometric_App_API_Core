using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Collections
{
    public class StringFormatCollection
    {
        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

        public static string DBSSDOBFormat
        {
            get
            {
                return "yyyy-MM-dd";
            }
        }
        public static string AccessTokenFormat
        {
            get
            {
                try
                {
                    return _configuration.GetSection("AppSettings:AccessTokenFormat").Value;

                }
                catch (NullReferenceException)
                {
                    throw new Exception("'AccessTokenFormat' key may be missing within appSettings in Web.Config file.");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public static string AccessTokenFormatV2
        {
            get
            {
                try
                {
                    return _configuration.GetSection("AppSettings:AccessTokenFormatV2").Value;

                }
                catch (NullReferenceException)
                {
                    throw new Exception("'AccessTokenFormatV2' key may be missing within appSettings in Web.Config file.");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public static string[] AccessTokenPropertyArray
        {
            get
            {
                return new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:" };
            }
        }
        public static string[] AccessTokenPropertyArrayV2
        {
            get
            {
                return new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:", ",random:" };
            }
        }

        public static string[] SecurityTokenPropertyArray
        {
            get
            {
                return new string[] { ",uid:", ",uname:", ",dc:", ",deviceId:" };
            }
        }
    }
}
