namespace Superstream;

#nullable disable

internal class SchemaUpdateRequest
{
  [JsonPropertyName("master_msg_name")]
  public string MasterMsgName { get; set; }

  [JsonPropertyName("file_name")]
  public string FileName { get; set; }

  [JsonPropertyName("schema_id")]
  public string SchemaId { get; set; }

  [JsonPropertyName("desc")]
  public byte[] Desc { get; set; }
}

#nullable restore
