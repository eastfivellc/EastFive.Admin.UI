using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using EastFive.Admin.UI.Resources;
using EastFive.Api.Resources;
using EastFive.Extensions;
using EastFive.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json.Linq;

namespace EastFive.Admin.UI
{
    public static class Comms
    {
        //public static string token = "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNyc2Etc2hhMjU2IiwidHlwIjoiSldUIn0.eyJzZXNzaW9uIjoiNmE4M2U5ODAtZjhhYi00Yzk0LWIzNjUtNTYwMTM1YzQ2ZGQ4IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXM_aWQ9NDVjZDAzZGQtZjE4OC00NTg4LWI5YzYtYzcxOTNlMjIwZjZiIjoiYWFkNGQwMmUtZDkxMS00YTA0LWEyM2UtNmI0YmM3ODZmMTczIiwibmJmIjoxNTcwNTM5MDY3LCJleHAiOjE1Nzg0ODc4NjcsImlzcyI6Imh0dHBzOi8vYWZmaXJtaGVhbHRocGRtcy1hdXRoZW50aWNhdGlvbi1wcmQuYXp1cmV3ZWJzaXRlcy5uZXQvIiwiYXVkIjoiaHR0cHM6Ly9hZmZpcm1oZWFsdGhwZG1zZGVtbzIuYXp1cmV3ZWJzaXRlcy5uZXQvIn0.B3RP8JC28UeDr931FwI09Ge1x2z8CcO7Im6Cn0nBS-wBi2ZlJuI10KyV9O8CVN8lhsxkRRXnorg6iDcqq320lJU4iufELXGUIjA1DVK3pR04JannW5NLwfeURLzvX7o0FLIaqvTwGlCzEVD9OxAHFnKUWE8I2Ohs-f1DBs8WXuSRIEJ1Wctni6GQPX_NIx_dryKIroYCOFW9mqheqst-R_zXvXchwlYgvEVWKz7L-r2p2jgok9i0GQ36frwDJ0mS4IvnYhMTlAc6q4WOznLCAvx047kPlk30o_SSO8B0s8hjYx1Q26TfaPkAOG1wiihDMX5hNJwKs55b7s8teOgkBg";

        //public const string ServerUrl = "http://localhost:57601";

        public static async Task<TResult> GetManifestAsync<TResult>(
                Blazored.LocalStorage.ILocalStorageService storage,
            Func<Route[], TResult> onFound,
            Func<string, TResult> onFailure)
        {
            var serverUrl = await storage.GetItemAsync<string>("serverUrl");
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{serverUrl}/api/ManifestRoute"))
                {
                    //request.Headers.Authorization =
                    //    new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    request.Headers.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                            "application/manifest+json"));
                    try
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            try
                            {
                                var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<Route[]>(json);
                                return onFound(notifications);
                            }
                            catch (Exception ex)
                            {
                                return onFailure($"{ex.Message}:{json}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return onFailure(ex.Message);
                    }
                }
            }
        }

        public static async Task<TResult> GetAsync<TResult>(Route route, Method matchingMethod,
                IDictionary<string, string> parameters,
                Blazored.LocalStorage.ILocalStorageService storage,
            Func<EntityType[], TResult> onFound,
            Func<string, TResult> onFailure)
        {
            var serverUrl = await storage.GetItemAsync<string>("serverUrl");
            using (var client = new HttpClient())
            {
                var url = matchingMethod.Parameters.Aggregate(
                    new Uri($"{serverUrl}/api/{route.Name}"),
                    (current, parameter) =>
                    {
                        var paramValue = parameters.ContainsKey(parameter.Name) ?
                            parameters[parameter.Name]
                            :
                            string.Empty;
                        var uriBuilder = new UriBuilder(current);
                        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                        query[parameter.Name] = paramValue;
                        uriBuilder.Query = query.ToString();
                        return uriBuilder.Uri;
                        //return current.AddQueryParameter(parameter.Name, paramValue);
                    });
                Console.WriteLine(url.AbsoluteUri);
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var token = await storage.GetItemAsync<string>("authorization_token");
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    try
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            var json = await response.Content.ReadAsStringAsync();

                            Console.WriteLine(json);
                            if (!response.IsSuccessStatusCode)
                                return onFailure(json);
                            try
                            {
                                var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                                int Ordering(JProperty prop)
                                {
                                    var orderIndex = route.Properties
                                        .Select(
                                            (rProp, index) =>
                                            {
                                                if (rProp.Name == prop.Name)
                                                    return index;
                                                return -1;
                                            })
                                        .Where(i => i >= 0)
                                        .First(
                                            (i, next) => i,
                                            () => int.MaxValue);
                                    return orderIndex;
                                }

                                if (responseData is JArray)
                                {
                                    var entities = (JArray)responseData;
                                    var entityTypes = entities
                                        .Select(
                                            entity =>
                                            {
                                                var jObj = entity as JObject;
                                                var props = jObj
                                                    .Properties()
                                                    .OrderBy(Ordering)
                                                    .Select((prop, index) => (IDataType)new JTokenDataType(prop, index))
                                                    .ToArray();
                                                return new EntityType()
                                                {
                                                    Properties = props,
                                                };
                                            })
                                        .ToArray();
                                    return onFound(entityTypes);
                                }
                                if(responseData is JObject)
                                {
                                    var jObj = responseData as JObject;
                                    var props = jObj
                                        .Properties()
                                        .OrderBy(Ordering)
                                        .Select((prop, index) => (IDataType)new JTokenDataType(prop, index))
                                        .ToArray();
                                    var entity = new EntityType()
                                    {
                                        Properties = props,
                                    };
                                    return onFound(entity.AsArray());
                                }
                                return onFailure(json);
                            }
                            catch (Exception)
                            {
                                return onFailure(json);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return onFailure(ex.Message);
                    }
                }
            }
        }

        public static async Task<bool> PostAsync(Route route, Method matchingMethod,
                string content,
                Blazored.LocalStorage.ILocalStorageService storage)
        {
            var serverUrl = await storage.GetItemAsync<string>("serverUrl");
            using (var client = new HttpClient())
            {
                var url = new Uri($"{serverUrl}/api/{route.Name}");
                Console.WriteLine($"POSTING to {url.AbsoluteUri}");
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    var token = await storage.GetItemAsync<string>("authorization_token");
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    using (request.Content = new StringContent(content))
                    {
                        request.Content.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        try
                        {
                            using (var response = await client.SendAsync(request))
                            {
                                return response.IsSuccessStatusCode;
                            }
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }
        }
    }
}
