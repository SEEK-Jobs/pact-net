using PactNet.Core;
using PactNet.Infrastructure.Outputters;
using PactNet.PactMessage.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PactNet.Configuration.Json;

namespace PactNet.PactMessage.Host.Commands
{
    internal class ReifyCommand : IReifyCommand
    {
        private readonly MessageInteraction _messageInteraction;
        private readonly IOutputBuilder _outputBuilder;
        private readonly Func<PactMessageHostConfig, IPactCoreHost> _coreHostFactory;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public ReifyCommand(MessageInteraction messageInteraction, IOutputBuilder outputBuilder, Func<PactMessageHostConfig, IPactCoreHost> coreHostFactory, JsonSerializerSettings jsonSerializerSettings)
        {
            _messageInteraction = messageInteraction;
            _outputBuilder = outputBuilder;
            _coreHostFactory = coreHostFactory;
            _jsonSerializerSettings = jsonSerializerSettings ?? JsonConfig.ApiSerializerSettings;
        }

        public void Execute()
        {
            var messageJson = JsonConvert.SerializeObject(_messageInteraction.Contents, _jsonSerializerSettings);

            var arguments = $"reify '{messageJson}'";
            var pactConfig = new PactConfig
            {
                Outputters = new List<IOutput> { _outputBuilder },
            };
            var coreHostConfig = new PactMessageHostConfig(pactConfig, arguments);

            var coreHost = _coreHostFactory(coreHostConfig);
            coreHost.Start();
        }
    }
}
