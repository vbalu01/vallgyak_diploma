using AutoPortal.Models.DbModels;

namespace AutoPortal.Models.ResponseModels
{
    public class VehicleSaleModel
    {
        public Vehicle Vehicle { get; set; }
        public SaleVehicle SaleVehicle { get; set; }
        public List<string> images { get; set; }
    }
}
