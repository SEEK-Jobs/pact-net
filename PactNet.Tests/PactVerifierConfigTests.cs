using System.Collections.Generic;
using System;
using Xunit;

namespace PactNet.Tests
{
    public class PactVerifierConfigTests
    {
        private readonly PactVerifierConfig _verifierConfig;

        public PactVerifierConfigTests()
        {
             _verifierConfig = new PactVerifierConfig();
        }

        private KeyValuePair<string,string> GetDummyHeader()
        {
            return new KeyValuePair<string, string>("dummy_key", "dummy_value");
        }

        [Fact]
        [Obsolete]
        public void PactVertifier_Init_State_CustomHeaderIsNull()
        {
            Assert.Null( _verifierConfig.CustomHeader );
        }

        [Fact]
        public void PactVertifier_Init_State_OutputersIsNotNull()
        {
            Assert.NotNull( _verifierConfig.Outputters );
        }

        [Fact]
        public void PactVertifier_Init_State_CustomHeadersCountIsZero()
        {
            Assert.Equal(0, _verifierConfig.CustomHeaders.Count);
        }

        [Fact]
        [Obsolete]
        public void WhenCustomHeaderIsNotNull_ShouldBeAvailableThroughCustomHeadersCollection()
        {
            _verifierConfig.CustomHeader = GetDummyHeader();

            Assert.True(_verifierConfig.CustomHeaders.ContainsKey(_verifierConfig.CustomHeader.Value.Key));
        }

        [Fact]
        [Obsolete]
        public void WhenCustomHeaderHasPreviousValue_AndCustomHeaderChangedToNull_ShouldPreviousValueIsNotAvailableInCustomHeaders()
        {
            _verifierConfig.CustomHeader = GetDummyHeader();

            _verifierConfig.CustomHeader = null;

            Assert.False( _verifierConfig.CustomHeaders.ContainsKey( GetDummyHeader().Key) );
        }

        [Fact]
        [Obsolete]
        public void WhenCustomHeaderHasPreviousValue_AndChangingCustomHeader_ShouldPreviousValueNotBeAvailableThroughCustomHeadersCollectionAnymore()
        {
            _verifierConfig.CustomHeader = GetDummyHeader();

            _verifierConfig.CustomHeader = new KeyValuePair<string, string>("new_dummy_key", "new_dummy_value");

            Assert.False( _verifierConfig.CustomHeaders.ContainsKey(GetDummyHeader().Key) );
        }
    }
}
