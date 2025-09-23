
using ConnectionInfo = MembershipSystemAPI.Models.ConnectionInfo;

namespace MembershipSystemAPI.Services;

public class InMemoryConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<ConnectionInfo>> _connections = new();
    private readonly ConcurrentDictionary<string, string> _connectionIdToApiKey = new();
    public void AddConnection(string apiKey, string connectionId, string deviceName)
    {
        _connectionIdToApiKey[connectionId] = apiKey;
        var connectionBag = _connections.GetOrAdd(apiKey, _ => new ConcurrentBag<ConnectionInfo>());
        connectionBag.Add(new ConnectionInfo
        {
            ConnectionId = connectionId,
            DeviceName = deviceName,
        });
    }

    public IEnumerable<ConnectionInfo> GetAllConnections()
    {
        var allConnections = new List<ConnectionInfo>();
        foreach (var bag in _connections.Values)
        {
            allConnections.AddRange(bag);
        }
        return allConnections;
    }

    public IEnumerable<Models.ConnectionInfo> GetConnections(string apiKey)
    {
        _connections.TryGetValue(apiKey, out var connectionBag);
        return connectionBag ?? Enumerable.Empty<Models.ConnectionInfo>();
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connectionIdToApiKey.TryGetValue(connectionId, out var apiKey))
        {
            if (_connections.TryGetValue(apiKey, out var connectionBag))
            {
                var updatedBag = new ConcurrentBag<ConnectionInfo>(connectionBag.Where(c => c.ConnectionId != connectionId));
                _connections[apiKey] = updatedBag;
                if (updatedBag.IsEmpty)
                {
                    _connections.TryRemove(apiKey, out _);
                }
            }
        }
    }
}
