namespace Superstream;

internal static class SuperstreamInitializer
{
  public static void Init<TKey, TValue>(
    ref IProducer<TKey, TValue> producer,
    ProducerConfig config,
    string token
  )
  {
    producer = ProducerInterceptor<TKey, TValue>.Init(producer, config, token);
  }

  public static void Init<TKey, TValue>(
    ref IConsumer<TKey, TValue> consumer,
    ConsumerConfig config,
    string token
  )
  {
    consumer = ConsumerInterceptor<TKey, TValue>.Init(consumer, config, token);
  }

  public static IConsumer<TKey, TValue> Init<TKey, TValue>(
    ConsumerBuilder<TKey, TValue> builder,
    ConsumerBuildOptions options
  )
  {
    return builder.BuildWithSuperstream(options);
  }

  public static IProducer<TKey, TValue> Init<TKey, TValue>(
    ProducerBuilder<TKey, TValue> builder,
    ProducerBuildOptions options
  )
  {
    return builder.BuildWithSuperstream(options);
  }
}
