using Google.Protobuf.WellKnownTypes;
using MongoDB.Bson;
using Nethereum.Contracts;
using Nethereum.Util;
using Nethereum.Util.HashProviders;
using Cila.Serialization;

namespace Cila
{
    public interface IExecutionChain
    {
        string ID {get;}
        void Update();
    }

    public class ExecutionChainEvent
    {
        public string Id {get;set;}

        public string OriginChainId {get; set;}

        public string AggregateId {get;set;}
        
        public byte[] Serialized {get;set;}

        public byte[] Hash {get;set;}

        public int BlockNumber {get;set;}

        public ulong Version {get;set;}
    }

    public class ExecutionChain : IExecutionChain
    {
        public string ID { get; set; }
        internal IChainClient ChainService { get => chainService; set => chainService = value; }
        private IChainClient chainService;
        private string _singletonAggregateID;
        private readonly EventStore _eventStore;
        private readonly EventsTransmitter _eventsTransmitter;

        private readonly KafkaProducer _producer;

        public ExecutionChain(string singletonAggregateID, EventStore eventStore, EventsTransmitter eventsTransmiiter, KafkaProducer producer)
        {
            this._singletonAggregateID = singletonAggregateID;
            _eventStore = eventStore;
            _eventsTransmitter = eventsTransmiiter;
            _producer = producer;
        }

        public void Update()
        {
            var hashProvider = new Sha3KeccackHashProvider();
            //TODO: replace with actual last block pulling for a specific chain
            var lastBlock = _eventStore.GetLatestVersion(_singletonAggregateID) ?? 0;
            //TODO: Replace with pulling new events for all aggregate after specific block
            var newEvents = ChainService.Pull(lastBlock, _singletonAggregateID);
            newEvents = newEvents ?? new List<byte[]>();
            var aggregates = newEvents.Select(x => 
            { 
                var domainEvent = OmniChainSerializer.DeserializeDomainEvent(x);
                return new ExecutionChainEvent(){
                    Id = ObjectId.GenerateNewId().ToString(),
                Serialized = x,
                AggregateId = _singletonAggregateID,
                OriginChainId = ID,
                Hash = hashProvider.ComputeHash(domainEvent.EvntPayload.ToByteArray()), //replace with retrieving it from the chain and validating
                Version = domainEvent.EvntIdx
                };
            }).OrderBy(x => x.Version).GroupBy(x=> x.AggregateId);

            foreach (var aggregate in aggregates){
                var newVersion = aggregate.Max(x=> x.Version);
                var currentVersion = _eventStore.GetLatestVersion(aggregate.Key);
                //TODO: Add conflic resolution logic here: we need to add getting also a hash of latest version merkle tree of all events and then checking if there are a different with the once we receive from the chain because we might push additional events that different by hash not by version
                if (currentVersion == null || currentVersion < newVersion)
                {
                    var startIndex = aggregate.Min(x=> x.Version);
                    // selects new events if current Version null then all events
                    var events = currentVersion == null ? aggregate : aggregate.Where(x=> x.Version > currentVersion);
                    try {
                        _eventsTransmitter.Dispatch(ID, aggregate.Key , events, (UInt32)startIndex);
                        _eventStore.AppendEvents(aggregate.Key, events);
                        ProduceInfrastructureEvent(events, aggregate.Key, null);
                    } catch (SmartContractCustomErrorRevertException e)
                    {
                        ProduceInfrastructureEvent(events, aggregate.Key, e.Message);
                    }
                }
            }
        }

        private async Task ProduceInfrastructureEvent(IEnumerable<ExecutionChainEvent> events, string aggregateId, string errorMessage)
        {
            var operationId = aggregateId + events.First().Version;
             var infEvent = new InfrastructureEvent{
                        Id = Guid.NewGuid().ToString(),
                        EvntType = InfrastructureEventType.RelayEventsTransmiitedEvent,
                        AggregatorId = aggregateId,
                        OperationId = operationId ,
                        RelayId = "#1",
                        ErrorMessage = errorMessage //TODO: replace with normal error handling
                    };
                    foreach (var e in events)
                    {
                        infEvent.Events.Add( new DomainEventDto{
                                Id = e.Version.ToString(),
                                Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                        });
                    }
                    await _producer.ProduceAsync("infr", infEvent);
        }
    }
}