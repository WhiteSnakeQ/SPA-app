using SPA_app.Events.Interface;

namespace SPA_app.Events.Publisher
{
    public sealed class EventPublisher : IEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Publish<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

            foreach (var handler in handlers)
                await handler.Handle(@event);
        }
    }
}
