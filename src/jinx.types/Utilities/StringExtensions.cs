namespace jinx.types.Utilities
{
    public static class StringExtensions
    {
        public static string ToLowerCamelCase(this string str)
        {
            var chars = str.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}
