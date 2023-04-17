using Cila.Domain.MessageQueue;

namespace Cila
{

    public class RelayService
    {
        public string Id { get; private set; }
        
        private List<IExecutionChain> _chains;
        public RelayService(OmniChainSettings config)
        {
            _chains = new List<IExecutionChain>();
            Id = config.RelayId;
            var random = new Random();
            var kafkaConfigProvider = new KafkaConfigProvider();
            var db = new Database.MongoDatabase(config);
            var subsService = new SubscriptionsService(db);
            var kafkraProducer = new KafkaProducer(kafkaConfigProvider.GetProducerConfig());
            var subs = subsService.GetAllFor(config.SingletonAggregateID).ToList();
            foreach (var item in config.Chains)
            {
                if (subs.Count == 0)
                {
                    subsService.Create(config.SingletonAggregateID, item.ChainId);
                }
                var chain1 = new ExecutionChain(config.SingletonAggregateID, new EventStore(db), new EventsTransmitter(subsService, config), kafkraProducer);
                chain1.ID = item.ChainId;
                chain1.ChainService = new EthChainClient(item.Rpc, item.Contract, item.PrivateKey);
                _chains.Add(chain1);
            }
        }

        public void SyncAllChains()
        {
            //fetch the latest state for each chains
            Console.WriteLine("Current active chains: {0}", _chains.Count);
            foreach (var chain in _chains)
            {
                chain.Update();
            }
        }
    }
}