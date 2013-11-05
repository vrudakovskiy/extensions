using System.Threading.Tasks;

namespace System.Extensions.System.Threading.Tasks
{
    internal static class TaskExtensions
    {
        public static Task Finally(this Task task, Action finallyAction)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (finallyAction == null) throw new ArgumentNullException("finallyAction");

            var tcs = new TaskCompletionSource<object>();
            task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        PerformFinallyAction(tcs, finallyAction);
                        tcs.TrySetResult(null);
                        break;
                    case TaskStatus.Canceled:
                        PerformFinallyAction(tcs, finallyAction);
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        PerformFinallyAction(tcs, finallyAction);
                        var exception = t.Exception;
                        tcs.TrySetException(exception.InnerExceptions);
                        break;
                }
            });

            return tcs.Task;
        }

        public static Task<T> Finally<T>(this Task<T> task, Action finallyAction)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (finallyAction == null) throw new ArgumentNullException("finallyAction");

            var tcs = new TaskCompletionSource<T>();
            task.ContinueWith(t =>
            {
                switch (t.Status)
                {
                    case TaskStatus.RanToCompletion:
                        PerformFinallyAction(tcs, finallyAction);
                        tcs.TrySetResult(t.Result);
                        break;
                    case TaskStatus.Canceled:
                        PerformFinallyAction(tcs, finallyAction);
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        PerformFinallyAction(tcs, finallyAction);
                        var exception = t.Exception;
                        tcs.TrySetException(exception.InnerExceptions);
                        break;
                }
            });

            return tcs.Task;
        }

        private static void PerformFinallyAction<T>(TaskCompletionSource<T> tcs, Action finallyAction)
        {
            try
            {
                finallyAction();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }
    }
}
