namespace Superstream.Models;

#nullable disable
internal class ClientReconnectionUpdateRequest
{
  [JsonPropertyName("new_nats_connection_id")]
  public string NewNatsConnectionId { get; set; }

  [JsonPropertyName("client_id")]
  public int ClientId { get; set; }
}
#nullable restore
