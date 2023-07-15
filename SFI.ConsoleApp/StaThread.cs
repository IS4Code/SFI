using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace IS4.SFI.ConsoleApp
{
    /// <summary>
    /// Used to execute code in a STA context.
    /// </summary>
    static class StaThread
    {
        static readonly TaskScheduler scheduler = new StaTaskScheduler(1);

        static bool IsSTA => Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

        public static void Invoke(Action action)
        {
            if(IsSTA)
            {
                action();
            }else{
                try{
                    Task.Factory.StartNew(action, CancellationToken.None, 0, scheduler).Wait();
                }catch(AggregateException e) when(e.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException!).Throw();
                    throw;
                }
            }
        }
        
        public static T Invoke<T>(Func<T> func)
        {
            if(IsSTA)
            {
                return func();
            }else{
                try{
                    return Task<T>.Factory.StartNew(func, CancellationToken.None, 0, scheduler).Result;
                }catch(AggregateException e) when(e.InnerExceptions.Count == 1)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException!).Throw();
                    throw;
                }
            }
        }

        public static ValueTask InvokeAsync(Action action)
        {
            if(IsSTA)
            {
                action();
                return new();
            }else{
                return new(Task.Factory.StartNew(action, CancellationToken.None, 0, scheduler));
            }
        }

        public static ValueTask<T> InvokeAsync<T>(Func<T> func)
        {
            if(IsSTA)
            {
                return new(func());
            }else{
                return new(Task<T>.Factory.StartNew(func, CancellationToken.None, 0, scheduler));
            }
        }
    }
}
