namespace Superstream.Models;

#nullable disable
internal class RegisterResponse
{
  [JsonPropertyName("client_id")]
  public int ClientId { get; set; }

  [JsonPropertyName("account_name")]
  public string AccountName { get; set; }

  [JsonPropertyName("learning_factor")]
  public int LearningFactor { get; set; }
}
#nullable restore
