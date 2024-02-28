using Superstream.Core;
using Superstream.Utils;

namespace Superstream;

internal class SuperstreamClient
{
  public int ClientId { get; set; }
  public string? AccountName { get; set; }
  public string NatsConnectionId
  {
    get => $"{BrokerConnection.ServerInfo.ServerName}:{BrokerConnection.ServerInfo.ClientId}";
  }

  public bool IsProducer => false;
  public bool IsConsumer => false;
  public ClientType ClientType { get; set; }
  public int LearningFactor { get; set; }
  public int LearningFactorCounter { get; set; }
  public bool LearningRequestSent { get; set; }
  public bool GetSchemaRequestSent { get; set; }
  public string? ProducerSchemaId { get; set; }
  public RawMessageDescriptor? ProducerProtoDescriptor { get; set; }
  public ClientCounters Counters { get; set; } = null!;
  public ClientConfiguration Configuration { get; set; } = null!;
  public ConcurrentDictionary<string, RawMessageDescriptor> ConsumerProtoDescriptors { get; set; } =
    new();

  public void RegisterClient()
  {
    var registerReq = new RegisterRequest
    {
      NatsConnectionId = NatsConnectionId,
      Language = SdkInfo.SdkLanguage,
      Version = SdkInfo.SdkVersion,
      LearningFactor = LearningFactor,
      Configuration = Configuration,
    };

    try
    {
      var registerReqBytes = JsonSerializer.SerializeToUtf8Bytes(registerReq);
      var resp =
        Request(Subjects.ClientRegisterSubject, registerReqBytes, 3)
        ?? throw new Exception("Failed to register client");
      var respStr = Encoding.UTF8.GetString(resp.Data);
      var registerResp =
        JsonSerializer.Deserialize<RegisterResponse>(respStr)
        ?? throw new Exception("Failed to deserialize register response");

      ClientId = registerResp.ClientId;
      AccountName = registerResp.AccountName;
      LearningFactor = registerResp.LearningFactor;
      LearningFactorCounter = 0;
      LearningRequestSent = false;
      GetSchemaRequestSent = false;

      Counters = new ClientCounters
      {
        TotalBytesBeforeReduction = 0,
        TotalBytesAfterReduction = 0,
        TotalMessagesSuccessfullyProduce = 0,
        TotalMessagesSuccessfullyConsumed = 0,
        TotalMessagesFailedProduce = 0,
        TotalMessagesFailedConsume = 0,
      };
    }
    catch (Exception ex)
    {
      throw new Exception($"superstream: error registering client:", ex);
    }
  }

  public void SubscribeUpdates()
  {
    var cus = new SuperstreamUpdateSubscription(ClientId);

    Task.Factory.StartNew(() => cus.UpdatesHandler(), TaskCreationOptions.LongRunning);

    try
    {
      cus.Subscription = BrokerConnection.SubscribeAsync(
        string.Format(Subjects.SuperstreamClientUpdatesSubject, ClientId),
        cus.SubscriptionHandler
      );
    }
    catch (Exception ex)
    {
      throw new Exception($"error connecting with superstream:", ex);
    }
  }

  public void SendLearningMessage(byte[] message)
  {
    try
    {
      JetStreamContext.Publish(
        string.Format(Subjects.SuperstreamLearningSubject, ClientId),
        message
      );
    }
    catch (Exception ex)
    {
      HandleError($"{nameof(SendLearningMessage)} at Publish: {ex.Message}");
    }
  }

  public void SendRegisterSchemaRequest()
  {
    if (LearningRequestSent)
      return;

    try
    {
      JetStreamContext.Publish(
        string.Format(Subjects.SuperstreamRegisterSchemaSubject, ClientId),
        Encoding.UTF8.GetBytes("")
      );
      LearningRequestSent = true;
    }
    catch (Exception ex)
    {
      HandleError($"{nameof(SendRegisterSchemaRequest)} at Publish: {ex.Message}");
    }
  }

  internal void SendGetSchemaRequest(string schemaId)
  {
    GetSchemaRequestSent = true;
    GetSchemaRequest req = new() { SchemaId = schemaId };

    try
    {
      var reqBytes = JsonSerializer.SerializeToUtf8Bytes(req);
      var msg = Request(string.Format(Subjects.SuperstreamGetSchemaSubject, ClientId), reqBytes, 3);

      var resp =
        JsonSerializer.Deserialize<SchemaUpdateRequest>(msg.Data)
        ?? throw new Exception("Failed to deserialize schema update request");
      RawMessageDescriptor rawMessageDescriptor = new(resp.FileName, resp.MasterMsgName, resp.Desc);
      var isValidDescriptor = RawMessageDescriptor.IsValid(rawMessageDescriptor);
      if (isValidDescriptor)
      {
        ConsumerProtoDescriptors.AddOrUpdate(
          schemaId,
          rawMessageDescriptor,
          (key, oldValue) => rawMessageDescriptor
        );
      }
      else
      {
        HandleError($"{nameof(SendGetSchemaRequest)}: error compiling schema");
        GetSchemaRequestSent = false;
        throw new Exception("superstream: error compiling schema");
      }
    }
    catch (Exception ex)
    {
      HandleError($"{nameof(SendGetSchemaRequest)}: {ex.Message}");
      GetSchemaRequestSent = false;
      throw;
    }
  }

  internal void HandleError(string message)
  {
    var errorMessage =
      $"[account name: {AccountName}][clientID: {ClientId}][sdk: C#][version: {SdkInfo.SdkVersion}]{message}";
    BrokerConnection.SendClientErrorToBackend(errorMessage);
  }

  public void SendClientTypeUpdateRequest(string clientType)
  {
    ClientType = clientType switch
    {
      "consumer" => ClientType.Consumer,
      "producer" => ClientType.Producer,
      _ => throw new Exception("Invalid client type")
    };

    var request = new ClientTypeUpdateRequest { ClientId = ClientId, Type = clientType, };

    try
    {
      var clientTypeUpdateReqBytes = JsonSerializer.SerializeToUtf8Bytes(request);
      BrokerConnection.Publish(Subjects.ClientTypeUpdateSubject, clientTypeUpdateReqBytes);
    }
    catch (Exception ex)
    {
      HandleError($"{nameof(SendClientTypeUpdateRequest)} at publish: {ex.Message}");
    }
  }

  internal void ReportClientsUpdate()
  {
    var timer = new System.Timers.Timer(30 * 1000);
    timer.Elapsed += (_, _) => HandleReportClientsUpdate();
    timer.Start();
  }

  internal void HandleReportClientsUpdate()
  {
    try
    {
      var byteCounters = JsonSerializer.SerializeToUtf8Bytes(Counters);

      if (Configuration.ConsumerTopicsPartitions == null)
      {
        Configuration.ConsumerTopicsPartitions = [];
      }

      var topicPartitionConfig = new TopicsPartitionsPerProducerConsumer
      {
        ProducerTopicsPartitions = Configuration.ProducerTopicsPartitions,
        ConsumerTopicsPartitions = Configuration.ConsumerTopicsPartitions,
      };

      var byteConfig = JsonSerializer.SerializeToUtf8Bytes(topicPartitionConfig);

      BrokerConnection.Publish(
        string.Format(Subjects.SuperstreamClientsUpdateSubject, "counters", ClientId),
        byteCounters
      );
      BrokerConnection.Publish(
        string.Format(Subjects.SuperstreamClientsUpdateSubject, "config", ClientId),
        byteConfig
      );
    }
    catch (Exception ex)
    {
      HandleError($"{nameof(HandleReportClientsUpdate)}: {ex.Message}");
    }
  }
}
