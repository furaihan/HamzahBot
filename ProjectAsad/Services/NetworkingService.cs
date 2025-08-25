using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ProjectAsad.Model;

namespace ProjectAsad.Services
{
    public class NetworkingService
    {
        public NetworkingService()
        {
            // Constructor logic here
        }

        // IP Address Analyzer
        public string AnalyzeIPAddress(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
            {
                return "Invalid IP address.";
            }

            var addressBytes = ip.GetAddressBytes();
            var binaryRepresentation = string.Join(".", addressBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            string ipClass = ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ?
                (addressBytes[0] < 128 ? "Class A" : addressBytes[0] < 192 ? "Class B" : addressBytes[0] < 224 ? "Class C" : "Class D or E") :
                "Not IPv4";

            bool isPrivate = (addressBytes[0] == 10) ||
                             (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31) ||
                             (addressBytes[0] == 192 && addressBytes[1] == 168);

            return $"IP Class: {ipClass}\nPublic/Private: {(isPrivate ? "Private" : "Public")}\nBinary: {binaryRepresentation}";
        }

        // Subnet Calculator
        public IPSubnet CalculateSubnet(string ipAddress, string subnetMask)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
            {
                throw new ArgumentException("Invalid IP address.");
            }
            
            IPAddress mask = IPAddress.Any;
            
            // Support CIDR notation like "/24"
            if (subnetMask.StartsWith("/"))
            {
                if (int.TryParse(subnetMask.TrimStart('/'), out int prefix) && prefix >= 0 && prefix <= 32)
                {
                    uint maskInt = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
                    var cidrMaskBytes = BitConverter.GetBytes(maskInt);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(cidrMaskBytes);
                    mask = new IPAddress(cidrMaskBytes);
                }
                else
                {
                    throw new ArgumentException("Invalid CIDR notation.");
                }
            }
            else 
            {
                if (!IPAddress.TryParse(subnetMask, out var parsedMask))
                {
                    throw new ArgumentException("Invalid subnet mask.");
                }
                mask = parsedMask;
            }

            var ipBytes = ip.GetAddressBytes();
            var maskBytes = mask.GetAddressBytes();

            var networkAddress = new byte[4];
            var broadcastAddress = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                networkAddress[i] = (byte)(ipBytes[i] & maskBytes[i]);
                broadcastAddress[i] = (byte)(networkAddress[i] | ~maskBytes[i]);
            }

            var network = new IPAddress(networkAddress);
            var broadcast = new IPAddress(broadcastAddress);

            // Calculate number of host bits
            int hostBits = maskBytes.Sum(b => Convert.ToString(b, 2).Count(c => c == '0'));
            int usableHosts = hostBits > 1 ? (int)Math.Pow(2, hostBits) - 2 : 0;

            // Calculate first and last host addresses
            var firstHostBytes = (byte[])networkAddress.Clone();
            var lastHostBytes = (byte[])broadcastAddress.Clone();
            
            // First host: network address + 1
            for (int i = 3; i >= 0; i--)
            {
                if (firstHostBytes[i] < 255)
                {
                    firstHostBytes[i]++;
                    break;
                }
                firstHostBytes[i] = 0;
            }
            
            // Last host: broadcast address - 1
            for (int i = 3; i >= 0; i--)
            {
                if (lastHostBytes[i] > 0)
                {
                    lastHostBytes[i]--;
                    break;
                }
                lastHostBytes[i] = 255;
            }

            var firstHost = new IPAddress(firstHostBytes);
            var lastHost = new IPAddress(lastHostBytes);

            IPSubnet subnet = new IPSubnet(
                network.ToString(),
                mask.ToString(),
                broadcast.ToString(),
                firstHost.ToString(),
                lastHost.ToString(),
                usableHosts
            );
            return subnet;
        }

        // CIDR Notation Converter
        public string ConvertCIDR(string cidr)
        {
            if (!int.TryParse(cidr, out var prefix) || prefix < 0 || prefix > 32)
            {
                return "Invalid CIDR prefix.";
            }

            var mask = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                mask.Append(i < prefix ? '1' : '0');
                if ((i + 1) % 8 == 0 && i != 31)
                {
                    mask.Append('.');
                }
            }

            var binaryMask = mask.ToString();
            var decimalMask = string.Join(".", Enumerable.Range(0, 4).Select(i => Convert.ToInt32(binaryMask.Substring(i * 9, 8), 2)));

            return $"CIDR: /{prefix}\nSubnet Mask: {decimalMask}\nBinary: {binaryMask}";
        }

        // IP Range Generator
        public List<string> GenerateIPRange(string startIP, string endIP)
        {
            if (!IPAddress.TryParse(startIP, out var start) || !IPAddress.TryParse(endIP, out var end))
            {
                throw new ArgumentException("Invalid IP addresses.");
            }

            var result = new List<string>();
            uint startInt = BitConverter.ToUInt32(start.GetAddressBytes().Reverse().ToArray(), 0);
            uint endInt = BitConverter.ToUInt32(end.GetAddressBytes().Reverse().ToArray(), 0);

            for (uint i = startInt; i <= endInt; i++)
            {
                var bytes = BitConverter.GetBytes(i).Reverse().ToArray();
                result.Add(new IPAddress(bytes).ToString());
            }

            return result;
        }

        // IPv6 Address Converter
        public string ConvertIPv6(string ipv6Address, bool compress = true)
        {
            if (!IPAddress.TryParse(ipv6Address, out var ip) || ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return "Invalid IPv6 address.";
            }

            if (compress)
            {
                return ip.ToString(); // .NET automatically compresses IPv6
            }
            else
            {
                // Expand IPv6 to full form
                var bytes = ip.GetAddressBytes();
                var groups = new List<string>();
                for (int i = 0; i < 16; i += 2)
                {
                    groups.Add($"{bytes[i]:x2}{bytes[i + 1]:x2}");
                }
                return string.Join(":", groups);
            }
        }

        // MAC Address Vendor Lookup (simplified version)
        public string LookupMACVendor(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress) || macAddress.Length < 8)
            {
                return "Invalid MAC address.";
            }

            // Extract OUI (first 3 octets)
            string oui = macAddress.Replace(":", "").Replace("-", "").Substring(0, 6).ToUpper();
            
            // Simplified vendor lookup (in a real implementation, you'd use a database)
            var vendors = new Dictionary<string, string>
            {
                {"001B63", "Apple"},
                {"00D0C9", "Intel"},
                {"0050C2", "IEEE Registration Authority"},
                {"001A3F", "Netgear"},
                {"002719", "Elitegroup Computer Systems"},
                {"0026B9", "Cisco"},
                {"00E04C", "Realtek"},
                {"001EDE", "Canon"},
                {"001C42", "Dell"},
                {"0013D3", "Micro-Star International"}
            };

            return vendors.ContainsKey(oui) ? $"Vendor: {vendors[oui]} (OUI: {oui})" : $"Unknown vendor (OUI: {oui})";
        }
    }
}