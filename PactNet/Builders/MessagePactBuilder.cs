using System;
using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Core;
using PactNet.PactMessage;
using PactNet.PactMessage.Host;
using PactNet.PactMessage.Host.Commands;
using PactNet.PactMessage.Models;
using PactNet.Wrappers;
using static System.String;

namespace PactNet
{
    public class MessagePactBuilder : IMessagePactBuilder
    {
        public string ConsumerName { get; private set; }
        public string ProviderName { get; private set; }
        public PactConfig PactConfig { get; }
        private readonly Func<string, string, IMessagePact> _pactMessageFactory;
        private readonly Func<string, string, PactConfig, MessageInteraction, Func<PactMessageHostConfig, IPactCoreHost>, IUpdateCommand> _updateCommandFactory;
        private IFileWrapper _fileWrapper;
        private IMessagePact _messagePact;

        public MessagePactBuilder()
            : this(new PactConfig
            { SpecificationVersion = "2.0.0" })
        {
        }

        public MessagePactBuilder(PactConfig config)
            : this(config, JsonConfig.ApiSerializerSettings)
        {
        }

        public MessagePactBuilder(PactConfig config, JsonSerializerSettings jsonSerializerSettings)
            : this(config, (consumerName, providerName) => new MessagePact(jsonSerializerSettings),
                (consumer, provider, pactConfig, messageInteraction, coreHostFactory) => new UpdateCommand(consumer, provider, pactConfig, messageInteraction, coreHostFactory, jsonSerializerSettings),
                new FileWrapper()
                )
        {
        }

        internal MessagePactBuilder(PactConfig pactConfig,
            Func<string, string, IMessagePact> pactMessageFactory,
            Func<string, string, PactConfig, MessageInteraction, Func<PactMessageHostConfig, IPactCoreHost>, IUpdateCommand> updateCommandFactory,
            IFileWrapper fileWrapper)
        {
            PactConfig = pactConfig;
            _pactMessageFactory = pactMessageFactory;
            _updateCommandFactory = updateCommandFactory;
            _fileWrapper = fileWrapper;
        }

        public IMessagePactBuilder ServiceConsumer(string consumerName)
        {
            if (IsNullOrEmpty(consumerName))
            {
                throw new ArgumentException("Please supply a non null or empty consumerName");
            }

            ConsumerName = consumerName;

            return this;
        }

        public IMessagePactBuilder HasPactWith(string providerName)
        {
            if (IsNullOrEmpty(providerName))
            {
                throw new ArgumentException("Please supply a non null or empty providerName");
            }

            ProviderName = providerName;

            return this;
        }

        public void Build()
        {
            if (_messagePact == null)
            {
                throw new InvalidOperationException("The Pact file could not be saved because the pact message is not initialised. Please initialise by calling the CreatePactMessage() method.");
            }

            //If a file with the previous interactions exists, it has to be deleted so unexpected interactions would not be in the new file
            if (_fileWrapper.Exists(GetPactFilePath(PactConfig.PactDir, ConsumerName, ProviderName)))
            {
                _fileWrapper.Delete(GetPactFilePath(PactConfig.PactDir, ConsumerName, ProviderName));
            }

            foreach (var messageInteraction in _messagePact.MessageInteractions)
            {
                var updateCommand = _updateCommandFactory(ConsumerName, ProviderName, PactConfig, messageInteraction, config => new PactCoreHost<PactMessageHostConfig>(config));
                updateCommand.Execute();
            }

        }
        private static string GetPactFilePath(string pactDir, string consumerName, string producerName)
        {
            var filePath = $"{pactDir}{consumerName.Replace(" ", "_")}-{producerName.Replace(" ", "_")}.json";
            return filePath;
        }

        public IMessagePact InitializePactMessage()
        {
            if (IsNullOrEmpty(ConsumerName))
            {
                throw new InvalidOperationException("ConsumerName has not been set, please supply a consumer name using the ServiceConsumer method.");
            }

            if (IsNullOrEmpty(ProviderName))
            {
                throw new InvalidOperationException("ProviderName has not been set, please supply a provider name using the HasPactWith method.");
            }

            _messagePact = _pactMessageFactory(ConsumerName, ProviderName);

            return _messagePact;
        }
    }
}
