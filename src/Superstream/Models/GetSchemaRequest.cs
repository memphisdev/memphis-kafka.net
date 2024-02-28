namespace Superstream.Models;

#nullable disable
internal class GetSchemaRequest
{
  [JsonPropertyName("schema_id")]
  public string SchemaId { get; set; }
}
#nullable restore
