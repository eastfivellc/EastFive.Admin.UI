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
        public const string authorizationTokenStorageKey = "authorization_token";

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
                    var token = await storage.GetItemAsync<string>(Comms.authorizationTokenStorageKey);
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
                    var token = await storage.GetItemAsync<string>(Comms.authorizationTokenStorageKey);
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
