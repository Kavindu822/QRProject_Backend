using QRFileTrackingapi.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;

public class RcodeFile
{
    [Key]
    [Required]
    public string Rcode { get; set; }

    [Required]
    [MaxLength(100)]
    public string EName { get; set; }

    [Required]
    [MaxLength(20)]
    public string EpfNo { get; set; }

    [Required]
    [MaxLength(15)]
    public string ContactNo { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; }

    [Required]
    public DateTime GetDate { get; set; } = DateTime.UtcNow;
}
