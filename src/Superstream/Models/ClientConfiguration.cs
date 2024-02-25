namespace Superstream.Models;

#nullable disable
internal class ClientConfiguration
{
  [JsonPropertyName("client_type")]
  public string ClientType { get; set; }

  [JsonPropertyName("producer_max_messages_bytes")]
  public int? ProducerMaxMessageBytes { get; set; }

  [JsonPropertyName("producer_required_acks")]
  public string ProducerRequiredAcks { get; set; }

  [JsonPropertyName("producer_timeout")]
  public int? ProducerTimeout { get; set; }

  [JsonPropertyName("producer_retry_max")]
  public int? ProducerRetryMax { get; set; }

  [JsonPropertyName("producer_retry_backoff")]
  public int? ProducerRetryBackoff { get; set; }

  [JsonPropertyName("producer_return_errors")]
  public bool ProducerReturnErrors { get; set; }

  [JsonPropertyName("producer_return_successes")]
  public bool ProducerReturnSuccesses { get; set; }

  [JsonPropertyName("producer_flush_max_messages")]
  public int? ProducerFlushMaxMessages { get; set; }

  [JsonPropertyName("producer_compression_level")]
  public string ProducerCompressionLevel { get; set; }

  [JsonPropertyName("consumer_fetch_min")]
  public int? ConsumerFetchMin { get; set; }

  [JsonPropertyName("consumer_fetch_default")]
  public int ConsumerFetchDefault { get; set; }

  [JsonPropertyName("consumer_retry_backoff")]
  public int? ConsumerRetryBackOff { get; set; }

  [JsonPropertyName("consumer_max_wait_time")]
  public int? ConsumerMaxWaitTime { get; set; }

  [JsonPropertyName("consumer_max_processing_time")]
  public int? ConsumerMaxProcessingTime { get; set; }

  [JsonPropertyName("consumer_return_errors")]
  public bool ConsumerReturnErrors { get; set; }

  [JsonPropertyName("consumer_offset_auto_commit_enable")]
  public bool? ConsumerOffsetAutoCommitEnable { get; set; }

  [JsonPropertyName("consumer_offset_auto_commit_interval")]
  public int? ConsumerOffsetAutoCommintInterval { get; set; }

  [JsonPropertyName("consumer_offsets_initial")]
  public int ConsumerOffsetsInitial { get; set; }

  [JsonPropertyName("consumer_offsets_retry_max")]
  public int ConsumerOffsetsRetryMax { get; set; }

  [JsonPropertyName("consumer_group_session_timeout")]
  public int? ConsumerGroupSessionTimeout { get; set; }

  [JsonPropertyName("consumer_group_heart_beat_interval")]
  public int? ConsumerGroupHeartBeatInterval { get; set; }

  [JsonPropertyName("consumer_group_rebalance_timeout")]
  public int? ConsumerGroupRebalanceTimeout { get; set; }

  [JsonPropertyName("consumer_group_rebalance_retry_max")]
  public int ConsumerGroupRebalanceRetryMax { get; set; }

  [JsonPropertyName("consumer_group_rebalance_retry_back_off")]
  public int? ConsumerGroupRebalanceRetryBackOff { get; set; }

  [JsonPropertyName("consumer_group_rebalance_reset_invalid_offsets")]
  public bool ConsumerGroupRebalanceResetInvalidOffsets { get; set; }

  [JsonPropertyName("consumer_group_id")]
  public string ConsumerGroupId { get; set; }

  [JsonPropertyName("servers")]
  public string Servers { get; set; }

  [JsonPropertyName("producer_topics_partitions")]
  public Dictionary<string, int[]> ProducerTopicsPartitions { get; set; }

  [JsonPropertyName("consumer_group_topics_partitions")]
  public Dictionary<string, int[]> ConsumerTopicsPartitions { get; set; }

  public ClientConfiguration()
  {
    ProducerTopicsPartitions = new Dictionary<string, int[]>();
    ConsumerTopicsPartitions = new Dictionary<string, int[]>();
  }

  public static implicit operator ClientConfiguration(ProducerConfig producerConfig)
  {
    string requiredAcks = producerConfig.Acks switch
    {
      Acks.None => "NoResponse",
      Acks.Leader => "WaitForLocal",
      Acks.All => "WaitForAll",
      _ => "NoResponse",
    };

    string compressionLevel = producerConfig.CompressionLevel switch
    {
      0 => "CompressionNone",
      1 => "CompressionGZIP",
      2 => "CompressionSnappy",
      3 => "CompressionZSTD",
      0x08 => "compressionCodecMask",
      5 => "timestampTypeMask",
      -1000 => "CompressionLevelDefault",
      _ => "CompressionLevelDefault",
    };

    return new ClientConfiguration
    {
      ClientType = "producer",
      ProducerMaxMessageBytes = producerConfig.MessageMaxBytes,
      ProducerRequiredAcks = requiredAcks,
      ProducerTimeout = producerConfig.RequestTimeoutMs,
      ProducerRetryMax = producerConfig.MessageSendMaxRetries,
      ProducerRetryBackoff = producerConfig.RetryBackoffMs,
      ProducerReturnErrors = true,
      ProducerReturnSuccesses = false,
      ProducerFlushMaxMessages = producerConfig.QueueBufferingMaxMessages,
      ProducerCompressionLevel = compressionLevel,
    };
  }

  public static implicit operator ClientConfiguration(ConsumerConfig consumerConfig)
  {
    var conf = new ClientConfiguration
    {
      ClientType = "consumer",
      ConsumerFetchMin = consumerConfig.FetchMinBytes,
      ConsumerFetchDefault = 1,
      ConsumerRetryBackOff = consumerConfig.ReconnectBackoffMs,
      ConsumerMaxWaitTime = consumerConfig.FetchWaitMaxMs,
      ConsumerMaxProcessingTime = (int)TimeSpan.FromMilliseconds(100).TotalMilliseconds,
      ConsumerReturnErrors = true,
      ConsumerOffsetAutoCommitEnable = consumerConfig.EnableAutoCommit,
      ConsumerOffsetAutoCommintInterval = consumerConfig.AutoCommitIntervalMs,
      ConsumerOffsetsInitial = -1,
      ConsumerOffsetsRetryMax = 3,
      ConsumerGroupSessionTimeout = consumerConfig.SessionTimeoutMs,
      ConsumerGroupHeartBeatInterval = consumerConfig.HeartbeatIntervalMs,
      ConsumerGroupRebalanceTimeout = (int)TimeSpan.FromSeconds(60).TotalMilliseconds,
      ConsumerGroupRebalanceRetryMax = 4,
      ConsumerGroupRebalanceRetryBackOff = (int)TimeSpan.FromSeconds(2).TotalMilliseconds,
      ConsumerGroupRebalanceResetInvalidOffsets = consumerConfig.AutoOffsetReset is not null,
    };

    return conf;
  }
}

#nullable restore
