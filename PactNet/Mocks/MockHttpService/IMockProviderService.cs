using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService
{
    public interface IMockProviderService : IMockProvider<IMockProviderService>
    {
        IMockProviderService With(ProviderServiceRequest request);
#if USE_STACKTRACE
        void WillRespondWith(ProviderServiceResponse response);
#elif USE_CALLER_INFO
        void WillRespondWith(
            ProviderServiceResponse response, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", 
            [CallerLineNumber] int callerLineNumber = 0);
#endif
        void Start();
        void Stop();
        void ClearInteractions();
        void VerifyInteractions();
        void SendAdminHttpRequest<T>(HttpVerb method, string path, T requestContent, IDictionary<string, string> headers = null) where T : class;
    }
}