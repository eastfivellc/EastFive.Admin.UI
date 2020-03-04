using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Blazored.LocalStorage;

using Radzen;

using EastFive.Linq;
using EastFive.Extensions;
using EastFive.Linq.Async;

namespace EastFive.Admin.UI
{
    public struct Session
    {
        public Guid id;

        public const string SessionsLocalStorageKey = "sessions";

        public static Session? selectedSession = default;

        public static List<Session> Sessions = new List<Session>();

        public string title;

        public string token;

        public Guid serverId;

        public static event EventHandler<Session> OnSessionSelected;

        public static async Task OnInitializedAsync(ILocalStorageService localStorage)
        {
            var sessionIdsJson = await localStorage.GetItemAsync<string>("session_ids");
            Console.WriteLine($"Available Stored Session Ids = `{sessionIdsJson}`");
            if (sessionIdsJson.IsNullOrWhiteSpace())
                return;
            var sessionIds = Newtonsoft.Json.JsonConvert.DeserializeObject<Guid[]>(sessionIdsJson);

            var sessions = await sessionIds
                .NullToEmpty()
                .Select(
                    async sessionId =>
                    {
                        var sessionJson = await localStorage.GetItemAsync<string>(sessionId.ToString("N"));
                        if (sessionJson.IsNullOrWhiteSpace())
                        {
                            Console.WriteLine($"Session [{sessionId}] not found in storage");
                            await Session.DeleteSession(sessionId, localStorage);
                            return default(Session?);
                        }
                        var session = Newtonsoft.Json.JsonConvert.DeserializeObject<Session>(sessionJson);
                        if(!Server.Servers.Select(server => server.id).Contains(session.serverId))
                        {
                            Console.WriteLine($"Session [{sessionId}]'s server has been deleted; deleting session.");
                            await Session.DeleteSession(sessionId, localStorage);
                            return default(Session?);
                        }
                        return session;
                    })
                .AsyncEnumerable()
                .SelectWhereHasValue()
                .ToArrayAsync();
            Session.Sessions = new List<Session>(sessions);

            var selectedSessionIdStr = await localStorage.GetItemAsync<string>("selected_session_id");
            Console.WriteLine($"SELECTING: Session Id = `{selectedSessionIdStr}`");
            if (selectedSessionIdStr.HasBlackSpace())
            {
                if (Guid.TryParse(selectedSessionIdStr, out Guid selectedSessionId))
                {
                    selectedSession = Sessions
                        .Where(session => session.id == selectedSessionId)
                        .First(
                            (session, next) => session,
                            () => default(Session?));
                    if(selectedSession.HasValue)
                        OnSessionSelected?.Invoke(null, selectedSession.Value);
                }
                if (!selectedSession.HasValue)
                {
                    Console.WriteLine($"INVALID Stored Selected Session Id = `{selectedSessionIdStr}`, deleting...");
                    await localStorage.RemoveItemAsync("selected_session_id");
                    return;
                }
                if (!Server.Servers.Select(server => server.id).Contains(selectedSession.Value.serverId))
                {
                    Console.WriteLine($"Session [{selectedSession.Value.id}]'s server has been deleted; unselecting session.");
                    await localStorage.RemoveItemAsync("selected_session_id");
                    selectedSession = default(Session?);
                    return;
                }
            }
        }

        public static async Task CreateSession(Session session, ILocalStorageService localStorage)
        {
            await localStorage.SetItemAsync(session.id.ToString("N"), 
                Newtonsoft.Json.JsonConvert.SerializeObject(session));
            Session.Sessions.Add(session);
            var sessionIds = Session.Sessions.Select(s => s.id).Distinct().ToArray();
            await localStorage.SetItemAsync("session_ids", Newtonsoft.Json.JsonConvert.SerializeObject(sessionIds));
            await localStorage.SetItemAsync("selected_session_id", session.id.ToString("N"));
        }

        public static async Task UpdateSession(Session session, ILocalStorageService localStorage)
        {
            await DeleteSession(session.id, localStorage);
            await CreateSession(session, localStorage);
        }

        public static async Task DeleteSession(Guid sessionId, ILocalStorageService localStorage)
        {
            Console.WriteLine($"Starting with {Session.Sessions.Count()} Sessions");
            bool removed = Session.Sessions
                .ToArray()
                .Where(session => session.id == sessionId)
                .First(
                    (session, next) =>
                    {
                        Console.WriteLine($"Removed {session.title} / {session.id} from memory.");
                        Session.Sessions.Remove(session);
                        return true;
                    },
                    () => false);
            await localStorage.RemoveItemAsync(sessionId.ToString("N"));
            Console.WriteLine($"Removed {sessionId} from local storage.");

            var sessionIds = Session.Sessions
                .Select(s => s.id)
                .Where(id => id != sessionId)
                .Distinct()
                .ToArray();
            Console.WriteLine($"Saving {sessionIds.Length} Session IDs");
            await localStorage.SetItemAsync("session_ids", 
                Newtonsoft.Json.JsonConvert.SerializeObject(sessionIds));
        }

        public static async Task SelectSessionAsync(Session newSelectedSession, ILocalStorageService localStorage)
        {
            Session.selectedSession = newSelectedSession;
            await localStorage.SetItemAsync(
                "selected_session_id", newSelectedSession.id.ToString());
            OnSessionSelected?.Invoke(null, newSelectedSession);
        }
    }
}
