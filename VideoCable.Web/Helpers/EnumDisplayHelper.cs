using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace VideoCable.Web.Helpers;

public static class EnumDisplayHelper
{
    public static string GetDisplayName(this Enum value)
    {
        var memberInfo = value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault();

        if (memberInfo is null)
            return value.ToString();

        var displayAttribute = memberInfo
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? value.ToString();
    }
}