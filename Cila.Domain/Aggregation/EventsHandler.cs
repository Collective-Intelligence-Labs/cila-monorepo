
using System.Security.Cryptography;
using System.Text;
using Cila.Database;
using Cila.Documents;
using Google.Protobuf;
using MongoDB.Driver;

public class EventsHandler: IEventHandler
{
    private readonly MongoDatabase _database;

    public EventsHandler(MongoDatabase database)
    {
        _database = database;
    }

    public void Handle(NFTMintedPayload e){
        var nfts = _database.GetNfts();
        var id = GetId(e.Hash, e.Owner);
        try
        {
            
            nfts.InsertOne(new NFTDocument{
                Id = id,
                Hash = e.Hash.ToBase64(),
                Owner = e.Owner.ToBase64()
            });
        }
        catch (MongoWriteException ex)
        {
            Console.WriteLine("Trying to create the same NFT " + id);
        }
    }

    public void Handle(NFTTransferedPayload e)
    {
        var nfts = _database.GetNfts();
        var builder = Builders<NFTDocument>.Update.Set(x=> x.Owner,e.To.ToBase64());
        nfts.UpdateOne(x=> x.Id == GetId(e.Hash, e.From), builder);
    }

    private string GetId(ByteString hash, ByteString owner)
    {
        var inputBytes = hash.ToByteArray().Concat(owner.ToByteArray()).ToArray();
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}