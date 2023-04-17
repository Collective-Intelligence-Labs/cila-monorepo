namespace Cila
{
    public interface IChainClient
    {
        Task<ChainResponse> SendAsync(Operation op);
        Task<List<byte[]>> PullAsync(ulong position, string aggregateId);
        List<byte[]> Pull(ulong position, string aggregateId);

        void Push(string aggregateId, UInt32 position, IEnumerable<byte[]> events);
        Task<string> PushAsync(string aggregateId, UInt32 position, IEnumerable<byte[]> events);
    }
}