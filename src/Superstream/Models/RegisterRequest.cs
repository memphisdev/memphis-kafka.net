namespace Superstream.Models;

#nullable disable

internal class RegisterRequest
{
  [JsonPropertyName("nats_connection_id")]
  public string NatsConnectionId { get; set; }

  [JsonPropertyName("language")]
  public string Language { get; set; }

  [JsonPropertyName("version")]
  public string Version { get; set; }

  [JsonPropertyName("learning_factor")]
  public int LearningFactor { get; set; }

  [JsonPropertyName("config")]
  public ClientConfiguration Configuration { get; set; }
}

#nullable restore
