using System;
using System.Threading.Tasks;

namespace PactNet
{
    /// <summary>
    /// Pact Builder
    /// </summary>
    public interface IPactBuilder
    {
        /// <summary>
        /// Verify the configured interactions
        /// </summary>
        /// <param name="interact">Action to perform the real interactions against the mock server</param>
        /// <exception cref="PactFailureException">Failed to verify the interactions</exception>
        void Verify(Action<IConsumerContext> interact);

        /// <summary>
        /// Verify the configured interactions
        /// </summary>
        /// <param name="interact">Action to perform the real interactions against the mock server</param>
        /// <exception cref="PactFailureException">Failed to verify the interactions</exception>
        Task VerifyAsync(Func<IConsumerContext, Task> interact);
    }

    /// <summary>
    /// Pact v2 Builder
    /// </summary>
    public interface IPactBuilderV2 : IPactBuilder
    {
        /// <summary>
        /// Add a new interaction to the pact
        /// </summary>
        /// <param name="description">Interaction description</param>
        /// <returns>Fluent builder</returns>
        IRequestBuilderV2 UponReceiving(string description);
    }

    /// <summary>
    /// Pact v3 Builder
    /// </summary>
    public interface IPactBuilderV3 : IPactBuilder
    {
        /// <summary>
        /// Add a new interaction to the pact
        /// </summary>
        /// <param name="description">Interaction description</param>
        /// <returns>Fluent builder</returns>
        IRequestBuilderV3 UponReceiving(string description);
    }
}
