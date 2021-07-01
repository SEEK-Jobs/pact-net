//#if NET5_0_OR_GREATER

//using System.Threading.Tasks;

//using Microsoft.AspNetCore.Http;

//using PactNet.Native.Messaging;

//using Xunit;

//namespace PactNet.Native.Tests.Messaging
//{
//    public class MessageMiddlewareTests
//    {
//        private RequestDelegate _nextRequestDelegate;
//        private FakeMessageMiddleware _middlewareUnderTest;

//        private bool _nextRequestProcessed;

//        public MessageMiddlewareTests()
//        {
//            _nextRequestDelegate = context =>
//            {
//                _nextRequestProcessed = true;
//                return Task.FromResult(true);
//            };

//            _middlewareUnderTest = new FakeMessageMiddleware(_nextRequestDelegate);
//        }

//        private void SetupNextDelegate()
//        {
//            _nextRequestDelegate = context =>
//            {
//                _nextRequestProcessed = true;
//                return Task.FromResult(true);
//            };

//            _middlewareUnderTest = new FakeMessageMiddleware(_nextRequestDelegate);
//        }

//        [Fact]
//        public async Task Middleware_Should_Intercept_NonMessageRequest_And_Do_Nothing()
//        {
//            var httpContext = new DefaultHttpContext();

//            await _middlewareUnderTest.InvokeAsync(httpContext);

//            Assert.True(_nextRequestProcessed);
//        }

//        [Fact]
//        public async Task Middleware_Should_Intercept_MessageRequest_And_Return_Response_If_Scenario_Set()
//        {
//            var httpContext = new DefaultHttpContext
//            {
//                Request =
//                {
//                    Path = "/pact-messages"
//                }
//            };

//            var interactionDescription = "a non existing description";
//            var expectedResponse = new { result = "mymessage" };

//            MessageScenarioBuilder.Instance
//                .WhenReceiving(interactionDescription)
//                .WillPublishMessage(() => expectedResponse);

//            _middlewareUnderTest.Description = $"{{\"description\": \"{interactionDescription}\"}}";

//            await _middlewareUnderTest.InvokeAsync(httpContext);

//            Assert.Equal(expectedResponse, _middlewareUnderTest.Response);
//            Assert.False(_nextRequestProcessed);
//        }

//        private class FakeMessageMiddleware : MessageMiddleware
//        {
//            public string Description;
//            public dynamic Response;

//            protected internal override async Task<string> GetRequestBodyAsync(HttpContext context)
//            {
//                return Description;
//            }

//            protected internal override async Task WriteToResponseAsync(HttpContext context, dynamic response)
//            {
//                Response = response;
//            }

//            public FakeMessageMiddleware(RequestDelegate next) : base(next)
//            {
//            }
//        }
//    }
//}
//#endif
