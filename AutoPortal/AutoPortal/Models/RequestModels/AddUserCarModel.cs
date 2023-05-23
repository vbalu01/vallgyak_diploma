namespace AutoPortal.Models.RequestModels
{
    public class AddUserCarModel
    {
        public string chassis_number { get; set; }
        public string engine_number { get; set; }
        public string license { get; set; }
        public string engine_code { get; set; }
        public string category { get; set; }
        public int? manufact_year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string fuel { get; set; }
        public string transmission { get; set; }
        public string drive { get; set; }
        public int? engine_ccm { get; set; }
        public int? performance { get; set; }
        public int? torque { get; set; }
        public string body { get; set; }
        public int? num_of_doors { get; set; }
        public int? num_of_seats { get; set; }
        public int? weight { get; set; }
        public int? max_weight {get; set; }
    }
}
