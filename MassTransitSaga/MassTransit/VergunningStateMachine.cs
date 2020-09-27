using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

namespace MassTransitSaga.MassTransit
{
    public interface ApproveVergunning
    {
        Guid VergunningId { get; set; }
    }

    public interface GenerateCommunication
    {
        Guid VergunningId { get; set; }
    }

    public interface CommunicationGenerated
    {
        Guid VergunningId { get; set; }
    }

    public interface GenerateCommunicationFaulted
    {
        Guid VergunningId { get; set; }
    }

    public interface CommunicationSent
    {
        Guid VergunningId { get; set; }
    }

    public class VergunningStateMachine : MassTransitStateMachine<VergunningState>
    {
        public State Approved { get; private set; }

        public State Generating { get; private set; }

        public State Generated { get; private set; }

        public State Sending { get; private set; }

        public State Sent { get; private set; }

        public State GenerationFaulted { get; private set; }

        public VergunningStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => ApproveVergunning, x => x.CorrelateById(context => context.Message.VergunningId));
            Event(() => GenerateCommunication, x => x.CorrelateById(context => context.Message.VergunningId));
            Event(() => CommunicationGenerated, x => x.CorrelateById(context => context.Message.VergunningId));
            Event(() => GenerateCommunicationFaulted, x => x.CorrelateById(context => context.Message.VergunningId));
            Event(() => CommunicationSent, x => x.CorrelateById(context => context.Message.VergunningId));

            Initially(
                When(ApproveVergunning)
                    .Then(x => x.Instance.ApprovalDate = DateTime.UtcNow)
                    .PublishAsync(context => context.Init<GenerateCommunication>(new { VergunningId = context.Instance.CorrelationId }))                    
                    .TransitionTo(Approved));

            During(Approved,
                When(GenerateCommunication)
                    .Activity(x => x.OfType<GenerateCommunicationActivity>())
                    .TransitionTo(Generating)
                    .Catch<Exception>(ex => ex.PublishAsync(context => 
                        context.Init<GenerateCommunicationFaulted>(new { VergunningId = context.Instance.CorrelationId }))));

            During(Approved,
                When(GenerateCommunicationFaulted)
                    .TransitionTo(GenerationFaulted));

            During(Generating,
                When(CommunicationGenerated)
                    .Activity(x => x.OfType<SendCommunicationActivity>())
                    .TransitionTo(Sending));

            During(Sending,
                When(CommunicationSent)
                    .TransitionTo(Sent));
        }

        public Event<ApproveVergunning> ApproveVergunning { get; private set; }

        public Event<GenerateCommunication> GenerateCommunication { get; private set; }

        public Event<CommunicationGenerated> CommunicationGenerated { get; private set; }

        public Event<GenerateCommunicationFaulted> GenerateCommunicationFaulted { get; private set; }

        public Event<CommunicationSent> CommunicationSent { get; private set; }
    }

    public class VergunningState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? GenerationDate { get; set; }

        public DateTime? SendDate { get; set; }

        public int? EmailId { get; set; }
    }

    public class GenerateCommunicationActivity : Activity<VergunningState, GenerateCommunication>
    {
        readonly ConsumeContext _context;
        readonly ILogger<GenerateCommunicationActivity> _logger;

        public GenerateCommunicationActivity(ConsumeContext context, ILogger<GenerateCommunicationActivity> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("publish-vergunning-generate");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<VergunningState, GenerateCommunication> context, Behavior<VergunningState, GenerateCommunication> next)
        {
            _logger.LogInformation($"Thread Id: {Thread.CurrentThread.ManagedThreadId} {this.GetType().Name} Info: Generating comms for {context.Instance.CorrelationId}");

            // do the activity thing
            context.Instance.GenerationDate = DateTime.UtcNow;

            // generate files on docgen
            try
            {
                throw new Exception("generation failed!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Thread Id: {Thread.CurrentThread.ManagedThreadId} {this.GetType().Name} Info: Generation comms for {context.Instance.CorrelationId}", ex);
                throw;
            }

            await Task.Delay(10000);

            await _context.Publish<CommunicationGenerated>(new { VergunningId = context.Instance.CorrelationId }).ConfigureAwait(false);

            // call the next activity in the behavior
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<VergunningState, GenerateCommunication, TException> context, Behavior<VergunningState, GenerateCommunication> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }

    public class SendCommunicationActivity : Activity<VergunningState, CommunicationGenerated>
    {
        readonly ConsumeContext _context;
        readonly ILogger<SendCommunicationActivity> _logger;

        public SendCommunicationActivity(ConsumeContext context, ILogger<SendCommunicationActivity> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("publish-vergunning-send");
        }

        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<VergunningState, CommunicationGenerated> context, Behavior<VergunningState, CommunicationGenerated> next)
        {
            _logger.LogInformation($"Thread Id: {Thread.CurrentThread.ManagedThreadId} {this.GetType().Name} Info: Sending comms for {context.Instance.CorrelationId}");

            // do the activity thing
            context.Instance.SendDate = DateTime.UtcNow;

            // send documents with email service
            await Task.Delay(5000);
            context.Instance.EmailId = (new Random()).Next(1000, 9999);

            await _context.Publish<CommunicationSent>(new { VergunningId = context.Instance.CorrelationId }).ConfigureAwait(false);

            // call the next activity in the behavior
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<VergunningState, CommunicationGenerated, TException> context, Behavior<VergunningState, CommunicationGenerated> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }

    public class VergunningStateMap : SagaClassMap<VergunningState>
    {
        protected override void Configure(EntityTypeBuilder<VergunningState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.ApprovalDate);
            entity.Property(x => x.GenerationDate);
            entity.Property(x => x.SendDate);
            entity.Property(x => x.EmailId);

            // If using Optimistic concurrency, otherwise remove this property
            //entity.Property(x => x.RowVersion).IsRowVersion();
        }
    }

    public class VergunningStateDbContext : SagaDbContext
    {
        public VergunningStateDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new VergunningStateMap(); }
        }
    }
}
