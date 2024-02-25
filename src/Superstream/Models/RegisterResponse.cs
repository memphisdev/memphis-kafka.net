namespace Superstream.Models;

#nullable disable
internal class RegisterResponse
{
  // [JsonPropertyName("client_id")]
  [JsonPropertyName("clientId")]
  public int ClientId { get; set; }

  // [JsonPropertyName("account_name")]
  [JsonPropertyName("accountName")]
  public string AccountName { get; set; }

  // [JsonPropertyName("learning_factor")]
  [JsonPropertyName("learningFactor")]
  public int LearningFactor { get; set; }
}
#nullable restore
