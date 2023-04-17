using Cila.Serialization;
using Google.Protobuf;
using Cila.Documents;
using Google.Protobuf.WellKnownTypes;
using MongoDB.Bson;

namespace Cila
{
    public class AggregatorService
    {
        public string Id { get; private set; }
        private readonly OmniChainSettings config;
        private EventsDispatcher _dispatcher;
        private readonly ChainClientsFactory chainClientsFactory;
        private KafkaProducer _producer;
        private readonly ChainsService chainsService;
        private readonly AggregagtedEventsService aggregagtedEventsService;

        public AggregatorService(OmniChainSettings config, EventsDispatcher dispatcher, 
        ChainClientsFactory chainClientsFactory, KafkaProducer producer, ChainsService chainsService, 
        AggregagtedEventsService aggregagtedEventsService)
        {
            this.config = config;
            _dispatcher = dispatcher;
            this.chainClientsFactory = chainClientsFactory;
            _producer = producer;
            this.chainsService = chainsService;
            this.aggregagtedEventsService = aggregagtedEventsService;
            Id = config.AggregatorId;
        }

        public async Task Aggregate()
        {
            var chains = chainsService.GetAll();
            //fetch the latest state for each chains
            Console.WriteLine("Current active chains: {0}", chains.Count);

            await Parallel.ForEachAsync(chains, async (chain, cancellationToken) =>
            {
                //var current = chain.LastSyncedBlock;
                var current = aggregagtedEventsService.GetLastVersion(config.SingletonAggregateID);
                var next = current != null ? current.Value + 1 : 0;
                var client = chainClientsFactory.GetChainClient(chain);
                // Should be replace by pulling from block number
                var newEvents =  await client.PullAsync(next, config.SingletonAggregateID);
                var aggregatedEvents = newEvents.Select(x=> {
                    var domainEvent = OmniChainSerializer.DeserializeDomainEvent(x);
                    return new AggregatedEvent{
                    DomainEvent = domainEvent,
                    Payload = x,
                    AggregateId = config.SingletonAggregateID, // replace with real aggregate ID
                    ChainId = chain.Id,
                    OperaionId = config.SingletonAggregateID + domainEvent.EvntIdx,
                    CommandId = config.SingletonAggregateID + domainEvent.EvntIdx,
                    BlockNumber = domainEvent.EvntIdx, //replace with block number,
                    BlockHash = null // should be repalced with real one
                     }; 
                });
                foreach (var e in aggregatedEvents)
                {
                    //Find if the event with this aggregagte ID and this number has been already preocessed by aggregagtor, 
                    // maybe include hash to check if there was a conflicing event, so if there is a conflicting event, than 
                    // we need to decide if we replace it with new one or not, it should be somehow provided in event metadata from the chain
                    // so we know that this event has been authorized by relay as an actual one, maybe even by relay timestamp

                    var existingEvents = aggregagtedEventsService.GetEvents(e.AggregateId, e.Version, e.Hash);
                    var conflict = existingEvents.Any();
                    if (!conflict)
                    {
                        _dispatcher.DispatchEvent(e.DomainEvent);
                    }
                    if (!existingEvents.Any(x=> x.ChainId == chain.Id))
                    {
                        aggregagtedEventsService.AddEvent(new AggregatedEventDocument(e));
                        var infEvent = new InfrastructureEvent{
                            Id = ObjectId.GenerateNewId().ToString(),
                            EvntType = InfrastructureEventType.EventsAggregatedEvent,
                            AggregatorId = Id,
                            OperationId = e.OperaionId,
                            ChainId = chain.Id
                        };
                        infEvent.Events.Add( new DomainEventDto{
                                Id = e.Hash,
                                Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                                AggregateId = e.AggregateId,
                                CommandId = e.CommandId,
                                SourceId = chain.Id,
                                Conflict = conflict
                        });
                        await _producer.ProduceAsync("infr", infEvent);
                        // mock Execution chain event here
                        infEvent.EvntType = InfrastructureEventType.TransactionExecutedEvent;
                        await _producer.ProduceAsync("infr", infEvent);
                    }
                }
            });
            // find new events and dispatch them to events dispatcher
        }
    }
}