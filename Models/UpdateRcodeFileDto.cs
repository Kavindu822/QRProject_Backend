namespace QRFileTrackingapi.Models
{
    public class UpdateRcodeFileDto
    {
        public required string Rcode { get; set; }
        public required string EName { get; set; }
        public required string EpfNo { get; set; }
        public required string ContactNo { get; set; }
        public required string Status { get; set; }
        public required DateTime GetDate { get; set; }
        public string Role { get; set; } // New property
    }

}
