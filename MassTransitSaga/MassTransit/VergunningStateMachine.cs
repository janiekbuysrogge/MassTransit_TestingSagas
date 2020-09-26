using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using MassTransit;

namespace MassTransitSaga.MassTransit
{
    public interface ApproveVergunning : CorrelatedBy<Guid>
    {
        Guid PermitId { get; set; }
    }

    public interface GenerateCommunication : CorrelatedBy<Guid>
    {
        Guid PermitId { get; set; }
    }

    public interface CommunicationGenerated : CorrelatedBy<Guid>
    {
        Guid PermitId { get; set; }
    }

    public interface CommunicationSent : CorrelatedBy<Guid>
    {
        Guid PermitId { get; set; }
    }

    public class VergunningStateMachine : MassTransitStateMachine<VergunningState>
    {
        public State Approved { get; private set; }

        public State Generating { get; private set; }

        public State Generated { get; private set; }

        public State Sending { get; private set; }

        public State Sent { get; private set; }

        public VergunningStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Initially(
                When(ApproveVergunning)
                    .PublishAsync(context => context.Init<GenerateCommunication>(new { }))                    
                    .TransitionTo(Approved));

            During(Approved,
                When(GenerateCommunication)
                    .Activity(x => x.OfType<GenerateCommunicationActivity>())
                    .TransitionTo(Generating));

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

        public Event<CommunicationSent> CommunicationSent { get; private set; }
    }

    public class VergunningState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }
    }

    public class GenerateCommunicationActivity : Activity<VergunningState, GenerateCommunication>
    {
        readonly ConsumeContext _context;

        public GenerateCommunicationActivity(ConsumeContext context)
        {
            _context = context;
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
            // do the activity thing

            // generate files on docgen

            await _context.Publish<CommunicationGenerated>(new { }).ConfigureAwait(false);

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

        public SendCommunicationActivity(ConsumeContext context)
        {
            _context = context;
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
            // do the activity thing

            // send documents with email service

            await _context.Publish<CommunicationSent>(new { }).ConfigureAwait(false);

            // call the next activity in the behavior
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<VergunningState, CommunicationGenerated, TException> context, Behavior<VergunningState, CommunicationGenerated> next)
            where TException : Exception
        {
            return next.Faulted(context);
        }
    }
}
