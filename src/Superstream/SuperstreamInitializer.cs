namespace Superstream;

public static class SuperstreamInitializer
{
  internal static void Init<TKey, TValue>(
    ref IProducer<TKey, TValue> producer,
    ProducerBuildOptions options
  )
  {
    options.EnsureIsValid();
    producer = ProducerInterceptor<TKey, TValue>.Init(
      producer,
      options.ProducerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }

  internal static void Init<TKey, TValue>(
    ref IConsumer<TKey, TValue> consumer,
    ConsumerBuildOptions options
  )
  {
    consumer = ConsumerInterceptor<TKey, TValue>.Init(
      consumer,
      options.ConsumerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }

  internal static IConsumer<TKey, TValue> Init<TKey, TValue>(
    ConsumerBuilder<TKey, TValue> builder,
    ConsumerBuildOptions options
  )
  {
    return builder.BuildWithSuperstream(options);
  }

  internal static IProducer<TKey, TValue> Init<TKey, TValue>(
    ProducerBuilder<TKey, TValue> builder,
    ProducerBuildOptions options
  )
  {
    return builder.BuildWithSuperstream(options);
  }

  public static IProducer<K, V> Init<K, V>(
    IProducer<K, V> target,
    ProducerBuildOptions options
  )
  {
    options.EnsureIsValid();
    return ProducerInterceptor<K, V>.Init(
      target,
      options.ProducerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }

  public static IConsumer<K, V> Init<K, V>(
    IConsumer<K, V> target,
    ConsumerBuildOptions options
  )
  {
    return ConsumerInterceptor<K, V>.Init(
      target,
      options.ConsumerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }
}
