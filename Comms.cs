using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using EastFive.Admin.UI.Resources;
using EastFive.Api.Resources;

namespace EastFive.Admin.UI
{
    public static class Comms
    {
        public static string token = "SECURITY";

        public static async Task<TResult> GetManifestAsync<TResult>(
            Func<Manifest, TResult> onFound,
            Func<string, TResult> onFailure)
        {
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://affirmhealthshield-dev.azurewebsites.net/api/Manifest")) // "http://192.168.1.5:54593/api/notification"))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    try
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            try
                            {
                                var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(json);
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

        public static async Task<TResult> GetNotifications<TResult>(
            Func<Notification[], TResult> onFound,
            Func<string, TResult> onFailure)
        {
            using(var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get,
                    "https://affirmhealthshield-dev.azurewebsites.net/api/notification")) // "http://192.168.1.5:54593/api/notification"))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(token);
                    try
                    {
                        using (var response = await client.SendAsync(request))
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            try
                            {
                                var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<Notification[]>(json);
                                return onFound(notifications);
                            } catch(Exception)
                            {
                                return onFailure(json);
                            }
                        }
                    } catch(Exception ex)
                    {
                        return onFailure(ex.Message);
                    }
                }
            }
        }
    }
}
