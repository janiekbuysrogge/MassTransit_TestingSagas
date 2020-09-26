using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;

namespace MassTransitConsumerSaga.MassTransit
{
    public class ItemApprovedSaga : ISaga, InitiatedBy<ItemApproved>, Orchestrates<DocumentGenerated>, Orchestrates<MailSent>
    {
        public Guid CorrelationId { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? DocumentGeneratedDate { get; set; }

        public async Task Consume(ConsumeContext<ItemApproved> context)
        {
            Console.WriteLine($"ItemApprovedSaga {CorrelationId} Item Approved");

            ApprovedDate = DateTime.Now;

            await context.Publish<DocumentGenerated>(new { ItemId = context.Message.ItemId });
        }

        public async Task Consume(ConsumeContext<DocumentGenerated> context)
        {
            Console.WriteLine($"ItemApprovedSaga {CorrelationId} Document Generated");

            DocumentGeneratedDate = DateTime.Now;

            await context.Publish<MailSent>(new { ItemId = context.Message.ItemId });
        }

        public async Task Consume(ConsumeContext<MailSent> context)
        {
            Console.WriteLine($"ItemApprovedSaga {CorrelationId} Mail Sent");
        }
    }
}
