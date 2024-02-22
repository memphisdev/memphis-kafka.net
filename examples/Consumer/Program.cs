
using System.Text;
using Confluent.Kafka;
using Superstream;

var token = "<superstream-token>";
var host = "<superstream-host>";
string brokerList = "<brokers>";
var topics = new List<string> { "test-topic" };

Console.WriteLine($"Started consumer, Ctrl-C to stop consuming");

var cancelled = false;
Console.CancelKeyPress += (_, e) =>
{
  e.Cancel = true;
  cancelled = true;
};

var config = new ConsumerConfig
{
  GroupId = "groupid",
  BootstrapServers = brokerList,
  EnableAutoCommit = false
};
var options = new ConsumerBuildOptions
{
  Token = token,
  Host = host,
  ConsumerConfig = config
};

using var consumer = new ConsumerBuilder<Ignore, byte[]>(config)
        .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
        .BuildWithSuperstream(options);


consumer.Assign(topics.Select(topic => new TopicPartitionOffset(topic, 0, Offset.Beginning)).ToList());

try
{
  while (!cancelled)
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
