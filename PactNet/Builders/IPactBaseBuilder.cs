namespace PactNet
{
    public interface IPactBaseBuilder<out T> where T : IPactBaseBuilder<T>
    {
        T ServiceConsumer(string consumerName);
        T HasPactWith(string providerName);
        void Build();

    }
}
