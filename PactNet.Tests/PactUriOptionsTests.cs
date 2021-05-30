using System;
using Xunit;

namespace PactNet.Tests
{
    public class PactUriOptionsTests
    {

        const string UsernameIsEmptyOrNullMessage = "username is null or empty.";
        const string PasswordIsNullOrEmptyMessage = "password is null or empty.";
        const string UsernameContainsColunMessage = "username contains a ':' character, which is not allowed.";

        [Theory]
        [InlineData( "bad:name", UsernameContainsColunMessage  ) ] // RFC 2617 compliance
        [InlineData( "", UsernameIsEmptyOrNullMessage  ) ] 
        [InlineData( null, UsernameIsEmptyOrNullMessage  ) ]
        public void Ctor_WhenUserNameIsNotAccpetable(string username, string expectedMessage)
        {
            Exception e = Assert.Throws <ArgumentException> (() => new PactUriOptions(username, "dummyval"));
            Assert.Equal(expectedMessage , e.Message);
        }

        [Theory]
        [InlineData( null, PasswordIsNullOrEmptyMessage ) ] // RFC 2617 compliance
        [InlineData( "", PasswordIsNullOrEmptyMessage ) ] // RFC 2617 compliance
        public void Ctor_WhenPasswordIsNotAccpetable(string password, string expectedMessage)
        {
            Exception e = Assert.Throws <ArgumentException> (() => new PactUriOptions("dummyval", password));
            Assert.Equal(expectedMessage , e.Message);
        }

        [Theory]
        [InlineData("some@user", "password")] // RFC 2716
        [InlineData("someuser" , "password")]
        [InlineData("someuser" , "pass word")] // RFC 2716
        public void Ctor_AllowUserNamesAndPasswords(string username, string password )
        {
            var options = new PactUriOptions(username, password);
            Assert.Equal(options.Username, username);
        }

        [Fact]
        public void Ctor_With_ValidUserNamePassword_ValidateAuthorisationSchemaAndvalue()
        {
            const string username = "Aladdin";
            const string password = "open sesame";
            const string expectedAuthScheme = "Basic";
            const string expectedAuthValue = "QWxhZGRpbjpvcGVuIHNlc2FtZQ==";

            var options = new PactUriOptions(username, password);

            Assert.Equal(expectedAuthScheme, options.AuthorizationScheme);
            Assert.Equal(expectedAuthValue, options.AuthorizationValue);
        }

        const string expectedBearAndBasicAuthMessage = "You can't set both bearer and basic authentication at the same time";

        [Fact]
        public void Ctor_GivenTokenIsSet_WhenUserNameIsSet_ThenThrowsInvalidOperationException()
        {
            const string username = "Aladdin";
            const string password = "open sesame";
            const string token = "sometokenvalue";

            var options = new PactUriOptions(token);

            Exception e = Assert.Throws<InvalidOperationException>( () => options.SetBasicAuthentication(username, password) );
            Assert.Equal(expectedBearAndBasicAuthMessage, e.Message);
        }

        [Fact]
        public void Ctor_GivenUserNamePasswordIsSet_WhenBearerTokenIsSet_ThenThrowsInvalidOperationException()
        {
            const string username = "Aladdin";
            const string password = "password";
            const string token = "token";

            var options = new PactUriOptions(username, password);

            Exception e = Assert.Throws< InvalidOperationException>( () => options.SetBearerAuthentication(token) );
            Assert.Equal(expectedBearAndBasicAuthMessage, e.Message);
        }

        [Fact]
        public void Ctor_GivenUserNamePasswordAreSet_WhenBearerTokenIsSet_ThenThrowsInvalidOperationException()
        {
            const string username = "Aladdin";
            const string password = "open sesame";
            const string token = "sometokenvalue";;

            var options = new PactUriOptions(username, password);

            Exception e = Assert.Throws<InvalidOperationException>(() => options.SetBearerAuthentication(token));
            Assert.Equal(expectedBearAndBasicAuthMessage, e.Message);
        }

        [Fact]
        public void Ctor_WithValidUsernameAndPassword_ReturnsCorrectAuthorizationSchemaAndValue()
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

            var options = new PactUriOptions().SetBearerAuthentication(token);

            Assert.Equal(expectedAuthScheme, options.AuthorizationScheme);
            Assert.Equal(token, options.AuthorizationValue);
        }

        [Fact]
        public void Ctor_WhenSetWithValidToken_ShouldTokenBeSameValue()
        {
            const string token = "MyToken";

            var options = new PactUriOptions().SetBearerAuthentication(token);

            Assert.Equal(token, options.Token);
        }

        [Fact]
        public void Ctor_SetSslCaPathEmpty_ThrowsArgumentException()
        {
            const string sslpath = "";

            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetSslCaFilePath(sslpath));
        }

        [Fact]
        public void Ctor_SetSslCaPathNull_ThrowsArgumentException()
        {
            const string sslpath = null;

            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetSslCaFilePath(sslpath));
        }

        [Fact]
        public void CTor_SetHttpProxyEmpty_ThrowsArgumentException()
        {
            const string proxy = "";
            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetHttpProxy(proxy));
        }

        [Fact]
        public void CTor_SetHttpProxyNull_ThrowsArgumentException()
        {
            const string proxy = null;
            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetHttpProxy(proxy));
        }

        [Fact]
        public void CTor_SetHttpsProxiesEmpty_ThrowsArgumentException()
        {
            const string httpProxy = "somevalue";
            const string httpsProxy = "";
            Assert.Throws<ArgumentException>( () => new PactUriOptions().SetHttpProxy( httpProxy, httpsProxy ) );
        }

        [Fact]
        public void CTor_SetHttpsProxiesNull_ThrowsArgumentException()
        {
            const string httpProxy = "somevalue";
            const string httpsProxy = null;
            Assert.Throws<ArgumentException>(() => new PactUriOptions().SetHttpProxy( httpProxy, httpsProxy ) );
        }
    }
}
