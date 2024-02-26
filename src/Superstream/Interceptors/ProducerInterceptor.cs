namespace Superstream.Interceptors;

internal class ProducerInterceptor<TKey, TValue> : DispatchProxy
{
#nullable disable
  public IProducer<TKey, TValue> Target { get; set; }
  public SuperstreamClient Client { get; set; }

#nullable restore

  private readonly List<string> targetMethodNames = ["Produce", "ProduceAsync"];

  protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
  {
    if (!IsTargetMethod(targetMethod))
    {
      return targetMethod?.Invoke(Target, args);
    }

    if (typeof(TValue) != typeof(byte[]))
      return targetMethod?.Invoke(Target, args);

    int partition = default;
    string topic = string.Empty;
    if (args[0].GetType() == typeof(TopicPartition))
    {
      partition = (args[0] as TopicPartition)!.Partition;
      topic = (args[0] as TopicPartition)!.Topic;
    }
    else
    {
      topic = args[0] as string;
    }
    OnSend(topic, args[1] as Message<TKey, byte[]>, partition);
    var result = targetMethod?.Invoke(Target, args);
    return result;
  }

  private bool IsTargetMethod(MethodInfo? targetMethod)
  {
    return targetMethod != null && targetMethodNames.Contains(targetMethod.Name);
  }

  public void OnSend(string topic, Message<TKey, byte[]> message, int partition = default)
  {
    if (Client.Configuration.ProducerTopicsPartitions == null)
    {
      Client.Configuration.ProducerTopicsPartitions = new Dictionary<string, int[]>();
    }

    if (Client.Configuration.ProducerTopicsPartitions.ContainsKey(topic))
    {
      if (!Client.Configuration.ProducerTopicsPartitions[topic].Contains(partition))
      {
        var partitions = new List<int>(Client.Configuration.ProducerTopicsPartitions[topic])
        {
          partition
        };
        Client.Configuration.ProducerTopicsPartitions[topic] = partitions.ToArray();
      }
    }
    else
    {
      Client.Configuration.ProducerTopicsPartitions[topic] = [partition];
    }

    if (!Client.IsProducer)
    {
      Client.SendClientTypeUpdateRequest("producer");
    }

    byte[] byteMsg;
    try
    {
      byteMsg = message.Value;
    }
    catch (Exception ex)
    {
      Client.HandleError($"{nameof(OnSend)} at encoding message {ex.Message}");
      return;
    }

    Client.Counters.TotalBytesBeforeReduction += byteMsg.Length;

    if (Client.ProducerProtoDescriptor != null)
    {
      byte[] protoMsg;
      try
      {
        protoMsg =
          ProtoBufSerialization
            .JsonToProto(
              Encoding.UTF8.GetString(byteMsg),
              Client.ProducerProtoDescriptor.Descriptor64,
              Client.ProducerProtoDescriptor.FileName,
              Client.ProducerProtoDescriptor.MasterMessageName
            )
            .GetAwaiter()
            .GetResult() ?? throw new Exception("Failed to serialize message");
      }
      catch (Exception ex)
      {
        Client.Counters.TotalBytesAfterReduction += byteMsg.Length;
        Client.Counters.TotalMessagesFailedProduce++;
        return;
      }
      message.Headers ??= [];
      message.Headers.Add("superstream_schema", Encoding.UTF8.GetBytes(Client.ProducerSchemaId));
      Client.Counters.TotalBytesAfterReduction += protoMsg.Length;
      Client.Counters.TotalMessagesSuccessfullyProduce++;
      message.Value = protoMsg;
    }
    else
    {
      Client.Counters.TotalBytesAfterReduction += byteMsg.Length;
      Client.Counters.TotalMessagesFailedProduce++;

      if (Client.LearningFactorCounter <= Client.LearningFactor)
      {
        Client.SendLearningMessage(byteMsg);
        Client.LearningFactorCounter++;
      }
      else if (
        !Client.LearningRequestSent
        && Client.LearningFactorCounter >= Client.LearningFactor
        && Client.ProducerProtoDescriptor == null
      )
      {
        Client.SendRegisterSchemaRequest();
      }
    }
  }

  public static IProducer<K, V> Init<K, V>(
    IProducer<K, V> target,
    ProducerConfig producerConfig,
    string token,
    string host,
    int learningFactor
  )
  {
    var proxy =
      Create<IProducer<K, V>, ProducerInterceptor<K, V>>() as ProducerInterceptor<K, V>
      ?? throw new InvalidOperationException(typeof(IProducer<K, V>).Name);

    proxy.Target = target;
    proxy.Client = InitSuperstream(token, host, producerConfig, learningFactor);
    return proxy as IProducer<K, V>
      ?? throw new InvalidOperationException(typeof(IProducer<K, V>).Name);
  }
}
