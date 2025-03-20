namespace QRFileTrackingapi.Models
{
    // AddFileDto.cs
    public class AddRcodeFileDto
    {
        public required string Rcode { get; set; }
        public required string EName { get; set; }
        public required string EpfNo { get; set; }
        public required string ContactNo { get; set; }
        public required string Status { get; set; }
        public required DateTime GetDate { get; set; }
    }

}
