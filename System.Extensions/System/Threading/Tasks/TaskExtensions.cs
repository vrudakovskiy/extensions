using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static Task<T> ReturnAsync<T>(this T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        public static Task<T> ThrowAsync<T>(this Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            var tcs = new TaskCompletionSource<T>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static void SetFrom<T>(this TaskCompletionSource<T> tcs, Task task, Func<T> getResult)
        {
            if (tcs == null) throw new ArgumentNullException("tcs");
            if (task == null) throw new ArgumentNullException("task");

            SetFromImpl(task, getResult,
               tcs.SetResult,
               tcs.SetCanceled,
               tcs.SetException);
        }

        public static void SetFrom<T>(this TaskCompletionSource<T> tcs, Task<T> task)
        {
            if (tcs == null) throw new ArgumentNullException("tcs");
            if (task == null) throw new ArgumentNullException("task");

            tcs.SetFrom(task, () => task.Result);
        }

        public static void TrySetFrom<T>(this TaskCompletionSource<T> tcs, Task task, Func<T> getResult)
        {
            if (tcs == null) throw new ArgumentNullException("tcs");
            if (task == null) throw new ArgumentNullException("task");

            SetFromImpl(task, getResult,
                x => tcs.TrySetResult(x),
                () => tcs.TrySetCanceled(),
                x => tcs.TrySetException(x));
        }

        public static void TrySetFrom<T>(this TaskCompletionSource<T> tcs, Task<T> task)
        {
            if (tcs == null) throw new ArgumentNullException("tcs");
            if (task == null) throw new ArgumentNullException("task");

            tcs.TrySetFrom(task, () => task.Result);
        }

        private static void SetFromImpl<T>(Task task, Func<T> getResult,
            Action<T> setResult, Action setCanceled, Action<IEnumerable<Exception>> setException)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    setResult(getResult());
                    break;
                case TaskStatus.Canceled:
                    setCanceled();
                    break;
                case TaskStatus.Faulted:
                    var exception = task.Exception;
                    setException(exception.InnerExceptions);
                    break;
            }
        }

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
                        var exception = t.Exception;
                        PerformFinallyAction(tcs, finallyAction);
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
                        var exception = t.Exception;
                        PerformFinallyAction(tcs, finallyAction);
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
