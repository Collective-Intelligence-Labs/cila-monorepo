using System;
using Confluent.Kafka;

namespace Cila.Domain.MessageQueue
{
	public class KafkaConfigProvider
	{
		public KafkaConfigProvider()
		{
		}

		public ConsumerConfig GetConsumerConfig()
		{
            var configConsumer = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                ClientId = "dotnet-kafka-consumer",
                GroupId = "test-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                MaxPollIntervalMs = 10000,
                EnablePartitionEof = true,
                SessionTimeoutMs = 6000,
                FetchWaitMaxMs = 1000,
                IsolationLevel = IsolationLevel.ReadCommitted,
                Acks = Acks.All
            };
            return configConsumer;
        }


        public ProducerConfig GetProducerConfig()
        {

            var configProducer = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
                ClientId = "dotnet-kafka-producer",
                Acks = Acks.All,
                MessageSendMaxRetries = 10,
                MessageTimeoutMs = 10000,
                EnableIdempotence = true,
                CompressionType = CompressionType.Snappy,
                BatchSize = 16384,
                LingerMs = 10,
                MaxInFlight = 5,
                EnableDeliveryReports = true,
                DeliveryReportFields = "all"
            };
            return configProducer;
        }
    }
}

