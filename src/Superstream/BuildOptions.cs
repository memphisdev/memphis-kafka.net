namespace Superstream;

public class BuildOptions
{
  public string Token { get; set; } = null!;
  public string Host { get; set; } = null!;

  internal virtual void EnsureIsValid()
  {
    if (string.IsNullOrWhiteSpace(Token))
      throw new Exception("Token is required");
    if (string.IsNullOrWhiteSpace(Host))
      throw new Exception("Host is required");
  }
}

public sealed class ProducerBuildOptions : BuildOptions
{
  public ProducerConfig ProducerConfig { get; set; } = null!;

  internal override void EnsureIsValid()
  {
    base.EnsureIsValid();
    if (ProducerConfig is null)
      throw new Exception("ProducerConfig is required");
  }
}

public sealed class ConsumerBuildOptions : BuildOptions
{
  public ConsumerConfig ConsumerConfig { get; set; } = null!;

  internal override void EnsureIsValid()
  {
    base.EnsureIsValid();
    if (ConsumerConfig is null)
      throw new Exception("ConsumerConfig is required");
  }
}