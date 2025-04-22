using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QRFileTrackingapi.Models.Entities
{
    public class RcodeFile
    {
        [Key]
        public string Rcode { get; set; }

        [Required]
        public string EName { get; set; }

        [Required]
        public string EpfNo { get; set; }

        public string ContactNo { get; set; }
        public string Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime GetDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime UpdateDate { get; set; }

        // Add Department field
        public string Department { get; set; }
    }
}
