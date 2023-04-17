

namespace Cila
{
    public class ChainClientsFactory 
    {
        private readonly ChainsService chainsService;

        public ChainClientsFactory(ChainsService chainsService)
        {
            this.chainsService = chainsService;
        }

        public IChainClient GetChainClient(string chainId)
        {
            var chain = chainsService.Get(chainId);
            return (GetChainClient(chain));
        }

        public IChainClient GetChainClient(ChainDocument chain)
        {
            return new EthChainClient(chain.RPC,chain.CQRSContract, chain.PrivateKey);
        }
    }
}