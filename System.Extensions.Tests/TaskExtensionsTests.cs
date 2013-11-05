using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace System.Extensions.Tests
{
    [TestClass]
    public class TaskExtensionsTests
    {
        [TestMethod]
        public void CheckFinallyForRanToCompletionTask()
        {
            var taskExecuted = false;
            var finallyCalled = false;
            var continuationExecuted = false;

            var task = Task.Factory
                .StartNew(() =>
                {
                    Assert.IsFalse(taskExecuted);
                    Assert.IsFalse(finallyCalled);
                    Assert.IsFalse(continuationExecuted);

                    taskExecuted = true;
                })
                .Finally(() =>
                {
                    Assert.IsTrue(taskExecuted);
                    Assert.IsFalse(finallyCalled);
                    Assert.IsFalse(continuationExecuted);

                    finallyCalled = true;
                })
                .ContinueWith(t =>
                {
                    Assert.AreEqual(TaskStatus.RanToCompletion, t.Status);

                    Assert.IsTrue(taskExecuted);
                    Assert.IsTrue(finallyCalled);
                    Assert.IsFalse(continuationExecuted);

                    continuationExecuted = true;
                });

            task.Wait();

            Assert.IsTrue(taskExecuted);
            Assert.IsTrue(finallyCalled);
            Assert.IsTrue(continuationExecuted);
        }

        [TestMethod]
        public void CheckFinallyForCanceledTask()
        {
            var taskExecuted = false;
            var finallyCalled = false;
            var continuationExecuted = false;

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;
                cts.Cancel();

                var task = Task.Factory
                    .StartNew(() =>
                    {
                        Assert.IsFalse(taskExecuted);
                        Assert.IsFalse(finallyCalled);
                        Assert.IsFalse(continuationExecuted);

                        taskExecuted = true;
                    }, token)
                    .Finally(() =>
                    {
                        Assert.IsFalse(taskExecuted);
                        Assert.IsFalse(finallyCalled);
                        Assert.IsFalse(continuationExecuted);

                        finallyCalled = true;
                    })
                    .ContinueWith(t =>
                    {
                        Assert.AreEqual(TaskStatus.Canceled, t.Status);

                        Assert.IsFalse(taskExecuted);
                        Assert.IsTrue(finallyCalled);
                        Assert.IsFalse(continuationExecuted);

                        continuationExecuted = true;
                    });

                task.Wait();

                Assert.IsFalse(taskExecuted);
                Assert.IsTrue(finallyCalled);
                Assert.IsTrue(continuationExecuted);
            }
        }

        [TestMethod]
        public void CheckFinallyForFaultedTask()
        {
            var taskExecuted = false;
            var finallyCalled = false;
            var continuationExecuted = false;

            var exception = new Exception("Test Error");

            var task = Task.Factory
                .StartNew(() =>
                {
                    Assert.IsFalse(taskExecuted);
                    Assert.IsFalse(finallyCalled);
                    Assert.IsFalse(continuationExecuted);

                    taskExecuted = true;
                    throw exception;
                })
                .Finally(() =>
                {
                    Assert.IsTrue(taskExecuted);
                    Assert.IsFalse(finallyCalled);
                    Assert.IsFalse(continuationExecuted);

                    finallyCalled = true;
                })
                .ContinueWith(t =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, t.Status);
                    Assert.IsNotNull(t.Exception);
                    Assert.AreSame(exception, t.Exception.InnerException);

                    Assert.IsTrue(taskExecuted);
                    Assert.IsTrue(finallyCalled);
                    Assert.IsFalse(continuationExecuted);

                    continuationExecuted = true;
                });

            task.Wait();

            Assert.IsTrue(taskExecuted);
            Assert.IsTrue(finallyCalled);
            Assert.IsTrue(continuationExecuted);
        }


        [TestMethod]
        public void CheckFinallyExceptionPropagation()
        {
            var finallyException = new Exception("Finally exception");

            var task = Task.Factory

                .StartNew(() => { })
                .Finally(() =>
                {
                    throw finallyException;
                })
                .ContinueWith(t =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, t.Status);
                    Assert.IsNotNull(t.Exception);
                    Assert.AreSame(finallyException, t.Exception.InnerException);
                });

            task.Wait();

            using (var cts = new CancellationTokenSource())
            {
                var token = cts.Token;
                cts.Cancel();

                var canceledTask = Task.Factory
                    .StartNew(() => { }, token)
                    .Finally(() =>
                    {
                        throw finallyException;
                    })
                    .ContinueWith(t =>
                    {
                        Assert.AreEqual(TaskStatus.Faulted, t.Status);
                        Assert.IsNotNull(t.Exception);
                        Assert.AreSame(finallyException, t.Exception.InnerException);
                    });

                canceledTask.Wait();
            }

            var faultedTask = Task.Factory
                .StartNew(() =>
                {
                    throw new Exception("Task exception");
                })
                .Finally(() =>
                {
                    throw finallyException;
                })
                .ContinueWith(t =>
                {
                    Assert.AreEqual(TaskStatus.Faulted, t.Status);
                    Assert.IsNotNull(t.Exception);
                    Assert.AreSame(finallyException, t.Exception.InnerException);
                });

            faultedTask.Wait();
        }

    }
}
