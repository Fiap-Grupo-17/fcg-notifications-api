using FCG.NotificationsAPI.Persistencia.Mongo.Documents;
using MongoDB.Driver;

namespace FCG.NotificationsAPI.Persistencia.Mongo;

/// <summary>
/// Encapsula a conexão MongoDB e expõe a coleção <c>processed_events</c>, usada
/// exclusivamente para idempotência de consumers. Este serviço não possui read model
/// próprio em Mongo. Registrado como singleton.
/// </summary>
public class MongoContext
{
    public const string ProcessedEventsCollection = "processed_events";

    public MongoOptions Options { get; }
    public IMongoDatabase Database { get; }

    public MongoContext(MongoOptions options)
    {
        Options = options;

        var client = new MongoClient(options.ConnectionString);
        Database = client.GetDatabase(options.Database);
    }

    public IMongoCollection<ProcessedEventDocument> ProcessedEvents =>
        Database.GetCollection<ProcessedEventDocument>(ProcessedEventsCollection);
}
