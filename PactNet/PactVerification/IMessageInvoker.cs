namespace PactNet.PactVerification
{
    public interface IMessageInvoker
    {
        object Invoke(MessagePactDescription description);
    }
}
