using Blazored.LocalStorage;
using EastFive.Extensions;
using EastFive.Linq;
using EastFive.Linq.Async;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public struct Server : IReferenceable
    {
        public const string ServersLocalStorageKey = "servers";

        public static Server? selectedServer = default;

        public static List<Server> Servers = new List<Server>();

        public Guid id;

        public string title;

        public Uri serverLocation;

        public Api.Resources.Route[] endpoints;

        public Api.Resources.Route[] EntryPoints
        {
            get
            {
                return this.endpoints
                    .Where(ep => ep.IsEntryPoint)
                    .ToArray();
            }
        }

        Guid IReferenceable.id => this.id;

        public static async Task OnInitializedAsync(ILocalStorageService localStorage)
        {
            var serverIdsJson = await localStorage.GetItemAsync<string>("server_ids");
            Console.WriteLine($"Available Stored Server Ids = `{serverIdsJson}`");
            if (serverIdsJson.IsNullOrWhiteSpace())
                return;
            var serverIds = Newtonsoft.Json.JsonConvert.DeserializeObject<Guid[]>(serverIdsJson);

            var servers = await serverIds
                .NullToEmpty()
                .Select(
                    async serverId =>
                    {
                        var serverJson = await localStorage.GetItemAsync<string>(serverId.ToString("N"));
                        Console.WriteLine($"LOADED: Stored Server:{serverId}");
                        if (serverJson.IsNullOrWhiteSpace())
                        {
                            await Server.DeleteServer(serverId, localStorage);
                            return default(Server?);
                        }
                        var server = Newtonsoft.Json.JsonConvert.DeserializeObject<Server>(serverJson);
                        return server;
                    })
                .AsyncEnumerable()
                .SelectWhereHasValue()
                .ToArrayAsync();
            Servers = new List<Server>(servers);

            var selectedServerIdStr = await localStorage.GetItemAsync<string>("selected_server_id");
            Console.WriteLine($"SELECTING:Server Id = `{selectedServerIdStr}`");
            if (selectedServerIdStr.HasBlackSpace())
            {
                if(Guid.TryParse(selectedServerIdStr, out Guid selectedServerId))
                    selectedServer = Servers
                        .Where(server => server.id == selectedServerId)
                        .First(
                            (server, next) => server,
                            () => default(Server?));
            }
        }

        public static async Task SetSelectedServer(Guid serverId,
            ILocalStorageService localStorage)
        {
            bool updated = await Server.Servers
                .Where(server => server.id == serverId)
                .First(
                    async (server, next) =>
                    {
                        Server.selectedServer = server;
                        await localStorage.SetItemAsync(
                            "selected_server_id", serverId.ToString());
                        return true;
                    },
                    () => false.AsTask());
            return;
        }

        public static async Task SaveServer(Server server, ILocalStorageService localStorage)
        {
            bool success = await await Comms.GetManifestAsync(server.serverLocation,
                async endpoints =>
                {
                    Console.WriteLine($"Downloaded endpoints for `{server.serverLocation}`");
                    server.endpoints = endpoints;

                    await localStorage.SetItemAsync(server.id.ToString("N"), Newtonsoft.Json.JsonConvert.SerializeObject(server));
                    var existingServer = Server.Servers.Where(s => s.id == server.id);
                    if (existingServer.Any())
                        Server.Servers.Remove(existingServer.First());
                    Server.Servers.Add(server);
                    var serverIds = Server.Servers.Select(s => s.id).Distinct().ToArray();
                    await localStorage.SetItemAsync("server_ids", Newtonsoft.Json.JsonConvert.SerializeObject(serverIds));
                    await localStorage.SetItemAsync("selected_server_id", server.id.ToString("N"));
                    return true;
                },
                (message) =>
                {
                    Console.WriteLine($"FAILURE to downloaded endpoints for `{server.serverLocation}`:{message}");
                    //this.failureMessage = message;
                    return false.AsTask();
                });
        }

        public static async Task DeleteServer(Guid serverId, ILocalStorageService localStorage)
        {
            Console.WriteLine($"Starting with {Server.Servers.Count()} Servers");
            bool removed = Server.Servers
                .Where(server => server.id == serverId)
                .First(
                    (server, next) =>
                    {
                        Console.WriteLine($"Removed {server.title} / {server.id} from memory.");
                        Server.Servers.Remove(server);
                        return true;
                    },
                    () => false);
            await localStorage.RemoveItemAsync(serverId.ToString("N"));
            Console.WriteLine($"Removed {serverId} from local storage.");

            var serverIds = Server.Servers
                .Select(s => s.id)
                .Distinct()
                .Where(id => id != serverId)
                .ToArray();
            Console.WriteLine($"Saving {serverIds.Length} Server IDs");
            await localStorage.SetItemAsync("server_ids", Newtonsoft.Json.JsonConvert.SerializeObject(serverIds));
        }
    }

}
