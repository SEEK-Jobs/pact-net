using System;

namespace PactNet.Native
{
    /// <summary>
    /// Defines the message scenario builder interface
    /// </summary>
    public interface IMessageScenarioBuilder
    {
        /// <summary>
        /// Set the description of the scenario
        /// </summary>
        /// <param name="description">the name of the interaction</param>
        /// <returns>Fluent builder</returns>
        IMessageScenarioBuilder WhenReceiving(string description);

        /// <summary>
        /// Set the action of the scenario
        /// </summary>
        /// <param name="action">the function invoked</param>
        void WillPublishMessage(Func<dynamic> action);
    }
}
