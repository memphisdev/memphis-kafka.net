namespace Superstream.Models;

internal class ClientCounters
{
  // [JsonPropertyName("total_bytes_before_reduction")]
  [JsonPropertyName("totalBytesBeforeReduction")]
  public long TotalBytesBeforeReduction { get; set; }

  // [JsonPropertyName("total_bytes_after_reduction")]
  [JsonPropertyName("totalBytesAfterReduction")]
  public long TotalBytesAfterReduction { get; set; }

  // [JsonPropertyName("total_messages_successfully_produce")]
  [JsonPropertyName("totalMessagesSuccessfullyProduce")]
  public int TotalMessagesSuccessfullyProduce { get; set; }

  // [JsonPropertyName("total_messages_successfully_consumed")]
  [JsonPropertyName("totalMessagesSuccessfullyConsumed")]
  public int TotalMessagesSuccessfullyConsumed { get; set; }

  // [JsonPropertyName("total_messages_failed_produce")]
  [JsonPropertyName("totalMessagesFailedProduce")]
  public int TotalMessagesFailedProduce { get; set; }

  // [JsonPropertyName("total_messages_failed_consume")]
  [JsonPropertyName("totalMessagesFailedConsume")]
  public int TotalMessagesFailedConsume { get; set; }
}
