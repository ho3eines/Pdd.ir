using System.Collections.Concurrent;
using System.Text;

namespace Pdd.ir.Server.Services
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _connections = new();

        public void AddConnection(string connectionId, System.Net.WebSockets.WebSocket socket)
        {
            _connections[connectionId] = socket;
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public async Task SendToConnectionAsync(string connectionId, string message)
        {
            if (_connections.TryGetValue(connectionId, out var socket) && socket.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task BroadcastAsync(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var connection in _connections)
            {
                if (connection.Value.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    try
                    {
                        await connection.Value.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch { }
                }
            }
        }

        public int ConnectionCount => _connections.Count;
    }
}
