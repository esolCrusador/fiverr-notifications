using Microsoft.Extensions.Logging;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Helpers
{
    public static class ObservableHelper
    {
        public static IObservable<Unit> SelectAsync(this IObservable<Task> source) =>
            source.Select(task => Observable.FromAsync(() => task)).Merge();

        public static IObservable<TDest> SelectAsync<TSource, TDest>(this IObservable<TSource> source, Func<TSource, Task<TDest>> select) =>
            source.Select(select).SelectAsync();

        public static IObservable<Unit> SelectAsync<TSource>(this IObservable<TSource> source, Func<TSource, Task> select) =>
            source.Select(task => Observable.FromAsync(() => select(task))).Merge();

        public static IObservable<TResult> SelectAsync<TResult>(this IObservable<Task<TResult>> observable)
        {
            return observable.Select(task => Observable.FromAsync(() => task)).Merge(); // Parallel execution
        }

        public static IObservable<TElement> LogException<TElement>(this IObservable<TElement> observable, ILogger logger)
        {
            return observable.Catch<TElement, Exception>(ex =>
            {
                if (ex is AggregateException aex && aex.InnerExceptions.Count == 1)
                    logger.LogError(aex.InnerExceptions[0], aex.InnerExceptions[0].Message);
                else
                    logger.LogError(ex, ex.Message);

                return observable.LogException(logger);
            });
        }

        public static IObservable<Unit> FromAction(Action action)
        {
            return Observable.Create<Unit>(observer =>
                {
                    try
                    {
                        action();
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }

                    return Task.CompletedTask;
                }
            );
        }

        internal static IObservable<Unit> One()
        {
            return Of(Unit.Default);
        }

        internal static IObservable<TValue> Of<TValue>(TValue value)
        {
            return Observable.Create<TValue>(observer =>
            {
                observer.OnNext(value);
                observer.OnCompleted();

                return Task.CompletedTask;
            });
        }
    }
}