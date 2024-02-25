namespace Superstream.Core;

#nullable disable
internal class Update
{
  public string Type { get; set; }
  public byte[] Payload { get; set; }
}

#nullable restore

internal class SuperstreamUpdateSubscription(int clientId)
{
  public int ClientId = clientId;
  public ISubscription? Subscription;
  public BlockingCollection<Update> UpdateChannel = [];

  public void UpdatesHandler()
  {
    foreach (var msg in UpdateChannel.GetConsumingEnumerable())
    {
      switch (msg.Type)
      {
        case "LearnedSchema":
          if (SuperstreamClients.TryGetValue(ClientId, out var client))
          {
            try
            {
              var schemaUpdateReq =
                JsonSerializer.Deserialize<SchemaUpdateRequest>(msg.Payload)
                ?? throw new Exception("Failed to deserialize schema update request");
              RawMessageDescriptor rawMessageDescriptor =
                new(schemaUpdateReq.FileName, schemaUpdateReq.MasterMsgName, schemaUpdateReq.Desc);
              var isValidDesc = RawMessageDescriptor.IsValid(rawMessageDescriptor);
              if (isValidDesc)
              {
                client.ProducerProtoDescriptor = rawMessageDescriptor;
                client.ProducerSchemaId = schemaUpdateReq.SchemaId;
              }
              else
              {
                client.HandleError($"{nameof(UpdatesHandler)}: error compiling schema");
              }
            }
            catch (Exception ex)
            {
              client.HandleError($"{nameof(UpdatesHandler)}: {ex.Message}");
            }
          }
          break;
      }
    }
  }

  public void SubscriptionHandler(object? sender, MsgHandlerEventArgs e)
  {
    try
    {
      var update = JsonSerializer.Deserialize<Update>(e.Message.Data);
      if (update == null)
        return;

      UpdateChannel.Add(update);
    }
    catch (Exception ex)
    {
      if (SuperstreamClients.TryGetValue(ClientId, out var client))
      {
        client.HandleError($"{nameof(SubscriptionHandler)} : {ex.Message}");
      }
    }
  }
}
