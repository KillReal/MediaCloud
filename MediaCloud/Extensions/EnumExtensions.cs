using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MediaCloud.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetType().GetMember(enumValue.ToString())
                                        .First()
                                        .GetCustomAttribute<DisplayAttribute>();

            return attribute == null 
                ? "" 
                : attribute.GetName() ?? "";
        }
    }
}
