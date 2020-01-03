using Blazored.LocalStorage;
using EastFive.Api.Resources;
using EastFive.Extensions;
using EastFive.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public class RouteManager
    {
        public struct RouteOption
        {
            public object[] options;
            public string nameType;
        }

        public static IDictionary<string, RouteOption> RefOptions = new Dictionary<string, RouteOption>();
        private static IDictionary<string, Task<RouteOption>> RefCalls = new Dictionary<string, Task<RouteOption>>();
        private static object taskLock = new object();

        public static async Task<RouteOption> LoadTableOptionsAsync(string typeName, ILocalStorageService localStorage)
        {
            var objectsTask = default(Task<RouteOption>);
            lock (taskLock)
            {
                if (RefOptions.ContainsKey(typeName))
                    return RefOptions[typeName];

                if(!RefCalls.ContainsKey(typeName))
                {
                    var fetch = FetchRouteObjectsAsync();
                    RefCalls.Add(typeName, fetch);
                }
                objectsTask = RefCalls[typeName];
            }
            return await objectsTask;

            async Task<RouteOption> FetchRouteObjectsAsync()
            {
                var routeString = await localStorage.GetItemAsync<string>(typeName);
                if (routeString.IsNullOrWhiteSpace())
                {
                    Console.WriteLine($"No route definition for `{typeName}`");
                    return new RouteOption { options = new object[] { } };
                }
                var routeInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Route>(routeString);
                var idProp = routeInfo.Properties
                    .Where(prop => prop.IsIdentfier)
                    .First(
                        (prop, next) => prop.Name,
                        () => "id");
                Console.WriteLine($"ID PROP = `{idProp}`");
                var nameProp = routeInfo.Properties
                    .Where(prop => prop.IsTitle)
                    .First(
                        (prop, next) => prop.Name,
                        () => "id");
                var nameType = routeInfo.Properties
                    .Where(prop => prop.IsTitle)
                    .First(
                        (prop, next) => prop.Type,
                        () => "string");
                Console.WriteLine($"NAME PROP = `{nameProp}`");
                return await TableData.TableAsync(typeName,
                        localStorage,
                    objs =>
                    {
                        var options = objs
                            .Select(
                                obj =>
                                {
                                    var jValObjName = obj[nameProp];
                                    var nameValue = (jValObjName as JValue).Value<string>();

                                    var jValObjId = obj[idProp];
                                    var idValue = (jValObjId as JValue).Value<string>();

                                    return idValue.PairWithKey(nameValue);
                                })
                            .Cast<object>()
                            .ToArray();
                        var routeOption = new RouteOption
                        {
                            nameType = nameType,
                            options = options,
                        };
                        RefOptions.Add(typeName, routeOption);
                        return routeOption;
                    },
                    (why) => new RouteOption { options = new object[] { } });
            }
        }
    }
}
