using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FiverrNotifications.Logic.Helpers
{
    public class TaskHelper
    {
        private readonly ILogger _logger;

        public TaskHelper(ILogger logger) => _logger = logger;

        public Func<TSource, Task> Safe<TSource>(Func<TSource, Task> selectTask)
        {
            return async source =>
            {
                try
                {
                    await selectTask(source);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            };
        }

        public async Task Safe(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task<TResult> Safe<TResult>(Task<TResult> task, TResult defaultResult = default)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return defaultResult;
            }
        }
    }
}
