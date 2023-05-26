using AutoPortal.Libs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("transmissiontypes")]
    public partial class TransmissionType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string transmission { get; set; }
    }

    public partial class TransmissionType
    {
        public static int? findIdByTransmission(string transmission)
        {
            using (SQL mysql = new SQL())
            {
                return mysql.transmissionTypes.SingleOrDefault(t => t.transmission == transmission).id;
            }
        }
    }
}