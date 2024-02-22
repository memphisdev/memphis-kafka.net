namespace Superstream;

internal static class SuperstreamInitializer
{
  public static void Init<TKey, TValue>(
    ref IProducer<TKey, TValue> producer,
    ProducerBuildOptions options
  )
  {
    options.EnsureIsValid();
    producer = ProducerInterceptor<TKey, TValue>.Init(
      producer,
      options.ProducerConfig,
      options.Token,
      options.Host
    );
  }

  public static void Init<TKey, TValue>(
    ref IConsumer<TKey, TValue> consumer,
    ConsumerBuildOptions options
  )
  {
    consumer = ConsumerInterceptor<TKey, TValue>.Init(
      consumer,
      options.ConsumerConfig,
      options.Token,
      options.Host
    );
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
