using Cila.Documents;
using MongoDB.Driver;

namespace Cila.Database {

    public class MongoDatabase: IDatabase
    {
        private MongoClient _client;

        private class Databases {
            public static string Aggregator  = "aggregator";
            public static string Relay  = "relay";
        }

        private class Collections {
            public static string Events  = "events";
            public static string Subscriptions  = "subscriptions";
            public static string Chains  = "chains";
            public static string Executions  = "executions";
            public static string AggregatedEvents = "aggregated-events";
            public static string NFTs = "nfts";
            public static string Operations = "operaions";
        }

        public MongoDatabase(OmniChainSettings settings)
        {
            _client = new MongoClient(settings.MongoDBConnectionString);
        }

        public IMongoCollection<DomainEvent> GetEvents()
        {
            return _client.GetDatabase(Databases.Relay).GetCollection<DomainEvent>(Collections.Events);
        }

        public IMongoCollection<AggregatedEventDocument> GetAggregatedEventsCollection()
        {
            return _client.GetDatabase(Databases.Aggregator).GetCollection<AggregatedEventDocument>(Collections.AggregatedEvents);
        }

        public IMongoCollection<OperationDocument> GetOperations()
        {
            return _client.GetDatabase(Databases.Aggregator).GetCollection<OperationDocument>(Collections.Operations);
        }

        public IMongoCollection<NFTDocument> GetNfts()
        {
            return _client.GetDatabase(Databases.Aggregator).GetCollection<NFTDocument>(Collections.NFTs);
        }

        public IMongoCollection<ExecutionChainEvent> GetEventsCollection()
        {
            return _client.GetDatabase(Databases.Aggregator).GetCollection<ExecutionChainEvent>(Collections.Events);
        }

        public IMongoCollection<ChainDocument> GetChainsCollection()
        {
            return _client.GetDatabase(Databases.Aggregator).GetCollection<ChainDocument>(Collections.Chains);
        }

        public IMongoCollection<SubscriptionDocument> GetSubscriptionsCollection()
        {
            return _client.GetDatabase(Databases.Relay).GetCollection<SubscriptionDocument>(Collections.Subscriptions);
        }

        public IMongoCollection<ExecutionDocument> GetExecutionsCollection()
        {
            return _client.GetDatabase(Databases.Relay).GetCollection<ExecutionDocument>(Collections.Executions);
        }

        public IEnumerable <OperationDocument> FindAllOperations()
        {
            var filter = Builders<OperationDocument>.Filter.Empty;
            return GetOperations().Find(filter).ToList();
        }

        public OperationDocument FindOne(string operationId)
        {
            var filter = Builders<OperationDocument>.Filter.Eq(x=> x.Id, operationId);
            return GetOperations().Find(filter).FirstOrDefault();
        }

    }
}
