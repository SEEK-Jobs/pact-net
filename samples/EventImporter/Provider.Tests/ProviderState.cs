using System.Collections.Generic;

namespace Provider.Tests
{
    public class ProviderState
    {
        public string State { get; set; }

        public IDictionary<string, string> Params { get; set; }
    }
}
