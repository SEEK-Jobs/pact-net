using PactNet.Mocks.Models;

namespace PactNet.Mocks
{
    public interface IMockProvider<out TMockProviderInterface, in TMockProviderMessage>
        where TMockProviderInterface : IMockProvider<TMockProviderInterface, TMockProviderMessage>
        where TMockProviderMessage : IMessage
    {
        TMockProviderInterface Given(string providerState);
        TMockProviderInterface With(TMockProviderMessage request);

        void Start();
        void Stop();
    }
}