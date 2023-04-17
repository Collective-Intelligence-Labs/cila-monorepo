using Cila.Database;
using Cila.Documents;
using Microsoft.AspNetCore.Mvc;

namespace cil_aggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationsController : ControllerBase
{
    private readonly MongoDatabase _db;

    public OperationsController(MongoDatabase db)
    {
        _db = db;
    }

    [HttpGet(Name = "GetAll")]
    public IEnumerable<OperationDocument> GetAl()
    {
        return _db.FindAllOperations();
    }

    [HttpGet("{id}")]
    public OperationDocument Get(string id)
    {
        return _db.FindOne(id);
    }
}

public class NftDto
{
    public string Hash { get; set; }
    
    public string Owner { get; set; }
}
