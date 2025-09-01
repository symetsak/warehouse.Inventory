namespace BlazorClient.Services.Sync
{
    public sealed class PendingCounterService
    {
        public int Count { get; private set; }
        public event Action? Changed;

        public void Set(int value)
        {
            Count = value < 0 ? 0 : value;
            Changed?.Invoke();
        }

        public void Increment()
        {
            Count++;
            Changed?.Invoke();
        }

        public void Decrement()
        {
            if (Count > 0) Count--;
            Changed?.Invoke();
        }
    }
}
