using ConnectionInfo = MembershipSystemAPI.Models.ConnectionInfo;

namespace MembershipSystemAPI.Services;

public interface IConnectionManager
{
    void AddConnection(string apiKey, string connectionId,string deviceName);
    void RemoveConnection(string connectionId);
    IEnumerable<ConnectionInfo> GetConnections(string apiKey);
    IEnumerable<ConnectionInfo> GetAllConnections();
}
