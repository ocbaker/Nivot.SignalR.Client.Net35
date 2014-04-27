﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Client.Infrastructure
{
    internal sealed class TaskQueue
    {
        private readonly object _lockObj = new object();
        private Task _lastQueuedTask;
        private volatile bool _drained;
        private readonly int? _maxSize;
        private long _size;

        public TaskQueue()
            : this(TaskAsyncHelper.Empty) {
        }

        public TaskQueue(Task initialTask) {
            _lastQueuedTask = initialTask;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is shared code")]
        public TaskQueue(Task initialTask, int maxSize) {
            _lastQueuedTask = initialTask;
            _maxSize = maxSize;
        }

#if !NET35 && !CLIENT_NET45 && !CLIENT_NET4 && !PORTABLE && !NETFX_CORE
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is shared code.")]
        public IPerformanceCounter QueueSizeCounter { get; set; }
#endif

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is shared code")]
        public bool IsDrained {
            get {
                return _drained;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is shared code")]
        public Task Enqueue(Func<object, Task> taskFunc, object state) {
            // Lock the object for as short amount of time as possible
            lock (_lockObj) {
                if (_drained) {
                    return _lastQueuedTask;
                }

                if (_maxSize != null) {
                    // Increment the size if the queue
                    if (Interlocked.Increment(ref _size) > _maxSize) {
                        Interlocked.Decrement(ref _size);

                        // We failed to enqueue because the size limit was reached
                        return null;
                    }

#if !NET35 && !CLIENT_NET45 && !CLIENT_NET4 && !PORTABLE && !NETFX_CORE
                    var counter = QueueSizeCounter;
                    if (counter != null) {
                        counter.Increment();
                    }
#endif
                }

                Task newTask = _lastQueuedTask.Then((next, nextState) => {
                    return next(nextState).Finally(s => {
                        var queue = (TaskQueue)s;
                        if (queue._maxSize != null) {
                            // Decrement the number of items left in the queue
                            Interlocked.Decrement(ref queue._size);

#if !NET35 && !CLIENT_NET45 && !CLIENT_NET4 && !PORTABLE && !NETFX_CORE
                            var counter = QueueSizeCounter;
                            if (counter != null) {
                                counter.Decrement();
                            }
#endif
                        }
                    },
                    this);
                },
                taskFunc, state);

                _lastQueuedTask = newTask;
                return newTask;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is shared code")]
        public Task Enqueue(Func<Task> taskFunc) {
            return Enqueue(state => ((Func<Task>)state).Invoke(), taskFunc);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is shared code")]
        public Task Drain() {
            lock (_lockObj) {
                _drained = true;

                return _lastQueuedTask;
            }
        }
    }
}