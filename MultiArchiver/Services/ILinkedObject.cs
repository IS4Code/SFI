namespace IS4.MultiArchiver.Services
{
    public interface ILinkedObject
    {
        ILinkedNode Node { get; }
        object Source { get; }

        string Label { get; set; }
    }

    public interface ILinkedObject<T> : ILinkedObject where T : class
    {
        T Value { get; }
    }

    public sealed class LinkedObject<T> : ILinkedObject<T> where T : class
    {
        public ILinkedNode Node { get; }
        public object Source { get; }
        public T Value { get; }
        public string Label { get; set; }

        public LinkedObject(ILinkedNode node, object source, T value)
        {
            Node = node;
            Source = source;
            Value = value;
        }

        public override string ToString()
        {
            return Value?.ToString();
        }
    }
}
