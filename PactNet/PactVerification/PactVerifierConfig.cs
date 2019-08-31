using System.Collections.Generic;
using PactNet.Infrastructure.Outputters;

namespace PactNet
{
    public class PactVerifierConfig
    {
        public IEnumerable<IOutput> Outputters { get; set; }

        public bool PublishVerificationResults { get; set; }

        public string ProviderVersion { get; set; }

        public KeyValuePair<string, string>? CustomHeader { get; set; }

        public bool Verbose { get; set; }

        public string MonkeyPatchFile { get; set; }

        public PactVerifierConfig()
        {
            Outputters = new List<IOutput>
            {
                new ConsoleOutput()
            };
        }
    }
}