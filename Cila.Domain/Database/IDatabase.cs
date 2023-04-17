using Cila.Documents;
using MongoDB.Driver;

namespace Cila.Database
{
    public interface IDatabase
    {
        IMongoCollection<DomainEvent> GetEvents();
        IMongoCollection<OperationDocument> GetOperations();
    }
}