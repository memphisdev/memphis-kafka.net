namespace Superstream.Utils;

internal static class Subjects
{
  public const string ClientReconnectionUpdateSubject = "internal.clientReconnectionUpdate";
  public const string ClientTypeUpdateSubject = "internal.clientTypeUpdate";
  public const string ClientRegisterSubject = "internal.registerClient";
  public const string SuperstreamLearningSubject = "internal.schema.learnSchema.{0}";
  public const string SuperstreamRegisterSchemaSubject = "internal_tasks.schema.registerSchema.{0}";
  public const string SuperstreamClientUpdatesSubject = "internal.updates.{0}";
  public const string SuperstreamGetSchemaSubject = "internal.schema.getSchema.{0}";
  public const string SuperstreamErrorSubject = "internal.clientErrors";
  public const string SuperstreamClientsUpdateSubject = "internal_tasks.clientsUpdate.{0}.{1}";
  public const string SuperstreamInternalUsername = "superstream_internal";
}

internal static class SdkInfo
{
  public const string SdkVersion = "1.0.2";
  public const string SdkLanguage = "C#";
}
