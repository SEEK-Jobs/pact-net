using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Provider.Tests
{
    public class AuthorizationTokenReplacementMiddleware
    {
        private const string Authorization = "Authorization";

        private readonly RequestDelegate next;

        public AuthorizationTokenReplacementMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey(Authorization))
            {
                // swap for a valid key
                string token = TokenGenerator.Generate();
                context.Request.Headers[Authorization] = $"Bearer {token}";
            }

            await this.next(context);
        }
    }
}
