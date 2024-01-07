using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("reviews")]
    public partial class Review
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public eVehicleTargetTypes source_type { get; set; }
        [Required]
        public int source_id { get; set; }
        [Required]
        public eVehicleTargetTypes target_type { get; set; }
        [Required]
        public int target_id { get; set;}
        [Required]
        public int rating { get; set;}
        [Required]
        public string description { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public bool edited { get; set; }
    }

    public partial class Review
    {
        [NotMapped]
        public string writerName { get; set; }
        public void LoadReviewWriter()
        {
            using(SQL mysql = new SQL())
            {
                switch (source_type)
                {
                    case eVehicleTargetTypes.USER:
                        writerName = mysql.users.Single(u => u.id == source_id).name;
                    break;

					case eVehicleTargetTypes.DEALER:
						writerName = mysql.dealers.Single(d => d.id == source_id).name;
					break;

					case eVehicleTargetTypes.SERVICE:
						writerName = mysql.services.Single(s => s.id == source_id).name;
					break;
				}
            }
        }
    }
}
