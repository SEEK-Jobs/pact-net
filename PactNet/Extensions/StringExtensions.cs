namespace PactNet.Extensions
{
    public static class StringExtensions
    {
        public static string ToLowerSnakeCase(this string input)
        {
            return !string.IsNullOrEmpty(input) ?
                input.Replace(' ', '_').ToLower() :
                string.Empty;
        }

        public static string EscapeDoubleQuotes(this string input)
        {
            return !string.IsNullOrEmpty(input) ?
                input.Replace("\"", "\\\"") :
                string.Empty;
        }
    }
}