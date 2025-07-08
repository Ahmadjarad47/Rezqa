namespace Rezqa.API.Hubs
{
    public class PresenceTracker
    {
        private readonly static Dictionary<string, List<(string ConnectionId, DateTime LastActive)>> onLineUser = new();

        public Task<bool> UserConnected(string username, string conid)
        {
            bool isOnline = false;
            lock (onLineUser)
            {
                if (onLineUser.ContainsKey(username))
                {
                    onLineUser[username].Add((conid, DateTime.UtcNow));

                    // إذا زاد عدد الاتصالات عن 3، احذف الأقدم
                    if (onLineUser[username].Count > 3)
                    {
                        onLineUser[username].RemoveAt(0); // حذف أول عنصر (الأقدم)
                    }
                }
                else
                {
                    onLineUser.Add(username, new List<(string, DateTime)> { (conid, DateTime.UtcNow) });
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public async Task<List<string>> GetOnline()
        {
            return onLineUser.Keys.ToList();
        }
        public Task<List<string>> GetConnectionForUser(string username)
        {
            List<string> conncetionIds = new List<string>();

            lock (onLineUser)
            {
                if (onLineUser.TryGetValue(username, out var connections))
                {
                    conncetionIds.AddRange(connections.Select(c => c.ConnectionId));
                }
            }
            return Task.FromResult(conncetionIds);
        }

        public Task<List<string>> GetConnectionsForUsers(string[] usernames)
        {
            List<string> conncetionIds = new List<string>();

            lock (onLineUser)
            {
                foreach (var username in usernames)
                {
                    if (onLineUser.TryGetValue(username, out var connections))
                    {
                        conncetionIds.AddRange(connections.Select(c => c.ConnectionId));
                    }
                }
            }
            return Task.FromResult(conncetionIds);
        }

        public Task<bool> UserDisConnected(string username, string connectionid)
        {
            bool isOffline = false;
            lock (onLineUser)
            {
                if (!onLineUser.ContainsKey(username))
                {
                    return Task.FromResult(isOffline);
                }
                onLineUser[username].RemoveAll(c => c.ConnectionId == connectionid);
                if (onLineUser[username].Count == 0)
                {
                    onLineUser.Remove(username);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        // دالة تنظيف الاتصالات القديمة
        public void CleanupOldConnections(TimeSpan maxAge)
        {
            lock (onLineUser)
            {
                var now = DateTime.UtcNow;
                var usersToRemove = new List<string>();

                foreach (var user in onLineUser.Keys.ToList())
                {
                    onLineUser[user].RemoveAll(conn => (now - conn.LastActive) > maxAge);

                    if (onLineUser[user].Count == 0)
                    {
                        usersToRemove.Add(user);
                    }
                }

                foreach (var user in usersToRemove)
                {
                    onLineUser.Remove(user);
                }
            }
        }
    }





    public class PresenceMessageTracker
    {
        private readonly static Dictionary<string, List<(string ConnectionId, DateTime LastActive)>> onLineUser = new();

        public Task<bool> UserConnected(string username, string conid)
        {
            bool isOnline = false;
            lock (onLineUser)
            {
                if (onLineUser.ContainsKey(username))
                {
                    onLineUser[username].Add((conid, DateTime.UtcNow));

                    // إذا زاد عدد الاتصالات عن 3، احذف الأقدم
                    if (onLineUser[username].Count > 3)
                    {
                        onLineUser[username].RemoveAt(0); // حذف أول عنصر (الأقدم)
                    }
                }
                else
                {
                    onLineUser.Add(username, new List<(string, DateTime)> { (conid, DateTime.UtcNow) });
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public async Task<List<string>> GetOnlinePartner(string[] usernames)
        {
            var onlineUser = new List<string>();
            lock (onLineUser)
            {
                foreach (var item in usernames)
                {
                    if (onLineUser.ContainsKey(item))
                    {
                        onlineUser.Add(item);
                    }
                }
            }
            return onlineUser;
        }
        public Task<List<string>> GetConnectionForUser(string username)
        {
            List<string> conncetionIds = new List<string>();

            lock (onLineUser)
            {
                if (onLineUser.TryGetValue(username, out var connections))
                {
                    conncetionIds.AddRange(connections.Select(c => c.ConnectionId));
                }
            }
            return Task.FromResult(conncetionIds);
        }

        public Task<List<string>> GetConnectionsForUsers(string[] usernames)
        {
            List<string> conncetionIds = new List<string>();

            lock (onLineUser)
            {
                foreach (var username in usernames)
                {
                    if (onLineUser.TryGetValue(username, out var connections))
                    {
                        conncetionIds.AddRange(connections.Select(c => c.ConnectionId));
                    }
                }
            }
            return Task.FromResult(conncetionIds);
        }

        public Task<bool> UserDisConnected(string username, string connectionid)
        {
            bool isOffline = false;
            lock (onLineUser)
            {
                if (!onLineUser.ContainsKey(username))
                {
                    return Task.FromResult(isOffline);
                }
                onLineUser[username].RemoveAll(c => c.ConnectionId == connectionid);
                if (onLineUser[username].Count == 0)
                {
                    onLineUser.Remove(username);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);
        }

        // دالة تنظيف الاتصالات القديمة
        public void CleanupOldConnections(TimeSpan maxAge)
        {
            lock (onLineUser)
            {
                var now = DateTime.UtcNow;
                var usersToRemove = new List<string>();

                foreach (var user in onLineUser.Keys.ToList())
                {
                    onLineUser[user].RemoveAll(conn => (now - conn.LastActive) > maxAge);

                    if (onLineUser[user].Count == 0)
                    {
                        usersToRemove.Add(user);
                    }
                }

                foreach (var user in usersToRemove)
                {
                    onLineUser.Remove(user);
                }
            }
        }
    }
}
