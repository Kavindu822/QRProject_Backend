using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRFileTrackingapi.Models.Entities
{
    public class RcodeFileHistory
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        public string Rcode { get; set; }  // File Code

        [Required]
        public string PreviousEpfNo { get; set; }  // Previous User

        [Required]
        public string NewEpfNo { get; set; }  // New User

        public string PreviousEName { get; set; }  // Previous User Name
        public string NewEName { get; set; }  // New User Name

        public string PreviousContactNo { get; set; }
        public string NewContactNo { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime TransferDate { get; set; }  // Date of Transfer
    }
}



//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace QRFileTrackingapi.Models.Entities
//{
//    public class RcodeFile
//    {
//        [Key]
//        [StringLength(50)]  // Optional: Define length for Rcode
//        public string Rcode { get; set; }

//        [Required]
//        [StringLength(100)]  // Optional: Define length for employee name
//        public string EName { get; set; }

//        [Required]
//        [StringLength(10)]  // Optional: Define length for EPF No
//        public string EpfNo { get; set; }

//        [StringLength(15)]  // Optional: Define length for ContactNo
//        public string ContactNo { get; set; }

//        [StringLength(20)]  // Optional: Define length for Status
//        public string Status { get; set; }

//        [Column(TypeName = "datetime2")]
//        public DateTime GetDate { get; set; }

//        [Column(TypeName = "datetime2")]
//        public DateTime UpdateDate { get; set; }
//    }

//    public class RcodeFileHistory
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int HistoryId { get; set; }  // Auto-increment primary key

//        [Required]
//        public string Rcode { get; set; }  // Foreign key for Rcode

//        [ForeignKey("Rcode")]  // Establish relationship between RcodeFileHistory and RcodeFile
//        public RcodeFile RcodeFile { get; set; }  // Navigation property to RcodeFile

//        [StringLength(100)]  // Optional: Define length for employee name
//        public string EName { get; set; }

//        [StringLength(10)]  // Optional: Define length for EPF No
//        public string EpfNo { get; set; }

//        [StringLength(15)]  // Optional: Define length for ContactNo
//        public string ContactNo { get; set; }

//        [StringLength(20)]  // Optional: Define length for Status
//        public string Status { get; set; }

//        [Column(TypeName = "datetime2")]
//        public DateTime? GetDate { get; set; }  // Nullable, as it may not always be set

//        [Column(TypeName = "datetime2")]
//        public DateTime UpdateDate { get; set; } = DateTime.UtcNow; // Auto-set current time

//        [Required]
//        [StringLength(50)]  // Optional: Define length for ActionType
//        public string ActionType { get; set; } // INSERT, UPDATE, DELETE
//    }
//}



//using System;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace QRFileTrackingapi.Models.Entities
//{
//    public class RcodeFileHistory
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int HistoryId { get; set; }  // Auto-increment primary key

//        public string Rcode { get; set; }
//        public string EName { get; set; }
//        public string EpfNo { get; set; }
//        public string ContactNo { get; set; }
//        public string Status { get; set; }

//        [Column(TypeName = "datetime2")]
//        public DateTime? GetDate { get; set; }

//        [Column(TypeName = "datetime2")]
//        public DateTime UpdateDate { get; set; } = DateTime.UtcNow; // Auto-set current time

//        [Required]
//        public string ActionType { get; set; } // INSERT, UPDATE, DELETE
//    }
//}
