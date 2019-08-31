using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Consumer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Consumer
{
    public class EventsApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public EventsApiClient(string baseUri = null, string authToken = null)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUri ?? "http://my.api/v2/capture") };

            if (authToken != null)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
            }
        }

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public bool IsAlive()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/stats/status");
            request.Headers.Add("Accept", "application/json");

            var response = _httpClient.SendAsync(request);

            try
            {
                var result = response.Result;
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result, _jsonSettings);
                    return responseContent.alive;
                }

                RaiseResponseError(request, result);
            }
            finally
            {
                Dispose(request, response);
            }

            return false;
        }

        public DateTime? UpSince()
        {
            var statusRequest = new HttpRequestMessage(HttpMethod.Get, "/stats/status");
            statusRequest.Headers.Add("Accept", "application/json");

            var statusResponse = _httpClient.SendAsync(statusRequest);

            try
            {
                var statusResult = statusResponse.Result;
                var statusResponseBody = new StatusResponseBody();

                if (statusResult.StatusCode == HttpStatusCode.OK)
                {
                    statusResponseBody = JsonConvert.DeserializeObject<StatusResponseBody>(statusResult.Content.ReadAsStringAsync().Result, _jsonSettings);
                }
                else
                {
                    RaiseResponseError(statusRequest, statusResult);
                }

                if (statusResponseBody.Alive)
                {
                    var uptimeLink = statusResponseBody.Links.Single(x => x.Key.Equals("uptime")).Value.Href;

                    if (!String.IsNullOrEmpty(uptimeLink))
                    {
                        var uptimeRequest = new HttpRequestMessage(HttpMethod.Get, uptimeLink);
                        uptimeRequest.Headers.Add("Accept", "application/json");

                        var uptimeResponse = _httpClient.SendAsync(uptimeRequest);
                        try
                        {
                            var uptimeResult = uptimeResponse.Result;
                            if (uptimeResult.StatusCode == HttpStatusCode.OK)
                            {
                                var uptimeResponseBody = JsonConvert.DeserializeObject<UptimeResponseBody>(uptimeResult.Content.ReadAsStringAsync().Result, _jsonSettings);
                                return uptimeResponseBody.UpSince;
                            }

                            RaiseResponseError(uptimeRequest, uptimeResult);
                        }
                        finally
                        {
                            Dispose(uptimeRequest, uptimeResponse);
                        }
                    }
                }
            }
            finally
            {
                Dispose(statusRequest, statusResponse);
            }

            return null;
        }

        public IEnumerable<Event> GetAllEvents()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/events");
            request.Headers.Add("Accept", "application/json");

            var response = _httpClient.SendAsync(request);

            try
            {
                var result = response.Result;
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var content = result.Content.ReadAsStringAsync().Result;
                    return !String.IsNullOrEmpty(content)
                                ? JsonConvert.DeserializeObject<IEnumerable<Event>>(content, _jsonSettings)
                                : new List<Event>();
                }

                RaiseResponseError(request, result);
            }
            finally
            {
                Dispose(request, response);
            }

            return null;
        }

        public Event GetEventById(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, String.Format("/events/{0}", id));
            request.Headers.Add("Accept", "application/json");

            var response = _httpClient.SendAsync(request);

            try
            {
                var result = response.Result;
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<Event>(result.Content.ReadAsStringAsync().Result, _jsonSettings);
                }

                RaiseResponseError(request, result);
            }
            finally
            {
                Dispose(request, response);
            }

            return null;
        }

        public IEnumerable<Event> GetEventsByType(string eventType)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, String.Format("/events?type={0}", eventType));
            request.Headers.Add("Accept", "application/json");

            var response = _httpClient.SendAsync(request);

            try
            {
                var result = response.Result;
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<Event>>(result.Content.ReadAsStringAsync().Result, _jsonSettings);
                }

                RaiseResponseError(request, result);
            }
            finally
            {
                Dispose(request, response);
            }

            return null;
        }

        public void CreateEvent(Guid eventId, string eventType = "DetailsView")
        {
            var @event = new
            {
                EventId = eventId,
                Timestamp = DateTimeFactory.Now().ToString("O"),
                EventType = eventType
            };

            var eventJson = JsonConvert.SerializeObject(@event, _jsonSettings);
            var requestContent = new StringContent(eventJson, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "/events") { Content = requestContent };

            var response = _httpClient.SendAsync(request);

            try
            {
                var result = response.Result;
                var statusCode = result.StatusCode;
                if (statusCode == HttpStatusCode.Created)
                {
                    return;
                }

                RaiseResponseError(request, result);
            }
            finally
            {
                Dispose(request, response);
            }
        }

        private static void RaiseResponseError(HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            throw new HttpRequestException(
                String.Format("The Events API request for {0} {1} failed. Response Status: {2}, Response Body: {3}",
                failedRequest.Method.ToString().ToUpperInvariant(),
                failedRequest.RequestUri,
                (int)failedResponse.StatusCode,
                failedResponse.Content.ReadAsStringAsync().Result));
        }

        public void Dispose()
        {
            Dispose(_httpClient);
        }

        public void Dispose(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables.Where(d => d != null))
            {
                disposable.Dispose();
            }
        }
    }
}
