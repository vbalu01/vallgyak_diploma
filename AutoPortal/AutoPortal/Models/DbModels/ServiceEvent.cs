﻿using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("serviceevents")]
    public class ServiceEvent
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public int service_id { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public int mileage { get; set; }
        [Required]
        public eServiceType serviceType { get; set; }
    }
}
