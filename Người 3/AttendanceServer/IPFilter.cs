
using System.Net;
using System.Net.Sockets;

namespace AttendanceServer
{
    public static class IPFilter
    {
        private static readonly IPAddress AllowedNetwork =
            IPAddress.Parse("192.168.1.0");

        private const int PrefixLength = 24;

        public static bool IsAllowed(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip))
                return true;
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            if (ip.AddressFamily != AddressFamily.InterNetwork)
                return false;

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] networkBytes = AllowedNetwork.GetAddressBytes();

            int fullBytes = PrefixLength / 8;
            int remainingBits = PrefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                if (ipBytes[i] != networkBytes[i])
                    return false;
            }

            if (remainingBits > 0)
            {
                int mask = 0xFF << (8 - remainingBits);

                if ((ipBytes[fullBytes] & mask) !=
                    (networkBytes[fullBytes] & mask))
                    return false;
            }

            return true;
        }
    }
}