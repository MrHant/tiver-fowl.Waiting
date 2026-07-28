namespace TestsCore.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Tiver.Fowl.Waiting.Timing;

    /// <summary>
    /// A virtual clock for the wait loop. Time advances only where the production loop
    /// would genuinely have waited - a polling sleep, or a timeout expiring on a blocked
    /// condition - so elapsed time and poll counts become exact instead of approximate.
    /// </summary>
    /// <remarks>
    /// Running a condition costs zero virtual milliseconds. A condition that needs to
    /// consume time must park on a <see cref="VirtualGate"/>.
    /// <para>
    /// Use one instance per <c>Wait.Until</c> call, installed through <see cref="FakeTime"/>.
    /// </para>
    /// </remarks>
    internal sealed class VirtualWaitTimer : IWaitTimer
    {
        /// <summary>
        /// Liveness guard only: a condition that blocks on something other than a
        /// <see cref="VirtualGate"/> would otherwise hang the run forever. It never
        /// decides a test outcome - reaching it is always an infrastructure error.
        /// </summary>
        private const int LivenessGraceMilliseconds = 30_000;

        private readonly object _sync = new object();
        private readonly List<int> _recordedSleeps = new List<int>();
        private readonly List<VirtualGate> _parkedGates = new List<VirtualGate>();
        private readonly ManualResetEventSlim _anyGateParked = new ManualResetEventSlim(false);

        private long _elapsedMilliseconds;

        public long ElapsedMilliseconds
        {
            get
            {
                lock (_sync)
                {
                    return _elapsedMilliseconds;
                }
            }
        }

        /// <summary>
        /// Every polling sleep the wait loop asked for, in order.
        /// </summary>
        public IReadOnlyList<int> RecordedSleeps
        {
            get
            {
                lock (_sync)
                {
                    return _recordedSleeps.ToArray();
                }
            }
        }

        public void Stop()
        {
            // Elapsed virtual time is only ever advanced deliberately, so there is
            // nothing to stop.
        }

        public void Sleep(int milliseconds)
        {
            lock (_sync)
            {
                _recordedSleeps.Add(milliseconds);
                _elapsedMilliseconds += milliseconds;
            }
        }

        /// <summary>
        /// Creates a gate that only the test can open - a condition that blocks for good.
        /// </summary>
        public VirtualGate CreateGate()
        {
            return new VirtualGate(this, null);
        }

        /// <summary>
        /// Creates a gate that opens once virtual time reaches
        /// <paramref name="opensAtVirtualMilliseconds"/> - a condition that takes that
        /// long to produce its result.
        /// </summary>
        public VirtualGate CreateGate(long opensAtVirtualMilliseconds)
        {
            return new VirtualGate(this, opensAtVirtualMilliseconds);
        }

        public bool WaitForTask(Task task, TimeSpan timeout)
        {
            if (task.IsCompleted)
            {
                return true;
            }

            long target;
            lock (_sync)
            {
                target = _elapsedMilliseconds + (long)timeout.TotalMilliseconds;
            }

            var handles = new[]
            {
                ((IAsyncResult)task).AsyncWaitHandle,
                _anyGateParked.WaitHandle
            };

            while (true)
            {
                // Wait for the condition to reach an observable state: finished, or
                // parked on a gate. Never guess - the whole point is determinism.
                var signalled = WaitHandle.WaitAny(handles, LivenessGraceMilliseconds);

                if (signalled == 0)
                {
                    // Completed within the timeout, at no virtual cost.
                    return true;
                }

                if (signalled == WaitHandle.WaitTimeout)
                {
                    throw new InvalidOperationException(
                        "Condition neither completed nor parked within " +
                        LivenessGraceMilliseconds + "ms of real time. A blocking condition " +
                        "under FakeTime must block via VirtualGate.Park() instead of Thread.Sleep.");
                }

                var due = TakeNextDueGate(target);
                if (due == null)
                {
                    // Nothing can unblock the condition before the timeout expires,
                    // so the loop times out with the task still pending - exactly what
                    // production does.
                    lock (_sync)
                    {
                        _elapsedMilliseconds = target;
                    }

                    return false;
                }

                // The condition resumes and either completes or parks again; either way
                // the next iteration observes it.
                due.OpenAndAwaitResume();
            }
        }

        internal void EnterPark(VirtualGate gate)
        {
            lock (_sync)
            {
                _parkedGates.Add(gate);
                _anyGateParked.Set();
            }
        }

        internal void ExitPark(VirtualGate gate)
        {
            lock (_sync)
            {
                _parkedGates.Remove(gate);
                if (_parkedGates.Count == 0)
                {
                    _anyGateParked.Reset();
                }
            }
        }

        /// <summary>
        /// Advances virtual time to the earliest parked gate that comes due no later than
        /// <paramref name="target"/> and returns it, or returns <c>null</c> when no parked
        /// gate opens in time.
        /// </summary>
        private VirtualGate TakeNextDueGate(long target)
        {
            lock (_sync)
            {
                VirtualGate due = null;
                foreach (var gate in _parkedGates)
                {
                    var opensAt = gate.OpensAtVirtualMilliseconds;
                    if (opensAt == null || opensAt.Value > target)
                    {
                        continue;
                    }

                    if (due == null || opensAt.Value < due.OpensAtVirtualMilliseconds.Value)
                    {
                        due = gate;
                    }
                }

                if (due == null)
                {
                    return null;
                }

                _elapsedMilliseconds = Math.Max(
                    _elapsedMilliseconds,
                    due.OpensAtVirtualMilliseconds.Value);
                return due;
            }
        }
    }
}
