using System.Text;

namespace PactNet.Infrastructure.Outputters
{
    public class OutputBuilder : IOutputBuilder
    {
        private readonly StringBuilder _outputBuilder = new StringBuilder();

        public void WriteLine(string line)
        {
            _outputBuilder.AppendLine(line);
        }

        public void Clear()
        {
            _outputBuilder.Clear();
        }

        public override string ToString()
        {
            return _outputBuilder.ToString();
        }
    }
}
