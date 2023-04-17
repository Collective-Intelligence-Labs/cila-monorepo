
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.JsonRpc.Client;
using Nethereum.Hex.HexTypes;
using System.Text;
using System.ComponentModel;
using Nethereum.ABI;
using Nethereum.Contracts.QueryHandlers;
using Google.Protobuf;

namespace Cila
{
    public class LoggingInterceptor : RequestInterceptor
    {
        public override Task InterceptSendRequestAsync(Func<string, string, object[], Task> interceptedSendRequestAsync, string method, string route = null, params object[] paramList)
        {
            Console.WriteLine($"Method: {method}");
            Console.WriteLine($"Params: {string.Join(", ", paramList)}");
            return interceptedSendRequestAsync(method,route,paramList);
        }
    }

    [Function("pull")]
    public class PullFuncation: FunctionMessage
    {
        [Parameter("address", "aggregateId", 1)]
        public string AggregateId {get;set;}

        [Parameter("uint", "startIndex", 2)]
        public int StartIndex {get;set;}

        [Parameter("uint", "limit", 3)]
        public int Limit {get;set;}
    }

    [Function("pullBytes")]
    public class PullBytesFunction: FunctionMessage
    {
        [Parameter("string", "aggregateId", 1)]
        public string AggregateId {get;set;}

        [Parameter("uint", "startIndex", 2)]
        public int StartIndex {get;set;}

        [Parameter("uint", "limit", 3)]
        public int Limit {get;set;}
    }


    [Function("pushBytes")]
    public class PushBytesFunction : FunctionMessage
    {
        [Parameter("string", "aggregateId", 1)]
        public string AggregateId { get; set; }

        [Parameter("uint", "startIndex", 2)]
        public UInt32 Position { get; set; }

        [Parameter("bytes[]", "evnts", 3)]
        public List<byte[]> Events { get; set; }
    }

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        private ContractHandler _handler;
        private Account _account;
        private Event<OmnichainEvent> _eventHandler;
        private NewFilterInput _filterInput;
        private string _privateKey;

        public EthChainClient(string rpc, string contract, string privateKey)
        {
            _privateKey = privateKey;
            _account = new Nethereum.Web3.Accounts.Account(privateKey);
            _web3 = new Web3(_account, rpc);
            _handler = _web3.Eth.GetContractHandler(contract);
        }

        public const int MAX_LIMIT = 1000000;

        public async Task<List<byte[]>> PullAsync(ulong position, string aggregateId)
        {
             Console.WriteLine("Chain Service Pull execution started from position: {0}, aggregate: {1}", position, aggregateId);
             var handler = _handler.GetFunction<PullBytesFunction>();
             var request = new PullBytesFunction{
                StartIndex = (int)position,
                    Limit = MAX_LIMIT,
                    AggregateId = aggregateId
                };
                var result =  await handler.CallAsync<PullEventsDTO>(request);
                Console.WriteLine("Chain Service Pull executed: {0}", result);
                //return result.Events;   
                return result.Events;
        }

        public List<byte[]> Pull(ulong position, string aggregateId)
        {
            return PullAsync(position, aggregateId).GetAwaiter().GetResult();
        }

        public async Task<ChainResponse> SendAsync(Operation op)
        {
            if (op == null)
                await Task.FromResult(true);

            var function = _handler.GetFunction<DispatchFunction>();

            var abi = new ABIEncode();

            var opBytes = op.ToByteArray();
            var req = new DispatchFunction
            {
                OpBytes = opBytes
            };
            req.FromAddress = _account.Address;
            var _queryHandler = _web3.Eth.GetContractQueryHandler<DispatchFunction>();
            var txHandler = _web3.Eth.GetContractTransactionHandler<DispatchFunction>();
            var gasEstimate = await txHandler.EstimateGasAsync(_handler.ContractAddress, req);
            req.Gas = gasEstimate.Value;

            var receipt = await txHandler.SendRequestAndWaitForReceiptAsync(_handler.ContractAddress, req);
            return new ChainResponse {
                ContractAddress = receipt.ContractAddress,
                EffectiveGasPrice = receipt.EffectiveGasPrice.ToUlong(),
                GasUsed = receipt.GasUsed.ToUlong(),
                CumulativeGasUsed = receipt.CumulativeGasUsed.ToUlong(),
                BlockHash = receipt.BlockHash,
                BlockNumber = receipt.BlockNumber.ToUlong(),
                Logs = receipt.Logs.ToString()
            };
        }

        public async Task<string> PushAsync(string aggregateId, UInt32 position, IEnumerable<byte[]> events)
        {
            var handler = _handler.GetFunction<PushBytesFunction>();
            var request = new PushBytesFunction{
                Events = events.ToList(),
                Position = position,
                AggregateId = aggregateId
            };
            foreach (var ev in request.Events){
                Console.WriteLine("Event: " + Convert.ToHexString(ev));
            }
            var result = await handler.CallAsync<string>(request, _account.Address, new HexBigInteger(210000), new HexBigInteger(0));
            Console.WriteLine("Chain Service Push} executed: {0}", result);
            return result;
        }
        public void Push(string aggregateId, UInt32 position, IEnumerable<byte[]> events)
        {
            PushAsync(aggregateId, position, events).GetAwaiter().GetResult();
        }
    }

    [FunctionOutput]
    public class PullEventsDTO: IFunctionOutputDTO
    {
        [Parameter("bytes[]",order:1)]
        public List<byte[]> Events {get;set;}
    }

    public class ChainResponse
    {
        public string ContractAddress { get; set; }
        public ulong EffectiveGasPrice { get; set; }
        public ulong GasUsed { get; set; }
        public ulong CumulativeGasUsed { get; set; }
        public ulong BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public ulong TransactionIndex { get; set; }
        public string TransactionHash { get; set; }
        public string Logs { get; set; }
    }
}