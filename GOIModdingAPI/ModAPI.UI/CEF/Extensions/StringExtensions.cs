namespace ModAPI.UI.CEF.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (str.Length <= 1)
                return str.ToLowerInvariant();

            return str[0].ToString().ToLowerInvariant() + str.Substring(1);
        }
    }
}