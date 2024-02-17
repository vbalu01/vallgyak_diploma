using AutoPortal.Libs;
using AutoPortal.Models.DbModels;
using System.Collections.Generic;

namespace AutoPortal.Models.AppModels
{
    public class SaleVehicleModel
    {
        public SaleVehicle Sale {  get; set; }
        public Vehicle Vehicle { get; set; }
        public string firstImage { get; set; }

        public static List<SaleVehicleModel> getSaleVehicles()
        {
            List<SaleVehicleModel> returnModel = new();
            using (SQL mysql = new SQL())
            {
                foreach(SaleVehicle sv in mysql.vehicleSales.Where(vs => vs.active == true && vs.announcement_date <= DateTime.Now).ToList())
                {
                    SaleVehicleModel svm = new();
                    svm.Sale = sv;
                    svm.Vehicle = mysql.vehicles.Single(v => v.chassis_number == sv.vehicle_id);
                    returnModel.Add(svm);
                }
            }

            return returnModel;
        }
    }
}
