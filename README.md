# Superstream

## Installation

```sh
 dotnet add package Superstream -v ${SUPERSTREAM_VERSION}
```

## Importing

```c#
using Superstream;
```

## Producer

To use `Superstream` with kafka producer, first define the producer configurations:
  
```c#
var config = new ProducerConfig { BootstrapServers = brokerList };
var options = new ProducerBuildOptions
{
  Token = "<superstream-token>",
  Host = "<superstream-host>",
  ProducerConfig = config
};
```

Then create a new instance of kafka producer use `BuildWithSuperstream()`:

```c#
using var producer = new ProducerBuilder<string?, byte[]>(config)
  .BuildWithSuperstream(options);
```
Finally, to produce messages to kafka, use `ProduceAsync` or `Produce`:

```c#
producer.ProduceAsync("<topic>", new() { Value = JsonSerializer.SerializeToUtf8Bytes("{\"test_key\":\"test_value\"}") });
```

## Consumer

To use `Superstream` with kafka consumer, first define the consumer configurations:

```c#
var config = new ConsumerConfig
{
  GroupId = "groupid",
  BootstrapServers = brokerList,
  EnableAutoCommit = false
};
var options = new ConsumerBuildOptions
{
  Token = "<superstream-token>",
  Host = "<superstream-host>",
  ConsumerConfig = config
};
```

Then create a new instance of kafka consumer use `BuildWithSuperstream()`:

```c#
using var consumer = new ConsumerBuilder<Ignore, byte[]>(config)
  .BuildWithSuperstream(options);
```

Finally, to consume messages from kafka, use `Consume`:

```c#
var consumeResult = consumer.Consume();
Console.WriteLine($"Message : ${Encoding.UTF8.GetString(consumeResult.Message.Value)}");
```