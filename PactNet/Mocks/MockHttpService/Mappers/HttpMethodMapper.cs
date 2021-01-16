using System;
using System.Collections.Generic;
using System.Net.Http;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Mappers
{
    internal class HttpMethodMapper : IHttpMethodMapper
    {
        private static readonly IDictionary<HttpVerb, HttpMethod> Map = new Dictionary<HttpVerb, HttpMethod>
        {
            { HttpVerb.Get, HttpMethod.Get },
            { HttpVerb.Post, HttpMethod.Post },
            { HttpVerb.Put, HttpMethod.Put },
            { HttpVerb.Delete, HttpMethod.Delete },
            { HttpVerb.Head, HttpMethod.Head },
            { HttpVerb.Patch, new HttpMethod("PATCH") },
            { HttpVerb.Options, HttpMethod.Options }
        };

        public HttpMethod Convert(HttpVerb from)
        {
            if (!Map.ContainsKey(from))
            {
                throw new ArgumentException(string.Format("Cannot map HttpVerb.{0} to a HttpMethod, no matching item has been registered.", from));
            }

            return Map[from];
        }
    }
}
