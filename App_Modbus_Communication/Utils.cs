using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace App_Modbus_Communication
{
    public static class Utils
    {
        public static string ValidateIpAddress(string ipAddress)
        {
            IPAddress validationIp;
            if (IPAddress.TryParse(ipAddress, out validationIp))
            {
                return ipAddress;
            }
            else
            {
                throw new ArgumentException($"The validation of the IP address:{ipAddress} failed. Please check the IP.");
            }

        }

    }
}
