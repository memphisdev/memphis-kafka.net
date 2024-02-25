namespace Superstream.Models;

internal class RawMessageDescriptor(string fileName, string masterMessageName, byte[] descriptor)
{
  public string FileName { get; set; } = fileName;
  public string MasterMessageName { get; set; } = masterMessageName;
  public byte[] Descriptor { get; set; } = descriptor;
  public string Descriptor64 => Convert.ToBase64String(Descriptor);

  internal static bool IsValid(RawMessageDescriptor rawMessageDescriptor)
  {
    try
    {
      ProtoBufSerialization
        .Compile(
          rawMessageDescriptor.Descriptor,
          rawMessageDescriptor.FileName,
          rawMessageDescriptor.MasterMessageName
        )
        .GetAwaiter()
        .GetResult();
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }
}
