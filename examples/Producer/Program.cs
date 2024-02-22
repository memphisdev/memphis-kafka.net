using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Superstream;

var token = "<superstream-token>";
var host = "<superstream-host>";
string brokerList = "<brokers>";
string topicName = "<topic-name>";

var config = new ProducerConfig { BootstrapServers = brokerList };
var options = new ProducerBuildOptions
{
  Token = token,
  Host = host,
  ProducerConfig = config
};
using var producer = new ProducerBuilder<string?, byte[]>(config)
  .BuildWithSuperstream(options);
Console.WriteLine("\n-----------------------------------------------------------------------");
Console.WriteLine($"Producer {producer.Name} producing on topic {topicName}.");
Console.WriteLine("-----------------------------------------------------------------------");
var cancelled = false;
Console.CancelKeyPress += (_, e) =>
{
  e.Cancel = true;
  cancelled = true;
};

while (!cancelled)
{
  string key = null;
  var person = new Person
  {
    Name = "John Doe",
    Age = Random.Shared.Next(20, 40)
  };

  try
  {
    var deliveryReport = await producer.ProduceAsync(
        topicName, new() { Key = key, Value = JsonSerializer.SerializeToUtf8Bytes(person) });

    await producer.ProduceAsync("<topic>", new() { Value = JsonSerializer.SerializeToUtf8Bytes("{\"test_key\":\"test_value\"}") });

    Console.WriteLine($"Delivered to: {deliveryReport.TopicPartitionOffset}");
  }
  catch (ProduceException<string, string> e)
  {
    Console.WriteLine($"Failed to deliver message: {e.Message} [{e.Error.Code}]");
  }
}

class Person
{
  [JsonPropertyName("name")]
  public string Name { get; set; }
  [JsonPropertyName("age")]
  public int Age { get; set; }
}