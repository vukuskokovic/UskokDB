namespace UskokDB;

internal static class StringFirstLetterLowerCase
{
    internal static string FirstLetterLowerCase(this string str)
    {
        var length = str.Length;
        if (length == 0 || !char.IsUpper(str[0])) return str;
        
        if (length == 1) return str.ToLower();
        return char.ToLower(str[0]) + str.Substring(1);

    }
}