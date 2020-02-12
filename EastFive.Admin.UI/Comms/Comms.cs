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
            return await GetManifestAsync(new Uri(serverUrl), onFound, onFailure);
        }

        public static async Task<TResult> GetManifestAsync<TResult>(
                Uri serverUrl,
            Func<Route[], TResult> onFound,
            Func<string, TResult> onFailure)
        {
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
                                var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<Route[]>(json,
                                    new Serialization.Json.Converter());
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

        public static async Task<TResult> GetAsync<TResource, TResult>(
                string routeName, string methodName,
                IDictionary<string, string> parameters,
                Server server,
            Func<TResource, TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            return await await ActionAsync(routeName, methodName,
                    parameters,
                    HttpMethod.Get,
                    server,
                    (httpRequest) => httpRequest,
                async (response) =>
                {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var failureMsg = $"Response Code:{response.StatusCode}";
                        Console.WriteLine(failureMsg);
                        return onFailure(failureMsg);
                    }
                    
                    try
                    {
                        var responseResource = Newtonsoft.Json.JsonConvert.DeserializeObject<TResource>(json,
                            new Serialization.Json.Converter());
                        return onSuccess(responseResource);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return onFailure(ex.Message);
                    }
                },
                onNoMatchingEndpoint:onNoMatchingEndpoint.AsAsyncFunc(),
                onNoMatchingMethod:onNoMatchingMethod.AsAsyncFunc(),
                onFailure.AsAsyncFunc());
        }

        public static async Task<TResult> GetAsync<TResource, TResult>(
                string routeName, string methodName,
                IDictionary<string, string> parameters,
                Session session,
            Func<TResource, TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            var server = Server.Servers.Where(server => server.id == session.serverId).First();
            return await await ActionAsync(routeName, methodName,
                    parameters,
                    HttpMethod.Get,
                    server,
                    (httpRequest) =>
                    {
                        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "bearer", session.token);
                        return httpRequest;
                    },
                async (response) =>
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var failureMsg = $"Response Code:{response.StatusCode}";
                        Console.WriteLine(failureMsg);
                        return onFailure(failureMsg);
                    }

                    try
                    {
                        var responseResource = Newtonsoft.Json.JsonConvert.DeserializeObject<TResource>(json,
                            new Serialization.Json.Converter());
                        return onSuccess(responseResource);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return onFailure(ex.Message);
                    }
                },
                onNoMatchingEndpoint: onNoMatchingEndpoint.AsAsyncFunc(),
                onNoMatchingMethod: onNoMatchingMethod.AsAsyncFunc(),
                onFailure.AsAsyncFunc());
        }

        public static async Task<TResult> GetRawJTokenAsync<TResult>(
                string routeName, string methodName,
                IDictionary<string, string> parameters,
                Session session,
            Func<JToken, TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            var server = Server.Servers.Where(server => server.id == session.serverId).First();
            return await await ActionAsync(routeName, methodName,
                    parameters,
                    HttpMethod.Get,
                    server,
                    (httpRequest) =>
                    {
                        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "bearer", session.token);
                        return httpRequest;
                    },
                async (response) =>
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var failureMsg = $"Response Code:{response.StatusCode}";
                        Console.WriteLine(failureMsg);
                        return onFailure(failureMsg);
                    }

                    try
                    {
                        var responseResource = JToken.Parse(json);
                        //Newtonsoft.Json.JsonConvert.DeserializeObject(json,
                        //    new Newtonsoft.Json.JsonSerializerSettings
                        //    {
                        //        Converters = new Serialization.Json.Converter().AsArray()
                        //    });
                        return onSuccess(responseResource);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        return onFailure(ex.Message);
                    }
                },
                onNoMatchingEndpoint: onNoMatchingEndpoint.AsAsyncFunc(),
                onNoMatchingMethod: onNoMatchingMethod.AsAsyncFunc(),
                onFailure.AsAsyncFunc());
        }

        public static Task<TResult> PostAsync<TResource, TResult>(
                string routeName, string methodName,
                TResource resource,
                Server server,
            Func<TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            return ActionAsync(routeName, methodName,
                    new Dictionary<string, string>(),
                    HttpMethod.Post,
                    server,
                    (httpRequest) =>
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource, 
                            new Serialization.Json.Converter());
                        httpRequest.Content = new StringContent(json,
                            System.Text.Encoding.UTF8, "application/json");
                        return httpRequest;
                    },
                onSuccess:
                    (response) => onSuccess(),
                onNoMatchingEndpoint: onNoMatchingEndpoint,
                onNoMatchingMethod: onNoMatchingMethod,
                onFailure: onFailure);
        }

        public static async Task<TResult> PostAsync<TResource, TResponse, TResult>(
                string routeName, string methodName,
                TResource resource,
                Server server,
            Func<TResponse, TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            return await await ActionAsync(routeName, methodName,
                    new Dictionary<string, string>(),
                    HttpMethod.Post,
                    server,
                    (httpRequest) =>
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(resource,
                            new Serialization.Json.Converter());
                        httpRequest.Content = new StringContent(json,
                            System.Text.Encoding.UTF8, "application/json");
                        return httpRequest;
                    },
                onSuccess:
                    async (response) =>
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            var failureMsg = $"Response Code:{response.StatusCode}";
                            Console.WriteLine(failureMsg);
                            return onFailure(failureMsg);
                        }

                        try
                        {
                            var responseResource = Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(json,
                                new Serialization.Json.Converter());
                            return onSuccess(responseResource);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            return onFailure(ex.Message);
                        }
                    },
                onNoMatchingEndpoint: onNoMatchingEndpoint.AsAsyncFunc(),
                onNoMatchingMethod: onNoMatchingMethod.AsAsyncFunc(),
                onFailure: onFailure.AsAsyncFunc());
        }

        public static async Task<TResult> OptionsAsync<TResource, TResult>(
                string routeName, string methodName,
                IDictionary<string, string> parameters,
                Server server,
            Func<TResource, TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            return await await ActionAsync(routeName, methodName,
                    parameters,
                    HttpMethod.Options,
                    server,
                    (httpRequest) => httpRequest,
                async (response) =>
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var failureMsg = $"Response Code:{response.StatusCode}";
                        Console.WriteLine(failureMsg);
                        return onFailure(failureMsg);
                    }

                    try
                    {
                        var responseResource = Newtonsoft.Json.JsonConvert.DeserializeObject<TResource>(json,
                            new Serialization.Json.Converter());
                        return onSuccess(responseResource);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return onFailure(ex.Message);
                    }
                },
                onNoMatchingEndpoint: onNoMatchingEndpoint.AsAsyncFunc(),
                onNoMatchingMethod: onNoMatchingMethod.AsAsyncFunc(),
                onFailure.AsAsyncFunc());
        }

        public static Task<TResult> ActionAsync<TResult>(
                string routeName, string methodName,
                IDictionary<string, string> parameters,
                HttpMethod httpMethod, Server server,
                Func<HttpRequestMessage, HttpRequestMessage> mutateRequest,
            Func<HttpResponseMessage, TResult> onSuccess,
            Func<TResult> onNoMatchingEndpoint = default,
            Func<TResult> onNoMatchingMethod = default,
            Func<string, TResult> onFailure = default)
        {
            return server.endpoints
                .NullToEmpty()
                .Where(ep => ep.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase))
                .First(
                    (route, next) =>
                    {
                        return route.Methods
                           .Where(method => method.Name == methodName)
                           .First(
                                (matchingMethod, next) =>
                                {
                                    var httpMethod = new HttpMethod(matchingMethod.HttpMethod);
                                    return Action(
                                            server.serverLocation.AbsoluteUri, httpMethod,
                                            matchingMethod,
                                            parameters,
                                            mutateRequest,
                                        onSuccess,
                                        onFailure);
                                },
                                () => onNoMatchingMethod().AsTask());
                    },
                    () => onNoMatchingEndpoint().AsTask());
        }

        private static async Task<TResult> Action<TResult>(
                string serverUrlStr, HttpMethod httpMethod,
                Method matchingMethod,
                IDictionary<string, string> parameters,
                Func<HttpRequestMessage, HttpRequestMessage> mutateRequest,
            Func<HttpResponseMessage, TResult> onSuccess,
            Func<string, TResult> onFailure = default)
        {
            using (var client = new HttpClient())
            {
                var serverUrl = new Uri(serverUrlStr);
                if (matchingMethod.Path.IsDefaultOrNull())
                {
                    var msg = $"No path specified for {matchingMethod.Name}";
                    if (onFailure.IsDefaultOrNull())
                        throw new Exception(msg);
                    return onFailure(msg);
                }
                var methodUrl = new Uri(serverUrl, matchingMethod.Path);
                var url = matchingMethod.Parameters
                    .Where(p => p.Where.Contains("QUERY", StringComparison.InvariantCultureIgnoreCase))
                    .Aggregate(methodUrl,
                        (current, parameter) =>
                        {
                            Console.WriteLine($"Looking for {parameter.Name} in {parameters.Count} params");
                            
                            if (!parameters.ContainsKey(parameter.Name))
                            {
                                Console.WriteLine($"PARAMETER `{parameter.Name}` was not provided");
                                return current;
                            }
                            var paramValue = parameters[parameter.Name];
                            var uriBuilder = new UriBuilder(current);
                            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                            query[parameter.Name] = paramValue;
                            uriBuilder.Query = query.ToString();
                            return uriBuilder.Uri;
                        });

                Console.WriteLine(url.AbsoluteUri);
                using (var request = mutateRequest(new HttpRequestMessage(httpMethod, url)))
                {
                    try
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            return onSuccess(response);
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
