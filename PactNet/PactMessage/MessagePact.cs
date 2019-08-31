using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PactNet.Core;
using PactNet.Infrastructure.Outputters;
using PactNet.PactMessage.Host;
using PactNet.PactMessage.Host.Commands;
using PactNet.PactMessage.Models;

namespace PactNet.PactMessage
{
	public class MessagePact : IMessagePact
	{
		private readonly IOutputBuilder _outputBuilder;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly Func<PactMessageHostConfig, IPactCoreHost> _coreHostFactory;
		private IEnumerable<ProviderState> _providerStates;
		private string _description;
		public IList<MessageInteraction> MessageInteractions { get; }

		private readonly Func<MessageInteraction, IOutputBuilder, Func<PactMessageHostConfig, IPactCoreHost>, IReifyCommand> _reifyCommandFactory;

		public MessagePact(JsonSerializerSettings jsonSerializerSettings = null) : this(
			(messageInteraction, builder, coreHostFactory) =>
				new ReifyCommand(messageInteraction, builder, coreHostFactory, jsonSerializerSettings),
				new OutputBuilder(),
				jsonSerializerSettings,
			    config => new PactCoreHost<PactMessageHostConfig>(config))
		{
		}

		internal MessagePact(Func<MessageInteraction, IOutputBuilder, Func<PactMessageHostConfig, IPactCoreHost>,
				IReifyCommand> reifyCommandFactory,
				IOutputBuilder outputBuilder,
				JsonSerializerSettings jsonSerializerSettings,
				Func<PactMessageHostConfig, IPactCoreHost> coreHostFactory)
		{
			_reifyCommandFactory = reifyCommandFactory;
			_outputBuilder = outputBuilder;
			_jsonSerializerSettings = jsonSerializerSettings;
			_coreHostFactory = coreHostFactory;
			MessageInteractions = new List<MessageInteraction>();
		}

		public IMessagePact ExpectedToReceive(string description)
		{
			if (string.IsNullOrEmpty(description))
			{
				throw new ArgumentException("Please supply a non null or empty description");
			}

			_description = description;

			return this;
		}

		public IMessagePact Given(IEnumerable<ProviderState> providerStates)
		{
			var providerStatesArray = providerStates?.ToArray();

			if (providerStatesArray == null || !providerStatesArray.Any())
			{
				throw new ArgumentException("Please supply a non null or empty providerStates");
			}

			_providerStates = providerStatesArray;

			return this;
		}

		public IMessagePact With(Message message)
		{
			if (message == null)
			{
				throw new ArgumentException("Please supply a non null message");
			}

			if (string.IsNullOrEmpty(_description))
			{
				throw new InvalidOperationException("description has not been set, please supply using the ExpectedToReceive method.");
			}

			MessageInteractions.Add(new MessageInteraction
			{
				Contents = message.Contents,
				ProviderStates = _providerStates,
				Description = _description,
			});

			return this;
		}

		public void VerifyConsumer<T>(Action<T> messageHandler)
		{
			foreach (var messageInteraction in MessageInteractions)
			{
				var reifyAction = _reifyCommandFactory(messageInteraction, _outputBuilder, _coreHostFactory);
				reifyAction.Execute();

				var message = _outputBuilder.ToString();
				if (message.StartsWith("ERROR"))
				{
					throw new PactFailureException($"Could not parse message. core error: {message}");
				}

				try
				{
					messageHandler(JsonConvert.DeserializeObject<T>(message, _jsonSerializerSettings));
				}
				catch (Exception e)
				{
					throw new PactFailureException($"could not handle the message {message}", e);
				}

				_outputBuilder.Clear();
			}
		}
	}
}
