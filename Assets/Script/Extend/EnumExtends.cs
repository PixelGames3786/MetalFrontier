using System;

public static class EnumExtensions
{
    public static T Next<T>(this T enumValue) where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        int index = (Array.IndexOf(values, enumValue) + 1) % values.Length;
        return (T)values.GetValue(index);
    }
}