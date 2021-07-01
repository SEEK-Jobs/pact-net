using System;

namespace PactNet.Native
{
    /// <summary>
    /// Pact message verifier
    /// </summary>
    public class MessagePactVerifier : PactVerifier
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MessagePactVerifier"/> class.
        /// </summary>
        public MessagePactVerifier()
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="MessagePactVerifier"/> class.
        /// </summary>
        /// <param name="config">Verifier configuration</param>
        public MessagePactVerifier(PactVerifierConfig config) : base(config)
        {
        }

        /// <summary>
        /// Set the base path for the message interaction model
        /// </summary>
        /// <param name="pactUri">the host uri</param>
        protected internal override void SetBasePath(Uri pactUri)
        {
            verifierArgs.Add("--base-path");

            var messageBasePath = pactUri.AbsolutePath != "/"
                ? pactUri.AbsolutePath + Constants.PactMessagesPath
                : Constants.PactMessagesPath;

            verifierArgs.Add(messageBasePath);
        }
    }
}
