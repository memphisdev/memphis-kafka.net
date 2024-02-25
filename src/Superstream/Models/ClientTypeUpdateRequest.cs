namespace Superstream.Models;

#nullable disable
internal class ClientTypeUpdateRequest
{
  [JsonPropertyName("client_id")]
  public int ClientId { get; set; }

  [JsonPropertyName("type")]
  public string Type { get; set; }
}
#nullable restore
