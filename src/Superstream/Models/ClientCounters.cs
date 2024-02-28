namespace Superstream.Models;

internal class ClientCounters
{
  [JsonPropertyName("total_bytes_before_reduction")]
  public long TotalBytesBeforeReduction { get; set; }

  [JsonPropertyName("total_bytes_after_reduction")]
  public long TotalBytesAfterReduction { get; set; }

  [JsonPropertyName("total_messages_successfully_produce")]
  public int TotalMessagesSuccessfullyProduce { get; set; }

  [JsonPropertyName("total_messages_successfully_consumed")]
  public int TotalMessagesSuccessfullyConsumed { get; set; }

  [JsonPropertyName("total_messages_failed_produce")]
  public int TotalMessagesFailedProduce { get; set; }

  [JsonPropertyName("total_messages_failed_consume")]
  public int TotalMessagesFailedConsume { get; set; }
}
