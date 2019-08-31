using System.Collections.Generic;

namespace PactNet.Mocks.Models
{
    public interface IMessage
    {
        IDictionary<string, object> Headers { get; set; }
        dynamic Body { get; set; }
    }
}