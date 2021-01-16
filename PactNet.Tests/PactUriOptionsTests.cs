using System;
using Xunit;

namespace PactNet.Tests
{
    public class PactUriOptionsTests
    {
        [Fact]
        public void Ctor_WithEmptyUsername_ThrowsArgumentException()
        {
            const string username = "";
            const string password = "somepassword";
            
            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetBasicAuthentication(username, password));
        }

        [Fact]
        public void Ctor_WithInvalidUsername_ThrowsArgumentException()
        {
            const string username = "some:user";
            const string password = "somepassword";

            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetBasicAuthentication(username, password));
        }

        [Fact]
        public void Ctor_WithEmptyPassword_ThrowsArgumentException()
        {
            const string username = "someuser";
            const string password = "";

            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetBasicAuthentication(username, password));
        }

        [Fact]
        public void Ctor_WithValidUsernameAndPassword_ReturnsCorrectAuthorizationSchemeAndValue()
        {
            const string username = "Aladdin";
            const string password = "open sesame";
            const string expectedAuthScheme = "Basic";
            const string expectedAuthValue = "QWxhZGRpbjpvcGVuIHNlc2FtZQ==";

            var options = new PactUriOptions().SetBasicAuthentication(username, password);

            Assert.Equal(expectedAuthScheme, options.AuthorizationScheme);
            Assert.Equal(expectedAuthValue, options.AuthorizationValue);
        }

        [Fact]
        public void Ctor_WithValidUsernameAndPassword_ReturnsCorrectUsernameAndPassword()
        {
            const string username = "Aladdin";
            const string password = "open sesame";

            var options = new PactUriOptions().SetBasicAuthentication(username, password);

            Assert.Equal(username, options.Username);
            Assert.Equal(password, options.Password);
        }

        [Fact]
        public void Ctor_WithEmptyToken_ThrowsArgumentException()
        {
            const string token = "";

            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetBearerAuthentication(token));
        }

        [Fact]
        public void Ctor_WithValidToken_ReturnsCorrectAuthorizationSchemeAndValue()
        {
            const string token = "MyToken";
            const string expectedAuthScheme = "Bearer";
            const string expectedAuthValue = token;

            var options = new PactUriOptions().SetBearerAuthentication(token);

            Assert.Equal(expectedAuthScheme, options.AuthorizationScheme);
            Assert.Equal(expectedAuthValue, options.AuthorizationValue);
        }

        [Fact]
        public void Ctor_WithValidToken_ReturnsCorrectToken()
        {
            const string token = "MyToken";

            var options = new PactUriOptions().SetBearerAuthentication(token);

            Assert.Equal(token, options.Token);
        }
    }
}
