using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService
{
    public interface IMockProviderService : IMockProvider<IMockProviderService, ProviderServiceRequest>
    {
        IMockProviderService UponReceiving(string description);
        void WillRespondWith(ProviderServiceResponse response);
        void ClearInteractions();
        void VerifyInteractions();
        void SendAdminHttpRequest(HttpVerb method, string path);
    }
}