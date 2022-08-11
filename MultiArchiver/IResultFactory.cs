using MorseCode.ITask;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Represents a "generic" version of <see cref="ResultFactory{T, TResult, TArgs}"/>
    /// where the type of the first argument of <see cref="Invoke{T}(T, TArgs)"/>
    /// is provided by the invoking code. When <typeparamref name="TResult"/> or
    /// <typeparamref name="TArgs"/> should be empty, using <see cref="System.ValueTuple"/>
    /// is recommended.
    /// </summary>
    /// <typeparam name="TResult">The implementer-defined type of the value returned by <see cref="Invoke{T}(T, TArgs)"/>.</typeparam>
    /// <typeparam name="TArgs">The implementer-defined type of the arguments used by <see cref="Invoke{T}(T, TArgs)"/>.</typeparam>
    public interface IResultFactory<out TResult, in TArgs>
    {
        /// <summary>
        /// Invokes the delegate, passing the produced object.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
        /// <param name="value">The produced object.</param>
        /// <param name="args">Custom arguments passed to the method.</param>
        /// <returns>The result of the method.</returns>
        ITask<TResult> Invoke<T>(T value, TArgs args) where T : class;
    }

    /// <summary>
    /// A general delegate receiving a result of an operation, alongside user-provided
    /// arguments, intended to return a custom value to the outside code.
    /// <typeparamref name="TArgs"/> should be empty, using <see cref="System.ValueTuple"/>
    /// is recommended.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="value"/>.</typeparam>
    /// <typeparam name="TResult">The implementer-defined type of the value returned when invoked.</typeparam>
    /// <typeparam name="TArgs">The implementer-defined type of the arguments used when invoked.</typeparam>
    /// <param name="value">The produced object.</param>
    /// <param name="args">Custom arguments passed to the method.</param>
    /// <returns>The result of the method.</returns>
    public delegate ITask<TResult> ResultFactory<in T, out TResult, in TArgs>(T value, TArgs args) where T : class;
}
