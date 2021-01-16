using PactNet.Core;
using System.Diagnostics;

namespace PactNet.Tests.Core
{
    internal class PactCoreHostSpy<T> : PactCoreHost<T> where T : IPactCoreHostConfig
    {
        public PactCoreHostSpy(T config) : base(config)
        {

        }

        public Process SpyRubyProcess => _process;
    }
}
