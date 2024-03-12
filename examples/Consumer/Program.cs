
using System.Text;
using Confluent.Kafka;
using Superstream;

var token = "<superstream-token>";
var host = "<superstream-host>";
string brokerList = "<brokers>";
var topics = new List<string> { "test" };

Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

var cts = new CancellationTokenSource();
var config = new ConsumerConfig
{
  GroupId = "cg",
  BootstrapServers = brokerList,
  EnableAutoCommit = false,
  SaslPassword = "...",
  SaslUsername = "...",
  SecurityProtocol = SecurityProtocol.SaslSsl,
  SaslMechanism = SaslMechanism.Plain
};
var options = new ConsumerBuildOptions
{
  Token = token,
  Host = host,
  ConsumerConfig = config
};

var kafkaConsumer = new ConsumerBuilder<Ignore, byte[]>(config)
        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
        .Build();
using var consumer = SuperstreamInitializer.Init(kafkaConsumer, options);

// consume by specific partition
consumer.Assign(topics.Select(topic => new TopicPartitionOffset(topic, 1, Offset.Beginning)).ToList());
// consume from all partitions
// consumer.Subscribe(topics);
try
{
  while (!cts.IsCancellationRequested)
  {
    try
    {
      var consumeResult = consumer.Consume();
      Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}");
      Console.WriteLine($"Message : ${Encoding.UTF8.GetString(consumeResult.Message.Value)}");
    }
    catch (ConsumeException e)
    {
      Console.WriteLine($"Consume error: {e.Error.Reason}");
    }
  }
}
catch (OperationCanceledException)
{
  Console.WriteLine("Closing consumer.");
  consumer.Close();
}
