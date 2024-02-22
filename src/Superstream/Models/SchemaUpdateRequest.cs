namespace Superstream;

#nullable disable

internal class SchemaUpdateRequest
{
  // [JsonPropertyName("master_msg_name")]
  [JsonPropertyName("MasterMsgName")]
  public string MasterMsgName { get; set; }

  // [JsonPropertyName("file_name")]
  [JsonPropertyName("FileName")]
  public string FileName { get; set; }

  // [JsonPropertyName("schema_id")]
  [JsonPropertyName("SchemaID")]
  public string SchemaId { get; set; }

  // [JsonPropertyName("desc")]
  [JsonPropertyName("Desc")]
  public byte[] Desc { get; set; }
}

#nullable restore
