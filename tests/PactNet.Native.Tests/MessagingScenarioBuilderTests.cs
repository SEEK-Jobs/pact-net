using System;

using FluentAssertions;

using Xunit;

namespace PactNet.Native.Tests
{
    /// <summary>
    /// Defines the messaging scenario builder tests
    /// </summary>
    public class MessagingScenarioBuilderTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WhenReceiving_Adding_Invalid_Scenario_Throws_Exception(string namedInteraction)
        {
            MessageScenarioBuilder.NewScenario
                .Invoking(x => x.WhenReceiving(namedInteraction))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WillPublishMessage_With_Null_Action_Throws_Exception()
        {
            MessageScenarioBuilder.NewScenario
                .Invoking(x => x.WhenReceiving("an interaction with null action").WillPublishMessage(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenReceiving_Adding_Scenario_With_Same_Name_Throws_Exception()
        {
            var namedInteraction = "same name interaction";

            var expectedMessage = new { id = 1, description = "a description" };

            var messageScenarioBuilder = MessageScenarioBuilder.NewScenario;
            messageScenarioBuilder.WhenReceiving(namedInteraction).WillPublishMessage(() => expectedMessage);

            messageScenarioBuilder
                .Invoking(x => x.WhenReceiving(namedInteraction))
                .Should().Throw<InvalidOperationException>($"NewScenario called \"{namedInteraction}\" has already been added");
        }

        [Fact]
        public void WhenReceiving_Twice_Throws_Exception()
        {
            MessageScenarioBuilder.NewScenario
                .Invoking(x => x.WhenReceiving("receiving first time").WhenReceiving("receiving second time"))
                .Should().Throw<InvalidOperationException>("You need to set the scenario action before adding another scenario");
        }

        [Fact]
        public void WhenReceiving_Adding_Scenario_Action_Without_Description_Throws_Exception()
        {
            var expectedMessage = new { id = 1, description = "a description" };

            MessageScenarioBuilder.NewScenario
                .Invoking(x => x.WillPublishMessage(() => expectedMessage))
                .Should().Throw<InvalidOperationException>("You need to set the scenario description before the action");
        }
    }
}
