using System;
using System.Text;

namespace PactNet
{
    public class PactUriOptions
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Token { get; private set;  }
        public string AuthorizationScheme { get; private set; }
        public string AuthorizationValue { get; private set; }
        public string SslCaFilePath { get; private set; }
        public string HttpProxy { get; private set; }
        public string HttpsProxy { get; private set; }

        public PactUriOptions() { }
        
        public PactUriOptions(string username, string password)
        {
            SetBasicAuthenticationInternal(username, password);
        }

        public PactUriOptions(string token)
        {
            SetBearerAuthentication(token);
        }

        public PactUriOptions SetBasicAuthentication(string username, string password)
        {
            if (!string.IsNullOrEmpty(Token))
            {
                throw new InvalidOperationException("You can't set both bearer and basic authentication at the same time");
            }
            SetBasicAuthenticationInternal(username, password);
            return this;
        }

        public PactUriOptions SetBearerAuthentication(string token)
        {
            if (!string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password))
            {
                throw new InvalidOperationException("You can't set both bearer and basic authentication at the same time");
            }
            SetBearerAuthenticationInternal(token);
            return this;
        }


        public PactUriOptions SetSslCaFilePath(string pathToSslCaFile)
        {
            if (string.IsNullOrEmpty(pathToSslCaFile))
            {
                throw new ArgumentException($"{nameof(pathToSslCaFile)} is null or empty");
            }
            SslCaFilePath = pathToSslCaFile;
            return this;
        }

        public PactUriOptions SetHttpProxy(string httpProxy)
        {
            if (string.IsNullOrEmpty(httpProxy))
            {
                throw new ArgumentException($"{nameof(httpProxy)} is null or empty");
            }
            HttpProxy = httpProxy;
            HttpsProxy = httpProxy;
            return this;
        }

        public PactUriOptions SetHttpProxy(string httpProxy, string httpsProxy)
        {
            SetHttpProxy(httpProxy);
            if (string.IsNullOrEmpty(httpsProxy))
            {
                throw new ArgumentException($"{nameof(httpsProxy)} is null or empty");
            }
            HttpsProxy = httpsProxy;
            return this;
        }

        private void SetBasicAuthenticationInternal(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException($"{nameof(username)} is null or empty.");
            }

            if (username.Contains(":"))
            {
                throw new ArgumentException($"{nameof(username)} contains a ':' character, which is not allowed.");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException($"{nameof(password)} is null or empty.");
            }

            Username = username;
            Password = password;
            AuthorizationScheme = "Basic";
            AuthorizationValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"));
        }

        private void SetBearerAuthenticationInternal(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException($"{nameof(token)} is null or empty.");
            }

            Token = token;
            AuthorizationScheme = "Bearer";
            AuthorizationValue = token;
        }
    }
}
