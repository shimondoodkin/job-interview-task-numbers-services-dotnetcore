namespace SharedProject.Utils
{

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /**
    * it runns a task with 1 concurrency at most and runs again if triggered in middle of processing to ensure all processed
    **/
    public class ExclusiveTaskRunner
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _runRequested = false;
        private Func<Task> _taskFunc;

        public ExclusiveTaskRunner(Func<Task> taskFunc)
        {
            _taskFunc = taskFunc;
        }

        public async Task RunExclusiveAsync()
        {
            while (true)
            {
                await _semaphore.WaitAsync();
                if (!_runRequested)
                {
                    _runRequested = true;
                    try
                    {
                        await _taskFunc();  // Run the stored task function
                    }
                    finally
                    {
                        _runRequested = false;
                        _semaphore.Release();
                    }

                    // After releasing the semaphore, check if a new run was requested during the task execution
                    if (_runRequested)
                    {
                        continue; // Continue in the loop to rerun the task
                    }
                    else
                    {
                        break; // Break the loop if no rerun was requested
                    }
                }
                else
                {
                    _semaphore.Release();
                    break; // Exit if already handled
                }
            }
        }
    }

    /*

    // Usage example
    public async Task MyTask()
    {
        Console.WriteLine("Task started...");
        await Task.Delay(1000); // Simulate work
        Console.WriteLine("Task completed.");
    }

    public async Task MainAsync()
    {
        var runner = new ExclusiveTaskRunner(MyTask);
        await runner.RunExclusiveAsync();
        await runner.RunExclusiveAsync(); // This will effectively queue a rerun if the task was already running.
        var _ = runner.RunExclusiveAsync(); // Assign to discard to explicitly ignore the returned task
        runner.RunExclusiveAsync().ConfigureAwait(false);  // Use ConfigureAwait and don't await the task
    }

    // To run MainAsync from a Main method:
    // Task.Run(() => MainAsync()).Wait();

    */
}