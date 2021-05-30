using System;
using System.Collections.Generic;
using PactNet.Infrastructure.Outputters;

namespace PactNet
{
    public class PactVerifierConfig
    {
        public IEnumerable<IOutput> Outputters { get; set; }
        
        public bool PublishVerificationResults { get; set; }

        public string ProviderVersion { get; set; }

        private KeyValuePair<string, string>? _customHeader;
        [Obsolete("Please use CustomHeaders instead")]
        public KeyValuePair<string, string>? CustomHeader { // backward compatibility
            get => _customHeader;
            set
            {
                if (value != null)
                {
                    CustomHeaders.Add(value.Value.Key, value.Value.Value);
                }
                if (_customHeader != null)
                {
                    CustomHeaders.Remove(_customHeader.Value.Key);
                }
                _customHeader = value;
            }
        }
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>();

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