namespace Superstream;

internal static class SuperstreamInitializerExtensions
{
  internal static IProducer<TKey, TValue> BuildWithSuperstream<TKey, TValue>(
    this ProducerBuilder<TKey, TValue> builder,
    ProducerBuildOptions options
  )
  {
    options.EnsureIsValid();
    return ProducerInterceptor<TKey, TValue>.Init(
      builder.Build(),
      options.ProducerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }

  internal static IConsumer<TKey, TValue> BuildWithSuperstream<TKey, TValue>(
    this ConsumerBuilder<TKey, TValue> builder,
    ConsumerBuildOptions options
  )
  {
    options.EnsureIsValid();
    return ConsumerInterceptor<TKey, TValue>.Init(
      builder.Build(),
      options.ConsumerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }
}
