namespace ProjectAsad.Model
{
    public class IPAnalyzeResult
    {
        public IPAnalyzeResult(string ipAddress, string ipClass, string publicPrivate, string binaryRepresentation)
        {
            IPAddress = ipAddress;
            IPClass = ipClass;
            PublicPrivate = publicPrivate;
            BinaryRepresentation = binaryRepresentation;
        }
        public string IPAddress { get; set; }
        public string IPClass { get; set; }
        public string PublicPrivate { get; set; }
        public string BinaryRepresentation { get; set; }
    }
}