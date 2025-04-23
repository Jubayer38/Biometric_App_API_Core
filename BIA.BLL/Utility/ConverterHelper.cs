using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.BLL.Utility
{
    public static class ConverterHelper
    {
        internal static string UpperLowerWithSpaceConverter(string inputString)
        {
            string outputString = "";
            try
            {
                if (String.IsNullOrEmpty(inputString))

                {
                    throw new Exception("String conversion error! Input text is can not be null or empty.");
                }
                else if (inputString.Length == 1)
                {
                    outputString = inputString.ToUpper();
                }
                else if (!inputString.Contains(' ')
                    && inputString.Length > 1)
                {
                    outputString = inputString.Substring(0, 1).ToUpper() + inputString.Substring(1, inputString.Length - 1).ToLower();
                }
                else if (inputString.Contains(' ')
                    && inputString.Length > 1)
                {
                    string[] splitedData = inputString.Split(' ');
                    for (int i = 0; i < splitedData.Count(); i++)
                    {
                        if (i > 0)
                        {
                            outputString += " " + splitedData[i].Substring(0, 1).ToUpper() + splitedData[i].Substring(1, splitedData[i].Length - 1).ToLower();
                        }
                        else
                        {

                            outputString += splitedData[i].Substring(0, 1).ToUpper() + splitedData[i].Substring(1, splitedData[i].Length - 1).ToLower();
                        }
                    }
                }
                else
                {
                    throw new Exception("Someting wrong happend while converting text!");
                }
                return outputString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string MSISDNCountryCodeAddition(string msisdn, string countryCode)
        {
            try
            {
                if (String.IsNullOrEmpty(msisdn))
                {
                    throw new Exception("MSISDN can not be null or empty.");
                }
                else if (msisdn.Length == 11 || msisdn.Length == 13)
                {
                    if (msisdn.Substring(0, 2) != countryCode)
                    {
                        msisdn = countryCode + msisdn;
                    }
                }
                else
                {
                    throw new Exception("MSISDN must be 11 or 13 degit.");
                }

                return msisdn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
