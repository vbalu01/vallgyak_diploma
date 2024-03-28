using AutoPortal.Models.AppModels;
using System.ComponentModel;

namespace AutoPortal.Libs
{
    public static class EnumHelper
    {
        public static string GetServiceTypeString(eServiceType type) {
            var enumType = typeof(eServiceType);
            var memberInfos = enumType.GetMember(type.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)valueAttributes[0]).Description;
        }

        public static string GetAccountstatusString(eAccountStatus status)
        {
            var enumType = typeof(eAccountStatus);
            var memberInfos = enumType.GetMember(status.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)valueAttributes[0]).Description;
        }

        public static string GetOwnershipTypeString(eVehiclePermissions permission)
        {
            var enumType = typeof(eVehiclePermissions);
            var memberInfos = enumType.GetMember(permission.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((DescriptionAttribute)valueAttributes[0]).Description;
        }

        public static List<string> GetStatusStringList(eAccountStatus s)
        {
            List<string> statuses = new();
            foreach(eAccountStatus status in Enum.GetValues(typeof(eAccountStatus)))
            {
                if (s.HasFlag(status))
                {
                    if(status != eAccountStatus.None)
                        statuses.Add(GetAccountstatusString(status));
                }
            }
            return statuses;
        }
    }
}
