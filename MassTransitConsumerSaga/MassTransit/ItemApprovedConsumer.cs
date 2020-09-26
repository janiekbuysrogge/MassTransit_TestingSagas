using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace MassTransitConsumerSaga.MassTransit
{
    public class ItemApprovedConsumer : IConsumer<ItemApproved>
    {
        public Task Consume(ConsumeContext<ItemApproved> context)
        {
            Console.WriteLine("hi there");

            return Task.CompletedTask;
        }
    }
}
