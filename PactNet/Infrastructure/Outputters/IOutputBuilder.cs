namespace PactNet.Infrastructure.Outputters
{
    internal interface IOutputBuilder : IOutput
    {
        void Clear();
        string ToString();
    }
}