using MongoDB.Bson;

namespace Cila.Documents
{
    public class AggregatedEventDocument
    {
        public string Id {get;set;}

        public string Hash {get;set;}

        public ulong Version {get;set;}

        public ulong BlockNumber {get;set;}

        public string BlockHash {get;set;}

        public string AggregateId {get;set;}

        public byte[] Payload {get;set;}

        public string ChainId {get;set;}

        public DateTime Timestamp {get;set;}

        public AggregatedEventDocument()
        {
            
        }

        public AggregatedEventDocument(AggregatedEvent e)
        {
            AggregateId = e.AggregateId;
            ChainId = e.ChainId;
            Payload = e.Payload;
            BlockHash = e.BlockHash;
            BlockNumber = e.BlockNumber;
            Version = e.Version;
            Timestamp = DateTime.UtcNow;
            Hash = e.Hash;
            Id = ObjectId.GenerateNewId().ToString();
        }
    }
}