namespace SPA_app.Events.Interface
{
    public interface IEventPublisher
    {
        Task Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}
