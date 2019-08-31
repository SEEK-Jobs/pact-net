using PactNet.PactMessage;

namespace PactNet
{
    public interface IMessagePactBuilder : IPactBaseBuilder<IMessagePactBuilder>
    {
        IMessagePact InitializePactMessage();
    }
}
