﻿using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("dealers")]
    public class Dealer
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
}
