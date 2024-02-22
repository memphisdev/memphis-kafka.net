using NATS.Client;
using Superstream.Utils;

namespace Superstream.Core;

internal class SuperstreamManager
{
  internal static ConcurrentDictionary<int, SuperstreamClient> SuperstreamClients = [];
  internal static IConnection BrokerConnection = null!;
  internal static IJetStream JetStreamContext = null!;
  internal static string NatsConnectionId = null!;

  public static void InitializeNatsConnection(
    string token,
    ClientType clientType,
    string host = "broker.superstream.dev"
  )
  {
    var options = ConnectionFactory.GetDefaultOptions();
    options.Servers = [host];
    options.AllowReconnect = true;
    options.ReconnectWait = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
    options.Timeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
    options.MaxReconnect = Options.ReconnectForever;

    string[] tokenArray = token.Split(":::");
    if (tokenArray.Length != 2)
    {
      throw new ArgumentException("superstream: token is not valid");
    }
    string jwt = tokenArray[0];
    string nKey = tokenArray[1];

    options.SetJWTEventHandlers(
      (sender, args) => args.JWT = jwt,
      (sender, args) =>
      {
        var userNKey = Nkeys.FromSeed(nKey);
        args.SignedNonce = userNKey.Sign(args.ServerNonce);
      }
    );

    options.ReconnectedEventHandler += (sender, args) =>
    {
      string natsConnectionID = null;
      foreach (var client in SuperstreamClients.Values)
      {
        try
        {
          natsConnectionID = BrokerConnection.GenerateNatsConnectionId();
        }
        catch (Exception ex)
        {
          client.HandleError(
            $"{nameof(InitializeNatsConnection)} at GenerateNatsConnectionID: {ex.Message}"
          );
          return;
        }

        var clientReconnectionUpdateReq = new ClientReconnectionUpdateRequest
        {
          NewNatsConnectionId = natsConnectionID,
          ClientId = client.ClientId,
        };

        try
        {
          var payload = JsonSerializer.SerializeToUtf8Bytes(clientReconnectionUpdateReq);
          Request(Subjects.ClientReconnectionUpdateSubject, payload, 1);
        }
        catch (Exception ex)
        {
          client.HandleError($"InitializeNatsConnection at RequestAsync: {ex.Message}");
          return;
        }
      }

      NatsConnectionId = natsConnectionID;
    };

    DisableDefaultNatsEventHandlers(options);

    BrokerConnection = new ConnectionFactory().CreateConnection(options);
    JetStreamContext = BrokerConnection.CreateJetStreamContext();
  }

  static void DisableDefaultNatsEventHandlers(Options options)
  {
    options.ClosedEventHandler += (_, _) => { };
    options.ServerDiscoveredEventHandler += (_, _) => { };
    options.DisconnectedEventHandler += (_, _) => { };
    options.LameDuckModeEventHandler += (_, _) => { };
    options.AsyncErrorEventHandler += (_, _) => { };
    options.HeartbeatAlarmEventHandler += (_, _) => { };
    options.UnhandledStatusEventHandler += (_, _) => { };
    options.FlowControlProcessedEventHandler += (_, _) => { };
    options.PullStatusErrorEventHandler += (_, _) => { };
    options.PullStatusWarningEventHandler += (_, _) => { };
  }

  internal static Msg Request(string subject, byte[] message, int timeoutRetry)
  {
    try
    {
      int timeoutMilliSeconds = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
      return BrokerConnection.Request(subject, message, timeoutMilliSeconds);
    }
    catch (NATSTimeoutException)
    {
      if (timeoutRetry <= 0)
        throw;
      return Request(subject, message, timeoutRetry - 1);
    }
  }

  internal static SuperstreamClient InitSuperstream(string token, ProducerConfig producerConfig)
  {
    SuperstreamClients ??= new ConcurrentDictionary<int, SuperstreamClient>();

    var opts = SuperstreamOption.Default;
    var clientType = "kafka";
    ClientConfiguration conf = producerConfig;
    var newClient = new SuperstreamClient
    {
      Configuration = conf,
      ClientType = ClientType.Producer,
    };

    if (BrokerConnection == null)
    {
      try
      {
        InitializeNatsConnection(token, ClientType.Producer, opts.Host);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"superstream: {ex.Message}");
        return newClient;
      }
    }

    newClient.LearningFactor = opts.LearningFactor;
    newClient.Configuration.Servers = opts.Servers;
    newClient.Configuration.ConsumerGroupId = opts.ConsumerGroup;

    try
    {
      newClient.RegisterClient();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"superstream: {ex.Message}");
      return newClient;
    }

    SuperstreamClients[newClient.ClientId] = newClient;

    try
    {
      newClient.SubscribeUpdates();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"superstream: {ex.Message}");
      return newClient;
    }
    Task.Run(() => newClient.ReportClientsUpdate());
    return newClient;
  }

  internal static SuperstreamClient InitSuperstream(string token, ConsumerConfig producerConfig)
  {
    SuperstreamClients ??= new ConcurrentDictionary<int, SuperstreamClient>();

    var opts = SuperstreamOption.Default;
    var clientType = "kafka";
    ClientConfiguration conf = producerConfig;
    var newClient = new SuperstreamClient
    {
      Configuration = conf,
      ClientType = ClientType.Producer,
    };

    if (BrokerConnection == null)
    {
      try
      {
        InitializeNatsConnection(token, ClientType.Producer, opts.Host);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"superstream: {ex.Message}");
        return newClient;
      }
    }

    newClient.LearningFactor = opts.LearningFactor;
    newClient.Configuration.Servers = opts.Servers;
    newClient.Configuration.ConsumerGroupId = opts.ConsumerGroup;

    try
    {
      newClient.RegisterClient();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"superstream: {ex.Message}");
      return newClient;
    }

    SuperstreamClients[newClient.ClientId] = newClient;

    try
    {
      newClient.SubscribeUpdates();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"superstream: {ex.Message}");
      return newClient;
    }
    Task.Run(() => newClient.ReportClientsUpdate());
    return newClient;
  }
}
