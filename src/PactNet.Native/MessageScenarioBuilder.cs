using System;
using System.Collections.Generic;

namespace PactNet.Native
{
    /// <summary>
    /// Defines the message scenario builder
    /// </summary>
    public class MessageScenarioBuilder : IMessageScenarioBuilder
    {
        /// <summary>
        /// Handles to create a new scenario with scenario builder
        /// </summary>
        public static IMessageScenarioBuilder NewScenario => new MessageScenarioBuilder();

        /// <summary>
        /// temporary description
        /// </summary>
        private string _descriptionAdded;

        /// <summary>
        /// tag to know if we are creating a scenario
        /// </summary>
        private bool _settingScenario;

        private MessageScenarioBuilder()
        {
            Scenarios.All ??= new Dictionary<string, Func<dynamic>>();
        }

        /// <inheritdoc />
        public IMessageScenarioBuilder WhenReceiving(string description)
        {
            try
            {
                ValidateDescription(description);

                _settingScenario = true;
                _descriptionAdded = description;

                return this;
            }
            catch (Exception)
            {
                ClearScenario();
                throw;
            }
        }

        /// <inheritdoc />
        public void WillPublishMessage(Func<dynamic> action)
        {
            try
            {
                ValidateAction(action);

                Scenarios.All[_descriptionAdded] = action;
            }
            finally
            {
                ClearScenario();
            }
        }

        /// <summary>
        /// Validates we can add a scenario description
        /// </summary>
        /// <param name="description">The description to add</param>
        private void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (Scenarios.All.TryGetValue(description, out _))
            {
                throw new InvalidOperationException($"NewScenario called \"{description}\" has already been added");
            }

            if (_settingScenario)
            {
                throw new InvalidOperationException("You need to set the scenario action before adding another scenario");
            }
        }

        /// <summary>
        /// Validates we can add a scenario action
        /// </summary>
        /// <param name="action">The action to add</param>
        private void ValidateAction(Func<dynamic> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!_settingScenario)
            {
                throw new InvalidOperationException("You need to set the scenario description before the action");
            }
        }

        /// <summary>
        /// Clearing state after setting scenario
        /// </summary>
        private void ClearScenario()
        {
            _settingScenario = false;
            _descriptionAdded = string.Empty;
        }
    }
}
