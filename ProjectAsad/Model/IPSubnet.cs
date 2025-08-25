namespace ProjectAsad.Model
{
    public class IPSubnet
    {
        public IPSubnet(string networkAddress,
                        string subnetMask,
                        string broadcastAddress,
                        string minHost,
                        string maxHost,
                        int usableHosts)
        {
            NetworkAddress = networkAddress;
            SubnetMask = subnetMask;
            BroadcastAddress = broadcastAddress;
            MinHost = minHost;
            MaxHost = maxHost;
            UsableHosts = usableHosts;
        }
        public string NetworkAddress { get; set; }
        public string SubnetMask { get; set; }
        public string BroadcastAddress { get; set; }
        public string MinHost { get; set; }
        public string MaxHost { get; set; }
        public int UsableHosts { get; set; }
    }
}