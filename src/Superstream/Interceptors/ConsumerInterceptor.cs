namespace Superstream.Interceptors;

internal class ConsumerInterceptor<TKey, TValue> : DispatchProxy
{
#nullable disable
  public IConsumer<TKey, TValue> Target { get; set; }
  public SuperstreamClient Client { get; set; }

#nullable restore

  private readonly List<string> targetMethodNames = ["Consume", "ConsumeAsync"];

  protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
  {
    if (!IsTargetMethod(targetMethod))
    {
      return targetMethod?.Invoke(Target, args);
    }
    var result = targetMethod?.Invoke(Target, args);
    if (typeof(TValue) != typeof(byte[]))
      return result;
    int partition = default;
    OnConsume(result as ConsumeResult<TKey, byte[]>, partition);
    return result;
  }

  public void OnConsume(ConsumeResult<TKey, byte[]> result, int partition)
  {
    Console.WriteLine("on consume");
    if (!Client.IsConsumer)
    {
      Client.SendClientTypeUpdateRequest("consumer");
    }

    if (Client.Configuration.ConsumerTopicsPartitions == null)
    {
      Client.Configuration.ConsumerTopicsPartitions = [];
    }

    if (Client.Configuration.ConsumerTopicsPartitions.ContainsKey(result.Topic))
    {
      if (!Client.Configuration.ConsumerTopicsPartitions[result.Topic].Contains(result.Partition))
      {
        var partitions = new List<int>(Client.Configuration.ConsumerTopicsPartitions[result.Topic]);
        partitions.Add(result.Partition);
        Client.Configuration.ConsumerTopicsPartitions[result.Topic] = partitions.ToArray();
      }
    }
    else
    {
      Client.Configuration.ConsumerTopicsPartitions[result.Topic] = [partition];
    }

    Client.Counters.TotalBytesAfterReduction += result.Message.Value.Length;

    for (int i = 0; i < result.Message.Headers.Count; i++)
    {
      var header = result.Message.Headers[i];
      if (string.Equals(header.Key, "superstream_schema"))
      {
        var schemaID = Encoding.UTF8.GetString(header.GetValueBytes());
        if (!Client.ConsumerProtoDescriptors.ContainsKey(schemaID))
        {
          if (!Client.GetSchemaRequestSent)
          {
            Client.SendGetSchemaRequest(schemaID);
          }

          while (!Client.ConsumerProtoDescriptors.ContainsKey(schemaID))
          {
            Thread.Sleep(500);
          }
        }

        if (Client.ConsumerProtoDescriptors.TryGetValue(schemaID, out var rawDesc))
        {
          try
          {
            var json64 = ProtoBufSerialization
              .ProtoToJson(
                result.Message.Value,
                rawDesc.Descriptor64,
                rawDesc.FileName,
                rawDesc.MasterMessageName
              )
              .GetAwaiter()
              .GetResult();
            var removeKey = result.Message.Headers[i].Key;
            result.Message.Headers.Remove(removeKey);
            result.Message.Value = Convert.FromBase64String(json64);
            Client.Counters.TotalBytesBeforeReduction += json64.Length;
            Client.Counters.TotalMessagesSuccessfullyConsumed++;
          }
          catch (Exception ex)
          {
            Client.HandleError($"{nameof(OnConsume)} at ProtoToJson {ex.Message}");
            return;
          }
        }
        else
        {
          Client.HandleError($"{nameof(OnConsume)} schema not found");
          Console.WriteLine("superstream: schema not found");
          return;
        }
        return;
      }
    }
    Client.Counters.TotalBytesBeforeReduction += result.Message.Value.Length;
    Client.Counters.TotalMessagesFailedConsume++;
  }

  private bool IsTargetMethod(MethodInfo? targetMethod)
  {
    return targetMethod != null && targetMethodNames.Contains(targetMethod.Name);
  }

  public static IConsumer<K, V> Init<K, V>(
    IConsumer<K, V> target,
    ConsumerConfig consumerConfig,
    string token,
    string host,
    int learningFactor
  )
  {
    var proxy =
      Create<IConsumer<K, V>, ConsumerInterceptor<K, V>>() as ConsumerInterceptor<K, V>
      ?? throw new InvalidOperationException(typeof(IConsumer<K, V>).Name);

    proxy.Target = target;
    proxy.Client = InitSuperstream(token, host, consumerConfig, learningFactor);
    return proxy as IConsumer<K, V>
      ?? throw new InvalidOperationException(typeof(IConsumer<K, V>).Name);
  }
}
