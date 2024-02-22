namespace Superstream;

public static class SuperstreamInitializerExtensions
{
  public static IProducer<TKey, TValue> BuildWithSuperstream<TKey, TValue>(
    this ProducerBuilder<TKey, TValue> builder,
    ProducerBuildOptions options
  )
  {
    options.EnsureIsValid();
    return ProducerInterceptor<TKey, TValue>.Init(
      builder.Build(),
      options.ProducerConfig,
      options.Token,
      options.Host
    );
  }

  public static IConsumer<TKey, TValue> BuildWithSuperstream<TKey, TValue>(
    this ConsumerBuilder<TKey, TValue> builder,
    ConsumerBuildOptions options
  )
  {
    options.EnsureIsValid();
    return ConsumerInterceptor<TKey, TValue>.Init(
      builder.Build(),
      options.ConsumerConfig,
      options.Token,
      options.Host
    );
  }
}
