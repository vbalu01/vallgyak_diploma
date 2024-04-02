using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("dealers")]
    public partial class Dealer
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string password { get; set; }
        public string description { get; set; }
        [Required]
        public string country { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public string address { get; set; }
        public string website { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
    }

    public partial class Dealer
    {
        public static double GetDealerReviewAvg(int dealerId)
        {
            using(SQL mysql = new SQL())
            {
                if(mysql.reviews.Any(r=>r.target_type == eVehicleTargetTypes.DEALER && r.target_id == dealerId))
                {
                    return mysql.reviews.Where(r => r.target_type == eVehicleTargetTypes.DEALER && r.target_id == dealerId).Average(d => d.rating);
                }
                else
                {
                    return 1.0;
                }
            }
        }
    }
}
