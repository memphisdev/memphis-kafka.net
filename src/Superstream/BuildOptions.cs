namespace Superstream;

public class BuildOptions
{
  internal string Token { get; set; } = null!;
  public string Host { get; internal set; } = null!;
  public int LearningFactor { get; set; } = 0;
  public string ConsumerGroup { get; set; } = string.Empty;

  internal virtual void EnsureIsValid()
  {
    if (string.IsNullOrWhiteSpace(Token))
      throw new Exception("Token is required");
    if (string.IsNullOrWhiteSpace(Host))
      throw new Exception("Host is required");
  }
}

public sealed class ProducerBuildOptions(ProducerConfig producerConfig) : BuildOptions
{
  public ProducerConfig ProducerConfig { get; set; } = producerConfig;

  internal override void EnsureIsValid()
  {
    base.EnsureIsValid();
    if (ProducerConfig is null)
      throw new Exception("ProducerConfig is required");
  }
}

public sealed class ConsumerBuildOptions(ConsumerConfig consumerConfig) : BuildOptions
{
  public ConsumerConfig ConsumerConfig { get; set; } = consumerConfig;

  internal override void EnsureIsValid()
  {
    base.EnsureIsValid();
    if (ConsumerConfig is null)
      throw new Exception("ConsumerConfig is required");
  }
}
