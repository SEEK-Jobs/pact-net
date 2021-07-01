#if NETSTANDARD2_0_OR_GREATER

using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

namespace PactNet.Native.Messaging
{
    /// <summary>
    /// Defines the message middleware
    /// It handles the queries for the verifier when there are message interactions
    /// </summary>
    public class MessageMiddleware
    {

        private readonly RequestDelegate _next;

        /// <summary>
        /// Creates an instance of <see cref="MessageMiddleware"/>
        /// </summary>
        /// <param name="next"></param>
        public MessageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// The invoke method of the middleware
        /// </summary>
        /// <param name="context">The http context</param>
        /// <returns>A task</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!(context.Request.Path.Value?.StartsWith(Constants.PactMessagesPath) ?? false))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;

            string jsonRequestBody = await GetRequestBodyAsync(context);

            var interactionDescription = JsonConvert.DeserializeObject<MessageInteraction>(jsonRequestBody)?.Description;

            if (string.IsNullOrWhiteSpace(interactionDescription))
            {
                //See if raising an exception is better
                await context.Response.WriteAsync(string.Empty);
            }

            var response = Scenarios.InvokeScenario(interactionDescription);

            await WriteToResponseAsync(context, response);
        }

        /// <summary>
        /// Read the request body as a string
        /// </summary>
        /// <param name="context">The http context</param>
        /// <returns>The request body</returns>
        protected internal virtual async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);

            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Write to response
        /// </summary>
        /// <param name="context">The http context</param>
        /// <param name="response">The object response</param>
        /// <returns>A task</returns>
        protected internal virtual async Task WriteToResponseAsync(HttpContext context, dynamic response)
        {
            await context.Response.WriteAsync((string)JsonConvert.SerializeObject(response));
        }

        private class MessageInteraction
        {
            public string Description { get; set; }
        }
    }
}
#endif
