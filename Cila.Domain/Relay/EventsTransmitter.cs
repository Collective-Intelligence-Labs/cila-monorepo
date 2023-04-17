using Cila.Serialization;

namespace Cila
{
    public class EventsTransmitter
    {
        private readonly SubscriptionsService _subscriptionsService;
        private readonly OmniChainSettings _settings;

        public EventsTransmitter(SubscriptionsService subscriptionsService, OmniChainSettings settings)
        {
            this._subscriptionsService = subscriptionsService;
            this._settings = settings;
        }

        public void Dispatch(string originChainId, string aggregateId, IEnumerable<ExecutionChainEvent> events, UInt32 startIndex)
        {
            var clients = GetSubscriptions(aggregateId, originChainId);
            foreach (var client in clients)
            {
                client.Push(aggregateId, startIndex, events.Select(x=> x.Serialized));
            }
        }

        private IEnumerable<IChainClient> GetSubscriptions(string aggregateId, string originChainId)
        {
            var chains = _subscriptionsService.GetAllExceptOrigin(aggregateId, originChainId);
            foreach( var chain in chains)
            {
                IChainClient chainClient = CreateChainClientInstance(chain.ChainId, GetChainClientType(chain.ChainId));
                yield return chainClient;
            }
        }

        private IChainClient CreateChainClientInstance(string chainId, Type type)
        {
            var chainSettings = _settings.Chains.Where(x=>x.ChainId == chainId).FirstOrDefault();
            if (chainSettings == null)
            {
                throw new ArgumentNullException("Chain settings might be set for the chain ID " + chainId);
            }
            return new EthChainClient(chainSettings.Rpc, chainSettings.Contract, chainSettings.PrivateKey);
        }

        private Type GetChainClientType(string chainId)
        {
            return typeof(EthChainClient);
        }
    }
}