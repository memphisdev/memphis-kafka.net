namespace Superstream.Models;

internal class TopicsPartitionsPerProducerConsumer
{
  [JsonPropertyName("producer_topics_partitions")]
  public Dictionary<string, int[]> ProducerTopicsPartitions { get; set; }

  [JsonPropertyName("consumer_group_topics_partitions")]
  public Dictionary<string, int[]> ConsumerTopicsPartitions { get; set; }

  public TopicsPartitionsPerProducerConsumer()
  {
    ProducerTopicsPartitions = new Dictionary<string, int[]>();
    ConsumerTopicsPartitions = new Dictionary<string, int[]>();
  }
}
