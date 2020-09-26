using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace MassTransitConsumerSaga.MassTransit
{
    public interface ItemApproved : CorrelatedBy<Guid>
    {
        int ItemId { get; set; }
    }

    public interface DocumentGenerated : CorrelatedBy<Guid>
    {
        int ItemId { get; set; }
    }

    public interface MailSent : CorrelatedBy<Guid>
    {
        int ItemId { get; set; }
    }
}
