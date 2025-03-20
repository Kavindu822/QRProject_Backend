namespace QRFileTrackingapi.Models
{
    public class AddUserAccountDto
    {
        public required string EpfNo { get; set; }
        public required string Name { get; set; }
        public required string Phone { get; set; }
        public required string PasswordHash { get; set; }
        public required string Department { get; set; }
        public required string SeatNo { get; set; }
        public required string Role { get; set; }
    }
}
