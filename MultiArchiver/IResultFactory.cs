using MorseCode.ITask;

namespace IS4.MultiArchiver
{
    public interface IResultFactory<out TResult, in TArgs>
    {
        ITask<TResult> Invoke<T>(T value, TArgs args) where T : class;
    }

    public delegate ITask<TResult> ResultFactory<in T, out TResult, in TArgs>(T value, TArgs args) where T : class;
}
