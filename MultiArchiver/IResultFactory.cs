namespace IS4.MultiArchiver
{
    public interface IResultFactory<out TResult, in TArgs>
    {
        TResult Invoke<T>(T value, TArgs args) where T : class;
    }

    public delegate TResult ResultFactory<T, out TResult, in TArgs>(T value, TArgs args) where T : class;
}
