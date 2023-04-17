using Cila.Database;
using Cila.Documents;
using Microsoft.AspNetCore.Mvc;

namespace cil_aggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NftController : ControllerBase
{
    private readonly NftService nftService;

    public NftController(NftService nftService)
    {
        this.nftService = nftService;
    }

    [HttpGet]
    public IEnumerable<NFTDocument> GetAll()
    {
        return nftService.FindAllNfts();
    }

    [HttpGet("{id}")]
    public NFTDocument Get(string id)
    {
        return nftService.FindOneNft(id);
    }

    [HttpGet("/owner/{id}")]
    public IEnumerable<NFTDocument> GetByOwner(string id)
    {
        return nftService.FindAllNfts(id);
    }

}
