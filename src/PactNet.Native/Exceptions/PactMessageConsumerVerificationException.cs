using System;

namespace PactNet.Native.Exceptions
{
    /// <summary>
    /// Defines the pact message consumer verification exception
    /// </summary>
    [Serializable]
    public class PactMessageConsumerVerificationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PactMessageConsumerVerificationException" /> class
        /// </summary>
        public PactMessageConsumerVerificationException()
            : this("The message could not be verified by the consumer handler")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PactMessageConsumerVerificationException" /> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public PactMessageConsumerVerificationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PactMessageConsumerVerificationException" /> class
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified</param>
        public PactMessageConsumerVerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
