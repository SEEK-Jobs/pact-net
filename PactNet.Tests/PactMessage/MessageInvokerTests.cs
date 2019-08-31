using System;
using System.Collections.Generic;
using PactNet.PactMessage.Models;
using PactNet.PactVerification;
using Xunit;

namespace PactNet.Tests.PactMessage
{
    public class MessageInvokerTests
    {
        private bool _firstStateWasCalled;
        private bool _secondStateWasCalled;

        public MessageInvokerTests()
        {
            _firstStateWasCalled = false;
            _secondStateWasCalled = false;
        }

        public MessageInvoker GetSubject()
        {
            var providerStates = new Dictionary<string, Action>
            {
                { "Test state 1", SetFirstState },
                { "Test state 2", SetSecondState }
            };
            var messagePublishers = new Dictionary<string, Func<object>>
            {
                {
                    "An entity was created", () => new MyMessage
                    {
                        Description = "Test"
                    }
                }
            };

            return new MessageInvoker(providerStates, messagePublishers);
        }

        private void SetFirstState()
        {
            _firstStateWasCalled = true;
        }

        private void SetSecondState()
        {
            _secondStateWasCalled = true;
        }

        [Fact]
        public void Invoke_ProviderStateDoesNotExist_ThrowsPactFailureException()
        {
            //Arrange
            var messageInvoker = GetSubject();
            var messageDescription = new MessagePactDescription
            {
                ProviderStates = new[]
                {
                    new ProviderState
                    {
                        Name = "Provider state that does not exist"
                    }
                },
                Description = "An entity was created"
            };

            //Act + Assert
            Assert.Throws<PactFailureException>(() => messageInvoker.Invoke(messageDescription));
        }

        [Fact]
        public void Invoke_PublisherDoesNotExist_ThrowsPactFailureException()
        {
            //Arrange
            var messageInvoker = GetSubject();
            var messageDescription = new MessagePactDescription
            {
                Description = "A description that does not exist"
            };

            //Act + Assert
            Assert.Throws<PactFailureException>(() => messageInvoker.Invoke(messageDescription));
        }


        [Fact]
        public void Invoke_ProviderStateWithNullName_InvokesPubliser()
        {
            //Arrange
            var messageInvoker = GetSubject();
            var messageDescription = new MessagePactDescription
            {
                ProviderStates = new[]
                {
                    new ProviderState
                    {
                        Name = null
                    }
                },
                Description = "An entity was created"
            };

            var expectedMessage = new MyMessage
            {
                Description = "Test"
            };

            //Act
            var actualMessage = messageInvoker.Invoke(messageDescription);

            //Assert
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void Invoke_WithoutProviderStates_InvokesPubliser()
        {
            //Arrange
            var messageInvoker = GetSubject();
            var messageDescription = new MessagePactDescription
            {
                Description = "An entity was created"
            };

            var expectedMessage = new MyMessage
            {
                Description = "Test"
            };

            //Act
            var actualMessage = messageInvoker.Invoke(messageDescription);

            //Assert
            Assert.Equal(expectedMessage, actualMessage);
        }

        [Fact]
        public void Invoke_MultipleProviderStates_InvokesPublisherAfterStatesWereSet()
        {
            //Arrange
            var messageInvoker = GetSubject();
            var messageDescription = new MessagePactDescription
            {
                ProviderStates = new[]
                {
                    new ProviderState
                    {
                        Name = "Test state 1"
                    },
                    new ProviderState
                    {
                        Name = "Test state 2"
                    }
                },
                Description = "An entity was created"
            };

            var expectedMessage = new MyMessage
            {
                Description = "Test"
            };

            //Act
            var actualMessage = messageInvoker.Invoke(messageDescription);

            //Assert
            Assert.True(_firstStateWasCalled);
            Assert.True(_secondStateWasCalled);
            Assert.Equal(expectedMessage, actualMessage);
        }

        private class MyMessage
        {
            public string Description { private get; set; }

            private bool Equals(MyMessage other)
            {
                return string.Equals(Description, other.Description);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MyMessage)obj);
            }

            public override int GetHashCode()
            {
                return (Description != null ? Description.GetHashCode() : 0);
            }
        }
    }
}
