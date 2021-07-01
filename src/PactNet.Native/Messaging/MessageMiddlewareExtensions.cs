#if NETSTANDARD2_0_OR_GREATER
using Microsoft.AspNetCore.Builder;

namespace PactNet.Native.Messaging
{
    /// <summary>
    /// Defines the message middleware extensions
    /// </summary>
    public static class MessageMiddlewareExtensions
    {
        /// <summary>
        /// Extensions method to add the message middleware
        /// </summary>
        /// <param name="builder">the builder</param>
        /// <returns>Fluent builder</returns>
        public static IApplicationBuilder UseMessaging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MessageMiddleware>();
        }
    }
}
#endif
