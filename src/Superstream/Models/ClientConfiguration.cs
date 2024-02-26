namespace Superstream.Models;

#nullable disable
internal class ClientConfiguration
{
  [JsonPropertyName("consumer_group")]
  public string ConsumerGroup { get; set; }

  [JsonPropertyName("client_type")]
  public string ClientType { get; set; }

  [JsonPropertyName("producer_required_acks")]
  public string ProducerRequiredAcks { get; set; }

  [JsonPropertyName("producer_timeout")]
  public int? ProducerTimeout { get; set; }

  [JsonPropertyName("producer_retry_max")]
  public int? ProducerRetryMax { get; set; }

  [JsonPropertyName("producer_retry_backoff")]
  public int? ProducerRetryBackoff { get; set; }

  [JsonPropertyName("producer_compression_level")]
  public string ProducerCompressionLevel { get; set; }

  [JsonPropertyName("consumer_fetch_min")]
  public int? ConsumerFetchMin { get; set; }

  [JsonPropertyName("consumer_retry_backoff")]
  public int? ConsumerRetryBackOff { get; set; }

  [JsonPropertyName("consumer_max_wait_time")]
  public int? ConsumerMaxWaitTime { get; set; }

  [JsonPropertyName("consumer_offset_auto_commit_enable")]
  public bool? ConsumerOffsetAutoCommitEnable { get; set; }

  [JsonPropertyName("consumer_offset_auto_commit_interval")]
  public int? ConsumerOffsetAutoCommintInterval { get; set; }


  [JsonPropertyName("consumer_group_session_timeout")]
  public int? ConsumerGroupSessionTimeout { get; set; }

  [JsonPropertyName("consumer_group_heart_beat_interval")]
  public int? ConsumerGroupHeartBeatInterval { get; set; }


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
      Acks.None => "NoRequired",
      Acks.Leader => "RequiredForLeaderOnly",
      Acks.All => "RequiredForAll",
      _ => "NoRequired",
    };

    string compressionType = producerConfig.CompressionType switch
    {
      Confluent.Kafka.CompressionType.None => "CompressionNone",
      Confluent.Kafka.CompressionType.Gzip => "CompressionGZIP",
      Confluent.Kafka.CompressionType.Snappy => "CompressionSnappy",
      Confluent.Kafka.CompressionType.Lz4 => "CompressionLz4",
      Confluent.Kafka.CompressionType.Zstd => "CompressionZstd",
      _ => "CompressionLevelDefault",
    };

    return new ClientConfiguration
    {
      ClientType = "producer",
      ProducerRequiredAcks = requiredAcks,
      ProducerTimeout = producerConfig.RequestTimeoutMs,
      ProducerRetryMax = producerConfig.MessageSendMaxRetries,
      ProducerRetryBackoff = producerConfig.RetryBackoffMs,
      ProducerCompressionLevel = compressionType,
      Servers = producerConfig.BootstrapServers
    };
  }

  public static implicit operator ClientConfiguration(ConsumerConfig consumerConfig)
  {
    var conf = new ClientConfiguration
    {
      ClientType = "consumer",
      ConsumerFetchMin = consumerConfig.FetchMinBytes,
      ConsumerRetryBackOff = consumerConfig.ReconnectBackoffMs,
      ConsumerMaxWaitTime = consumerConfig.FetchWaitMaxMs,
      ConsumerOffsetAutoCommitEnable = consumerConfig.EnableAutoCommit,
      ConsumerOffsetAutoCommintInterval = consumerConfig.AutoCommitIntervalMs,
      ConsumerGroupSessionTimeout = consumerConfig.SessionTimeoutMs,
      ConsumerGroupHeartBeatInterval = consumerConfig.HeartbeatIntervalMs,
      ConsumerGroupRebalanceResetInvalidOffsets = consumerConfig.AutoOffsetReset is not null,
      Servers = consumerConfig.BootstrapServers,
      ConsumerGroupId = consumerConfig.GroupId
    };

    return conf;
  }
}

#nullable restore
