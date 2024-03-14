namespace Superstream;

public static class SuperstreamInitializer
{
  public static IProducer<K, V> Init<K, V>(
    string token,
    string host,
    IProducer<K, V> target,
    ProducerBuildOptions options
  )
  {
    options.Token = token;
    options.Host = host;
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
    string token,
    string host,
    IConsumer<K, V> target,
    ConsumerBuildOptions options
  )
  {
    options.Token = token;
    options.Host = host;
    return ConsumerInterceptor<K, V>.Init(
      target,
      options.ConsumerConfig,
      options.Token,
      options.Host,
      options.LearningFactor
    );
  }
}
