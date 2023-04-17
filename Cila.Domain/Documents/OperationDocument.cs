namespace Cila.Documents
{
    public class OperationDocument
    {
        public string Id {get;set;}
        public string ClientID {get;set;}
        public string AggregateId {get;set;}
        public string RouterId {get;set;}
        public DateTime Created {get;set;}
        public List<string> Commands {get;set;}
        public List<string> Events {get;set;}
        public List<SyncItems> Routers {get;set;}
        public List<SyncItems> Chains {get;set;}
        public List<SyncItems> Relays {get;set;}
        public List<SyncItems> Aggregators {get;set;}
        public List<InfrastructureEventItem> InfrastructureEvents {get;set;}

        public OperationDocument()
        {
            InfrastructureEvents  = new List<InfrastructureEventItem>();
            Commands = new List<string>();
            Relays = new List<SyncItems>();
            Chains = new List<SyncItems>();
            Aggregators = new List<SyncItems>();
            Routers = new List<SyncItems>();
            Events = new List<string>();
        }
    }

    public class SyncItems
    {
        public string Id {get;set;}
        public string Name {get;set;}
        public bool OriginalSource {get;set;}
        public DateTime Timestamp {get;set;}
        public string ErrorMessage {get;set;}
    }

    public class InfrastructureEventItem {
        public DateTime Timestamp {get;set;}

        public InfrastructureEventType Type {get;set;}

        public string PortalId { get; set; }

        public string EventId {get;set;}

        public string RouterId {get;set;}

        public string RelayId {get;set;}

        public string CoreId {get;set;}

        public string AggreggatorId {get;set;}

        public string OperationId {get;set;}

        public List<string> DomainEvents {get;set;}

        public List<string> DomainCommands {get;set;}
    }

}