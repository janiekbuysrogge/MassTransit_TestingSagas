## POC MassTransit Saga's

- [x] StateMachine saga's with custom activities
- [x] Using custom id as correlation id
- [x] Persisted in SQL database
- [ ] Failure / Retry / Compensation logic
- [ ] Resume (durable) when consumer goes down during consumption


### Usable for communication scenario

Long running in the case of multiple external services where we are the orchestrator.
Each one of those services can fail.

### Usable for payment scenario

Long running in the case of a user moving to the payment gateway, and returning later on with a failed or succeeded payment.