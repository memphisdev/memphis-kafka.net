namespace Superstream;

internal class SuperstreamOption
{
  private int _learningFactor;
  public string Host { get; set; } = null!;
  public int LearningFactor
  {
    get => _learningFactor;
    set
    {
      if (value >= 0 && value <= 10000)
      {
        _learningFactor = value;
        return;
      }

      throw new ArgumentOutOfRangeException(
        nameof(value),
        "Learning factor should be in range of 0 to 10000"
      );
    }
  }
  public string ConsumerGroup { get; set; } = null!;
  public string Servers { get; set; } = null!;

  public static SuperstreamOption Default => new() { Host = "" };
}
