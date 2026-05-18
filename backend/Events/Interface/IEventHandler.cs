namespace SPA_app.Events.Interface
{
        public interface IEventHandler<in TEvent> where TEvent : IEvent
        {
            Task Handle(TEvent @event);
        }
}
