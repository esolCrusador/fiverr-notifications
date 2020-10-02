using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Helpers
{
    public static class ObservableExtensions
    {
        public static IObservable<int> SelectAsync(this IObservable<Task> source) =>
            source.Select(async task =>
            {
                await task;
                return 1;
            }).SelectAsync();

        public static IObservable<TDest> SelectAsync<TSource, TDest>(this IObservable<TSource> source, Func<TSource, Task<TDest>> select) =>
            source.Select(select).SelectAsync();

        public static IObservable<int> SelectAsync<TSource>(this IObservable<TSource> source, Func<TSource, Task> select) =>
            source.Select(async task =>
            {
                await select(task);
                return 1;
            }).SelectAsync();

        public static IObservable<TResult> SelectAsync<TResult>(this IObservable<Task<TResult>> observable) =>
            Observable.Create<TResult>(observer =>
            {
                var tasks = new ConcurrentBag<Task>();

                var subscription = observable.Subscribe(async task =>
                {
                    tasks.Add(task);
                    try
                    {
                        observer.OnNext(await task);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                exception =>
                {
                    observer.OnError(exception);
                },
                async () =>
                {
                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }
                });

                return subscription;
            });
    }
}
