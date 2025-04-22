namespace QRFileTrackingapi.Models
{
    public class UpdateMultipleFilesDto
    {
        public List<string> Rcodes { get; set; }
        public string NewEpfNo { get; set; }
        public string NewEName { get; set; }

        public string NewContactNo { get; set; }
    }
}
