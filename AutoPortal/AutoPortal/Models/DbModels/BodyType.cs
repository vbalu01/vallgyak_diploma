using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPortal.Models.DbModels
{
    [Table("bodytypes")]
    public class BodyType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string body { get; set; }
    }
}
