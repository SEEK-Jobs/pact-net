using System;
using System.IO;

namespace PactNet
{
    internal static class Constants
    {
        public const string AdministrativeRequestHeaderKey = "X-Pact-Mock-Service";
        public const string InteractionsPath = "/interactions";
        public const string InteractionsVerificationPath = "/interactions/verification";
        public const string PactPath = "/pact";

#if USE_NET4X
        public static string BuildDirectory = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""))).LocalPath;
        public static string DefaultPactDir = Path.GetFullPath($"{BuildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
        public static string DefaultLogDir = Path.GetFullPath($"{BuildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}");
#else
        public static string BuildDirectory = AppContext.BaseDirectory;
        public static string DefaultPactDir = Path.GetFullPath($"{BuildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
        public static string DefaultLogDir = Path.GetFullPath($"{BuildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}");
#endif
    }
}
