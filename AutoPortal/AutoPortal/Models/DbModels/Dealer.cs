using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("dealers")]
    public class Dealer
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
    }
}
