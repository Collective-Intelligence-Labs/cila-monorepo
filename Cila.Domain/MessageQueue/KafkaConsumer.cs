using Cila.Domain.MessageQueue;
using Cila.Serialization;
using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cila
{
    public class KafkaConsumer
    {
        private readonly IConsumer<string, byte[]> consumer;

        private readonly EventsDispatcher dispatcher;

        public KafkaConsumer(KafkaConfigProvider configProvider, EventsDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            consumer = new ConsumerBuilder<string, byte[]>(configProvider.GetConsumerConfig()).Build();
        }

        public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(()=> {
                consumer.Subscribe(topic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        if (consumeResult != null && consumeResult.Message != null && consumeResult.Message.Value != null)
                        {
                            dispatcher.Dispatch(OmniChainSerializer.DeserializeInfrastructureEvent(consumeResult.Message.Value));
                        }
                        //Console.WriteLine($"Consumed message '{consumeResult.Message.Value}' from topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}");
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Error occurred: {ex.Error.Reason}");
                    }
                }
            });
        }

        public void Dispose()
        {
            consumer?.Dispose();
        }
    }
}