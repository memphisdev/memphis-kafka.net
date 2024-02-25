using Superstream.Utils;

namespace Superstream;

internal static class IConnectionExtensions
{
  public static string GenerateNatsConnectionId(this IConnection connection)
  {
    var connectionId = connection.ConnectedId;
    var serverName = connection.ServerInfo.ServerName;
    return $"{serverName}:{connectionId}";
  }

  public static void SendClientErrorToBackend(this IConnection connection, string error)
  {
    connection.Publish(Subjects.SuperstreamErrorSubject, Encoding.UTF8.GetBytes(error));
  }
}
