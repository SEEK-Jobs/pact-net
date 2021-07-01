using FluentAssertions;

using Xunit;

namespace PactNet.Native.Tests
{
    /// <summary>
    /// Defines the scenarios tests
    /// </summary>
    public class ScenariosTests
    {
        private readonly IMessageScenarioBuilder _messageScenarioBuilder;

        public ScenariosTests()
        {
            _messageScenarioBuilder = MessageScenarioBuilder.NewScenario;
        }

        [Fact]
        public void WhenReceiving_Adding_Scenario_Without_Action_Does_Not_Sets_Scenario()
        {
            var namedInteraction = "a wrong interaction";

            _messageScenarioBuilder.WhenReceiving(namedInteraction);

            object actualMessage = Scenarios.InvokeScenario(namedInteraction);

            actualMessage.Should().BeNull();
        }

        [Fact]
        public void InvokeScenario_After_Setting_Scenario_Description_And_Action()
        {
            var namedInteraction = "a named interaction";

            var expectedMessage = new { id = 1, description = "a description" };

            _messageScenarioBuilder.WhenReceiving(namedInteraction).WillPublishMessage(() => expectedMessage);

            object actualMessage = Scenarios.InvokeScenario(namedInteraction);

            actualMessage.Should().BeEquivalentTo(expectedMessage);
        }

        [Fact]
        public void InvokeScenario_With_Wrong_Description_Returns_Null()
        {
            var namedInteraction = "a new interaction";

            var expectedMessage = new { id = 1, description = "a description" };

            _messageScenarioBuilder.WhenReceiving(namedInteraction).WillPublishMessage(() => expectedMessage);

            object actualMessage = Scenarios.InvokeScenario("another interaction");

            actualMessage.Should().BeNull();
        }
    }
}
