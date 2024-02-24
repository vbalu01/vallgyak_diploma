using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;

namespace AutoPortal.Models.ResponseModels
{
    public class SaleVehicleInfoModel
    {
        public SaleVehicle sale {  get; set; }
        public Vehicle vehicle { get; set; }
        public List<string> images { get; set; }
        public List<ServiceEvent> services { get; set; }
        public List<MileageStandModel> mileages { get; set; }

        public SaleVehicleInfoModel()
        {
            images = new();
            services = new();
            mileages = new();
        }
    }
}
