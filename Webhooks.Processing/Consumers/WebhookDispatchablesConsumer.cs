using MassTransit;
using MyJwtAuthServer.Contracts.Events;
using MyJwtAuthServer.Contracts.Events.BusinessEvents;
using System.Text.Json;

namespace Webhooks.Processing.Consumers
{
    public class WebhookDispatchablesConsumer(IPublishEndpoint publishEndpoint, ILogger<WebhookDispatchedConsumer> logger) : 
        IConsumer<ChangeEmailConfirmationSentEvent>,
        IConsumer<PasswordResetCodeConfirmationSentEvent>,
        IConsumer<PasswordResetLinkConfirmationSentEvent>,
        IConsumer<RegistrationEmailConfirmationSentEvent>,
        IConsumer<UserDeletedEvent>,
        IConsumer<UserEmailChangedEvent>,
        IConsumer<UserEmailConfirmedEvent>,
        IConsumer<UserLoggedInEvent>,
        IConsumer<UserLoggedOutEvent>,
        IConsumer<UserPasswordResetEvent>,
        IConsumer<UserRefreshTokenEvent>,
        IConsumer<UserRegisteredEvent>
    {
        public async Task Consume(ConsumeContext<ChangeEmailConfirmationSentEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<PasswordResetCodeConfirmationSentEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<PasswordResetLinkConfirmationSentEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<RegistrationEmailConfirmationSentEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserLoggedOutEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserEmailConfirmedEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserRefreshTokenEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserPasswordResetEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserLoggedInEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserEmailChangedEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<UserDeletedEvent> context)
        {
            await Dispatch(context.Message, context.CancellationToken);
        }

        private async Task Dispatch<T>(T message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Sending webhook messages: {typeof(T).FullName}");

            await publishEndpoint.Publish(new WebhookDispatchedEvent(nameof(T), JsonSerializer.Serialize(message), null), cancellationToken);
        }
        
    }
}
