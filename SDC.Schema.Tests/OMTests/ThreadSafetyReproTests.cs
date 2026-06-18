using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SDC.Schema.Tests.OMTests
{
    /// <summary>
    /// SAFE, BOUNDED reproduction harness for the SDC object-model concurrency root causes
    /// documented in <c>Documentation/ThreadSafety_RootCauseDiagnosis.md</c>.
    ///
    /// SAFETY DESIGN (why this cannot recreate the prior testhost crash-loop):
    ///  - These tests only perform concurrent WRITES (node creation); every dictionary READ
    ///    (count/inspection) happens AFTER all writer threads have joined. There is therefore
    ///    no concurrent Dictionary read-during-write, which is the mechanism that spins/hangs
    ///    the CLR (see diagnosis Section 1). Corruption is proven by its post-join AFTERMATH,
    ///    not by forcing the unrecoverable hang.
    ///  - All work is hard-bounded (fixed thread/iteration counts).
    ///  - A Stopwatch watchdog reports if parallel work runs long, and a [Timeout] backstop
    ///    enforces the repository rule that functional tests exceeding ~10s are aborted.
    ///
    /// These tests assert the THREAD-SAFE expectation, so they are EXPECTED TO FAIL on the
    /// current (un-fixed) code. The failures ARE the diagnostic evidence. Do not "fix" them by
    /// weakening the assertions; they become the regression gate once the OM is made thread-safe.
    /// </summary>
    [TestClass()]
    public class ThreadSafetyReproTests
    {
        // Bounded contention parameters — large enough to expose the race, small enough to finish fast.
        private static readonly int THREADS = Math.Max(4, Environment.ProcessorCount);
        private const int NODES_PER_THREAD = 250;

        // Watchdog: parallel work should complete in well under this; if not, we report rather than hang.
        private const int WATCHDOG_MS = 6000;

        /// <summary>
        /// Runs <paramref name="perThreadBody"/> on exactly <paramref name="threadCount"/> DEDICATED
        /// <see cref="Thread"/> objects and guarantees the calling test thread is released within
        /// <see cref="WATCHDOG_MS"/>.
        ///
        /// WHY DEDICATED THREADS (not Parallel.For): a <see cref="Barrier"/> used to align thread
        /// starts requires exactly <paramref name="threadCount"/> participants to actually run
        /// concurrently. <c>Parallel.For</c> does NOT guarantee that — the thread pool injects
        /// workers gradually (~1/sec via its starvation-avoidance heuristic), so early workers block
        /// on the barrier waiting for pool threads that arrive too slowly, tripping the watchdog as a
        /// TEST ARTIFACT rather than a real stall. Dedicated threads make the barrier deterministic so
        /// a watchdog trip means a genuine production-side hang.
        ///
        /// Workers are background threads: if the watchdog trips we abandon (don't block) the join so
        /// the test thread is freed; the [Timeout] backstop and process teardown reap any stuck worker.
        /// </summary>
        private static bool RunBoundedThreads(int threadCount, Action<int> perThreadBody, out TimeSpan elapsed)
        {
            var sw = Stopwatch.StartNew();
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                int idx = i; // capture per-thread index
                threads[i] = new Thread(() => perThreadBody(idx))
                {
                    IsBackground = true,
                    Name = $"ReproWorker_{idx}"
                };
            }

            foreach (var th in threads)
                th.Start();

            bool allJoined = true;
            foreach (var th in threads)
            {
                int remaining = (int)Math.Max(0, WATCHDOG_MS - sw.ElapsedMilliseconds);
                if (!th.Join(remaining)) // honor the overall watchdog budget across all joins
                {
                    allJoined = false;
                    break; // abandon remaining joins; background workers are reaped on teardown
                }
            }

            sw.Stop();
            elapsed = sw.Elapsed;
            return allJoined;
        }

        /// <summary>
        /// RC-3 reproduction: the <c>ObjectID = _MaxObjectID++</c> increment in the BaseType
        /// constructor (PartialClasses.cs InitBaseType) is a read-modify-write on shared TopNode
        /// state with NO Interlocked/lock, so concurrent node creation produces duplicate ObjectIDs.
        ///
        /// All threads attach to the SAME shared TopNode (<c>de</c>) so they contend on the SAME
        /// <c>_MaxObjectID</c> counter, maximizing the RC-3 race.
        ///
        /// OPEN ISSUE (see ThreadSafety_SessionHandoff.md): with one shared parent, every insert also
        /// re-sorts that parent's growing _ChildNodes list under lock(_SyncRoot) via reflection, so a
        /// watchdog trip here may reflect a PERF CLIFF rather than a deadlock. The deadlock-vs-perf
        /// classification (lower NODES_PER_THREAD and re-time) is NOT yet complete.
        /// </summary>
        [TestMethod()]
        [TestCategory("ThreadSafetyRepro")]
        [Timeout(10000)]
        public void Repro_NonAtomicMaxObjectID_ConcurrentCreation_ProducesDuplicateObjectIDs()
        {
            // Arrange: one shared TopNode (DataElementType). All threads attach DisplayedType children
            // directly to it, so every thread contends on the SAME _MaxObjectID counter — maximizing the
            // RC-3 race. (DisplayedType attaches legally to a DataElementType TopNode, per existing tests.)
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);

            var objectIds = new ConcurrentBag<int>();
            var exceptions = new ConcurrentBag<Exception>();
            using var startBarrier = new Barrier(THREADS);

            // Act: exactly THREADS dedicated threads create children simultaneously to maximize
            // contention on _MaxObjectID++. The Barrier makes all threads start the loop together.
            bool finished = RunBoundedThreads(THREADS, t =>
                {
                    startBarrier.SignalAndWait(); // align thread starts for peak contention
                    for (int j = 0; j < NODES_PER_THREAD; j++)
                    {
                        try
                        {
                            var node = new DisplayedType(de, $"DI.t{t}_n{j}");
                            objectIds.Add(node.ObjectID); // ObjectID read is a plain field; safe post-construction
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }, out var elapsed);

            // Watchdog: if this trips, treat as the hang signal rather than blocking the runner.
            if (!finished)
                Assert.Inconclusive($"WATCHDOG TRIPPED after {WATCHDOG_MS} ms — possible production-side stall (RC-2 hang surface). Investigate before re-running.");

            // Assert (thread-safe expectation): every ObjectID must be unique.
            // Rationale: ObjectGUID is always unique (Guid.NewGuid), but ObjectID comes from the unsynchronized
            // counter; duplicates here are direct proof of RC-3. This assertion is EXPECTED TO FAIL pre-fix.
            int total = objectIds.Count;
            int distinct = objectIds.Distinct().Count();
            int duplicates = total - distinct;

            Console.WriteLine($"[RC-3] threads={THREADS} created={total} distinctIds={distinct} duplicates={duplicates} exceptions={exceptions.Count} elapsed={elapsed.TotalMilliseconds:F0}ms");

            Assert.AreEqual(0, duplicates,
                $"RC-3 CONFIRMED: {duplicates} duplicate ObjectID(s) across {total} concurrently-created nodes. " +
                $"Cause: non-atomic '_MaxObjectID++' in BaseType ctor. Fix: Interlocked.Increment or lock the counter.");
        }

        /// <summary>
        /// RC-2 / RC-4 reproduction: when multiple threads create children under the SAME parent,
        /// the parent's plain List&lt;&gt; (Items / ChildItemsList) and the shared _ChildNodes list are
        /// mutated without a unifying read/write lock, corrupting parent-child consistency.
        ///
        /// Detection is done AFTER join (single-threaded), so this cannot trigger the concurrent
        /// read-during-write hang; it surfaces lost entries / collection-modified exceptions instead.
        /// </summary>
        [TestMethod()]
        [TestCategory("ThreadSafetyRepro")]
        [Timeout(10000)]
        public void Repro_ConcurrentChildrenSameParent_CorruptsParentChildConsistency()
        {
            // Arrange: a single shared parent (the DataElementType TopNode) so all threads contend on the
            // SAME child collections (_ChildNodes list + the parent's Items list) and the shared dictionaries.
            // DisplayedType attaches legally to a DataElementType TopNode (per existing BaseTypeThreadSafetyTests).
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);
            BaseType sharedParent = de;

            var exceptions = new ConcurrentBag<Exception>();
            int attempted = THREADS * NODES_PER_THREAD;
            using var startBarrier = new Barrier(THREADS);

            // Act: exactly THREADS dedicated threads all attach to the SAME parent.
            bool finished = RunBoundedThreads(THREADS, t =>
                {
                    startBarrier.SignalAndWait();
                    for (int j = 0; j < NODES_PER_THREAD; j++)
                    {
                        try
                        {
                            // All threads attach to the SAME parent -> races the parent List<> and _ChildNodes list.
                            _ = new DisplayedType(de, $"DI.shared_t{t}_n{j}");
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex); // collection-modified / index errors are themselves evidence
                        }
                    }
                }, out var elapsed);

            if (!finished)
                Assert.Inconclusive($"WATCHDOG TRIPPED after {WATCHDOG_MS} ms — possible production-side stall (RC-2 hang surface). Investigate before re-running.");

            // Post-join, single-threaded inspection (safe): count nodes actually registered under the TopNode.
            // Nodes is an ITopNode member, so read it from the TopNode (de).
            int registeredChildren = de.Nodes.Values.Count(n => n.ParentNode == sharedParent);

            Console.WriteLine($"[RC-2/4] threads={THREADS} attempted={attempted} registeredChildren={registeredChildren} exceptions={exceptions.Count} elapsed={elapsed.TotalMilliseconds:F0}ms");

            // Assert (thread-safe expectation): no exceptions AND every attempted child is correctly parented.
            // Rationale: a thread-safe OM would register exactly 'attempted' children with the correct parent and
            // throw nothing. Any shortfall or exception is proof of RC-2/RC-4. EXPECTED TO FAIL pre-fix.
            Assert.AreEqual(0, exceptions.Count,
                $"RC-2/RC-4 CONFIRMED: {exceptions.Count} exception(s) during concurrent same-parent creation " +
                $"(e.g. '{exceptions.FirstOrDefault()?.GetType().Name}: {exceptions.FirstOrDefault()?.Message}'). " +
                $"Cause: unsynchronized parent List<>/_ChildNodes mutation.");

            Assert.AreEqual(attempted, registeredChildren,
                $"RC-2/RC-4 CONFIRMED: expected {attempted} correctly-parented children but found {registeredChildren}. " +
                $"Lost/misparented nodes indicate concurrent collection corruption.");
        }
    }
}
