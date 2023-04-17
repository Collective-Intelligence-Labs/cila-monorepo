using Cila;
using Google.Protobuf;

namespace Cila.Serialization
{
    public class OmniChainSerializer
    {

        public static DomainEvent DeserializeDomainEvent(byte[] data)
        {
            var msg = new DomainEvent();
            msg.MergeFrom(data);
            return msg;
        }

        public static InfrastructureEvent DeserializeInfrastructureEvent(byte[] data)
        {
            var msg = new InfrastructureEvent();
            msg.MergeFrom(data);
            return msg;
        }

        public static byte[] Serialize(DomainEvent e)
        {
            return e.ToByteArray();
        }

        public static void ValidateData(byte[] data)
        {
            if (data == null || data.Length < 2)
            {
                throw new ArgumentException("Invalid data");
            }
        }
        public static IMessage DeserializeEvent(DomainEvent e)
        {
            DomainEventType messageType = (DomainEventType)e.EvntType;
            byte[] messageBytes = e.EvntPayload.ToArray();
            IMessage message;

            switch (messageType)
            {
                case DomainEventType.NftMinted:
                    message = new NFTMintedPayload();
                    break;
                case DomainEventType.NftTransfered:
                    message = new NFTTransferedPayload();
                    break;
                default:
                    throw new ArgumentException("Invalid message type");
            }

            message.MergeFrom(messageBytes);
            return message;
        }
    }

    enum OmniChainMessageType
    {
        ItemIssued = 1,
        ItemTransfered = 2,
    }
}