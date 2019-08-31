using System.Collections.Generic;
using PactNet.Core;
using PactNet.Infrastructure.Outputters;

namespace PactNet.PactMessage.Host
{
    internal class PactMessageHostConfig : IPactCoreHostConfig
    {
        public string Script { get; }
        public string Arguments { get; }
        public bool WaitForExit { get; }
        public IEnumerable<IOutput> Outputters { get; }
        public IDictionary<string, string> Environment { get; }

        public PactMessageHostConfig(PactConfig pactConfig, string arguments)
        {
            Outputters = pactConfig.Outputters;
            Script = "pact-message";

            Arguments = arguments;

            WaitForExit = true;
        }
    }
}
