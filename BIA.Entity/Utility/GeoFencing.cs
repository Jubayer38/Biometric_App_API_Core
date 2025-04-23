using BIA.Entity.DB_Model;
using BIA.Entity.ENUM;
using BIA.Entity.RequestEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Utility
{
    public class GeoFencing
    {
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double distance = 0;
            try
            {
                const double R = 6371000.0; // Earth's radius in meters

                // Convert latitude and longitude from degrees to radians
                lat1 = ToRadians(lat1);
                lon1 = ToRadians(lon1);
                lat2 = ToRadians(lat2);
                lon2 = ToRadians(lon2);

                // Differences in coordinates
                double dLat = lat2 - lat1;
                double dLon = lon2 - lon1;

                // Haversine formula
                double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                // Calculate the distance in meters
                distance = R * c;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

            return distance;
        }        

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
