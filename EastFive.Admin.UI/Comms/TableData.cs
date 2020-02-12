using EastFive.Api.Resources;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public static class TableData
    {
        public const string storageTableInformationTokenName = "storageTableInformationToken";

        public static Task<TResult> TableAsync<TResult>(Route route,
                Blazored.LocalStorage.ILocalStorageService storage,
            Func<Newtonsoft.Json.Linq.JObject[], TResult> onFound,
            Func<string, TResult> onFailure)
        {
            return TableAsync(route.Name, storage, onFound, onFailure);
        }

        public static async Task<TResult> TableAsync<TResult>(string routeName,
                Blazored.LocalStorage.ILocalStorageService storage,
            Func<Newtonsoft.Json.Linq.JObject[], TResult> onFound,
            Func<string, TResult> onFailure)
        {
            using (var client = new HttpClient())
            {
                var serverUrl = await storage.GetItemAsync<string>("serverUrl");
                var url = new Uri($"{serverUrl}/api/{routeName}");
                Console.WriteLine(url.AbsoluteUri);
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    var tableInfoToken = await storage.GetItemAsync<string>(storageTableInformationTokenName);
                    request.Headers.Add("StorageTableInformation", tableInfoToken);
                    request.Headers.Add("X-StorageTableInformation-List", "true");
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

                                var entities = (JArray)responseData;
                                var entityTypes = entities
                                    .Select(
                                        entity =>
                                        {
                                            var jObj = entity as JObject;
                                            return jObj;
                                        })
                                    .ToArray();
                                return onFound(entityTypes);
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
    }
}
