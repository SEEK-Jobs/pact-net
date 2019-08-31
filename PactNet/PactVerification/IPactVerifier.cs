namespace PactNet
{
    public interface IPactVerifier
    {
        IPactVerifier ProviderState(string providerStateSetupUri);
        IPactVerifier ServiceProvider(string providerName, string baseUri);
        IPactVerifier HonoursPactWith(string consumerName);
        IPactVerifier PactUri(string fileUri, PactUriOptions options = null);
        void Verify(string description = null, string providerState = null);
    }
}