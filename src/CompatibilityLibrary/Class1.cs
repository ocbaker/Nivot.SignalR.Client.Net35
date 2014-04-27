using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CompatibilityLibrary
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="delay">The time span to wait before completing the returned Task</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="delay"/> is less than -1 or greater than Int32.MaxValue.
        /// </exception>
        /// <remarks>
        /// After the specified time delay, the Task is completed in RanToCompletion state.
        /// </remarks>
        public static Task Delay(TimeSpan delay) {
            return Delay(delay, default(CancellationToken));
        }

        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="delay">The time span to wait before completing the returned Task</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned Task</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="delay"/> is less than -1 or greater than Int32.MaxValue.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The provided <paramref name="cancellationToken"/> has already been disposed.
        /// </exception>        
        /// <remarks>
        /// If the cancellation token is signaled before the specified time delay, then the Task is completed in
        /// Canceled state.  Otherwise, the Task is completed in RanToCompletion state once the specified time
        /// delay has expired.
        /// </remarks>        
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken) {
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > Int32.MaxValue) {
                throw new ArgumentOutOfRangeException("delay", Environment.GetResourceString("Task_Delay_InvalidDelay"));
            }

            return Delay((int)totalMilliseconds, cancellationToken);
        }

        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned Task</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <remarks>
        /// After the specified time delay, the Task is completed in RanToCompletion state.
        /// </remarks>
        public static Task Delay(int millisecondsDelay) {
            return Delay(millisecondsDelay, default(CancellationToken));
        }
        /// <summary>
        /// Creates a Task that will complete after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned Task</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned Task</param>
        /// <returns>A Task that represents the time delay</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// The <paramref name="millisecondsDelay"/> is less than -1.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// The provided <paramref name="cancellationToken"/> has already been disposed.
        /// </exception>        
        /// <remarks>
        /// If the cancellation token is signaled before the specified time delay, then the Task is completed in
        /// Canceled state.  Otherwise, the Task is completed in RanToCompletion state once the specified time
        /// delay has expired.
        /// </remarks>     
        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
            // Throw on non-sensical time
            if (millisecondsDelay < -1) {
                throw new ArgumentOutOfRangeException("millisecondsDelay", Environment.GetResourceString("Task_Delay_InvalidMillisecondsDelay"));
            }

            // some short-cuts in case quick completion is in order
            if (cancellationToken.IsCancellationRequested) {
                // return a Task created as already-Canceled
                return Task.FromCancellation(cancellationToken);
            } else if (millisecondsDelay == 0) {
                // return a Task created as already-RanToCompletion
                return Task.CompletedTask;
            }

            // Construct a promise-style Task to encapsulate our return value
            var promise = new DelayPromise(cancellationToken);

            // Register our cancellation token, if necessary.
            if (cancellationToken.CanBeCanceled) {
                promise.Registration = cancellationToken.InternalRegisterWithoutEC(state => ((DelayPromise)state).Complete(), promise);
            }

            // ... and create our timer and make sure that it stays rooted.
            if (millisecondsDelay != Timeout.Infinite) // no need to create the timer if it's an infinite timeout
            {
                promise.Timer = new Timer(state => ((DelayPromise)state).Complete(), promise, millisecondsDelay, Timeout.Infinite);
                promise.Timer.KeepRootedWhileScheduled();
            }

            // Return the timer proxy task
            return promise;
        }

        /// <summary>Task that also stores the completion closure and logic for Task.Delay implementation.</summary>
        private sealed class DelayPromise : Task<VoidTaskResult>
        {
            internal DelayPromise(CancellationToken token)
                : base() {
                this.Token = token;
#if !FEATURE_PAL && !FEATURE_CORECLR
                if (AsyncCausalityTracer.LoggingOn)
                    AsyncCausalityTracer.TraceOperationCreation(CausalityTraceLevel.Required, this.Id, "Task.Delay", 0);

                if (Task.s_asyncDebuggingEnabled) {
                    AddToActiveTasks(this);
                }
#endif
            }

            internal readonly CancellationToken Token;
            internal CancellationTokenRegistration Registration;
            internal Timer Timer;

            internal void Complete() {
                // Transition the task to completed.
                bool setSucceeded;

                if (Token.IsCancellationRequested) {
                    setSucceeded = TrySetCanceled(Token);
                } else {
#if !FEATURE_PAL && !FEATURE_CORECLR
                    if (AsyncCausalityTracer.LoggingOn)
                        AsyncCausalityTracer.TraceOperationCompletion(CausalityTraceLevel.Required, this.Id, AsyncCausalityStatus.Completed);

                    if (Task.s_asyncDebuggingEnabled) {
                        RemoveFromActiveTasks(this.Id);
                    }
#endif
                    setSucceeded = TrySetResult(default(VoidTaskResult));
                }

                // If we won the ----, also clean up.
                if (setSucceeded) {
                    if (Timer != null) Timer.Dispose();
                    Registration.Dispose();
                }
            }
        }
    }
}
