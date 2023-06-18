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
    }
}
