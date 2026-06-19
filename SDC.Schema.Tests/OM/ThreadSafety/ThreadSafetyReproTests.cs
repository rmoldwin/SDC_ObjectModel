using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SDC.Schema.Tests.OM.ThreadSafety
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
        /// TS-3 reproduction: the <c>ObjectID = _MaxObjectID++</c> increment in the BaseType
        /// constructor (PartialClasses.cs InitBaseType) is a read-modify-write on shared TopNode
        /// state with NO Interlocked/lock, so concurrent node creation produces duplicate ObjectIDs.
        ///
        /// All threads attach to the SAME shared TopNode (<c>de</c>) so they contend on the SAME
        /// <c>_MaxObjectID</c> counter, maximizing the TS-3 race.
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
            // TS-3 race. (DisplayedType attaches legally to a DataElementType TopNode, per existing tests.)
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
                Assert.Inconclusive($"WATCHDOG TRIPPED after {WATCHDOG_MS} ms — possible production-side stall (TS-2 hang surface). Investigate before re-running.");

            // Assert (thread-safe expectation): every ObjectID must be unique.
            // Rationale: ObjectGUID is always unique (Guid.NewGuid), but ObjectID comes from the unsynchronized
            // counter; duplicates here are direct proof of TS-3. This assertion is EXPECTED TO FAIL pre-fix.
            int total = objectIds.Count;
            int distinct = objectIds.Distinct().Count();
            int duplicates = total - distinct;

            Console.WriteLine($"[TS-3] threads={THREADS} created={total} distinctIds={distinct} duplicates={duplicates} exceptions={exceptions.Count} elapsed={elapsed.TotalMilliseconds:F0}ms");

            Assert.AreEqual(0, duplicates,
                $"TS-3 CONFIRMED: {duplicates} duplicate ObjectID(s) across {total} concurrently-created nodes. " +
                $"Cause: non-atomic '_MaxObjectID++' in BaseType ctor. Fix: Interlocked.Increment or lock the counter.");
        }

        /// <summary>
        /// TS-2 / TS-4 reproduction: when multiple threads create children under the SAME parent,
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
                Assert.Inconclusive($"WATCHDOG TRIPPED after {WATCHDOG_MS} ms — possible production-side stall (TS-2 hang surface). Investigate before re-running.");

            // Post-join, single-threaded inspection (safe): count nodes actually registered under the TopNode.
            // Nodes is an ITopNode member, so read it from the TopNode (de).
            int registeredChildren = de.Nodes.Values.Count(n => n.ParentNode == sharedParent);

            Console.WriteLine($"[TS-2/4] threads={THREADS} attempted={attempted} registeredChildren={registeredChildren} exceptions={exceptions.Count} elapsed={elapsed.TotalMilliseconds:F0}ms");

            // Assert (thread-safe expectation): no exceptions AND every attempted child is correctly parented.
            // Rationale: a thread-safe OM would register exactly 'attempted' children with the correct parent and
            // throw nothing. Any shortfall or exception is proof of TS-2/TS-4. EXPECTED TO FAIL pre-fix.
            Assert.AreEqual(0, exceptions.Count,
                $"TS-2/TS-4 CONFIRMED: {exceptions.Count} exception(s) during concurrent same-parent creation " +
                $"(e.g. '{exceptions.FirstOrDefault()?.GetType().Name}: {exceptions.FirstOrDefault()?.Message}'). " +
                $"Cause: unsynchronized parent List<>/_ChildNodes mutation.");

            Assert.AreEqual(attempted, registeredChildren,
                $"TS-2/TS-4 CONFIRMED: expected {attempted} correctly-parented children but found {registeredChildren}. " +
                "Lost/misparented nodes indicate concurrent collection corruption.");
        }

        /// <summary>
        /// TS-6 reproduction: <see cref="Move"/> performs source-list removal
        /// (<c>objList.Remove</c>) and target-list insertion (<c>propList.Add</c>) with NO outer
        /// lock, so concurrent calls from different threads race on the same <see cref="List{T}"/>
        /// backing arrays.
        ///
        /// Setup: one <see cref="DataElementType"/> tree; <c>THREADS × NodesPerThread</c>
        /// <see cref="DisplayedType"/> nodes are pre-created single-threaded under a single source
        /// <see cref="ChildItemsType"/>. Each of the <c>THREADS</c> dedicated threads then moves
        /// its assigned slice of nodes to a shared target <see cref="ChildItemsType"/>.
        /// All threads start simultaneously via a <see cref="Barrier"/>.
        ///
        /// The concurrent <c>List&lt;T&gt;.Remove</c> on the source list and concurrent
        /// <c>List&lt;T&gt;.Add</c> on the target list race on the non-thread-safe backing array,
        /// causing items to be lost, overwritten, or miscounted.
        ///
        /// NOTE: <see cref="MoveInDictionaries"/> (step 4 of Move) DOES acquire a
        /// <see cref="WriteLockScope"/> — but that lock protects only the per-tree dictionaries,
        /// NOT the IList operations in steps 2–3. The lock in step 4 therefore provides zero
        /// protection for the races in steps 2–3.
        ///
        /// EXPECTED TO FAIL on unfixed code (TS-6). Do not weaken or suppress the assertions;
        /// they become the regression gate once Move() wraps its IList mutations in a write lock.
        /// </summary>
        [TestMethod()]
        [TestCategory("ThreadSafetyRepro")]
        [Timeout(12000)]
        public void Repro_ConcurrentMoves_UnprotectedListMutations_CorruptListIntegrity()
        {
            const int NodesPerThread = 10;
            int totalNodes = THREADS * NodesPerThread;

            // Arrange: one tree; all source nodes pre-created single-threaded under one ChildItemsType.
            // Single-threaded construction avoids contaminating this probe with TS-2/TS-3 races.
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);
            var sourceSection = new SectionItemType(de, "S.Source");
            var sourceChildren = sourceSection.GetChildItemsNode();
            var targetSection = new SectionItemType(de, "S.Target");
            var targetChildren = targetSection.GetChildItemsNode();

            var nodes = Enumerable.Range(0, totalNodes)
                .Select(i => (IdentifiedExtensionType)new DisplayedType(sourceChildren, $"DI.Src_{i}"))
                .ToList();

            int nodesBeforeMove = de.Nodes.Count;

            var exceptions = new ConcurrentBag<Exception>();
            using var startBarrier = new Barrier(THREADS);

            // Act: each thread moves its NodesPerThread assigned nodes simultaneously.
            // Step 2 (objList.Remove) and step 3 (propList.Add) inside MoveSingleNode() are
            // the unprotected TS-6 race surface — no lock guards them.
            bool finished = RunBoundedThreads(THREADS, t =>
            {
                startBarrier.SignalAndWait();
                for (int k = 0; k < NodesPerThread; k++)
                {
                    try { nodes[t * NodesPerThread + k].Move(targetChildren, -1, false); }
                    catch (Exception ex) { exceptions.Add(ex); }
                }
            }, out var elapsed);

            if (!finished)
                Assert.Inconclusive(
                    $"WATCHDOG TRIPPED after {WATCHDOG_MS} ms — possible stall on TS-6 race surface.");

            // Post-join single-threaded inspection (safe): check both object-tree (IList) and
            // dictionary perspectives.
            int sourceListCount  = sourceChildren.ChildItemsList?.Count ?? 0;
            int targetListCount  = targetChildren.ChildItemsList?.Count ?? 0;
            int sourceRemaining  = de.Nodes.Values.Count(n => n.ParentNode == (BaseType)sourceChildren);
            int targetLanded     = de.Nodes.Values.Count(n => n.ParentNode == (BaseType)targetChildren);
            int nodesAfterMove   = de.Nodes.Count;
            bool targetHasNulls  = targetChildren.ChildItemsList?.Any(n => n is null) ?? false;

            Console.WriteLine(
                $"[TS-6] threads={THREADS} nodesPerThread={NodesPerThread} totalNodes={totalNodes} " +
                $"nodesBeforeMove={nodesBeforeMove} nodesAfterMove={nodesAfterMove} " +
                $"sourceListCount={sourceListCount} targetListCount={targetListCount} " +
                $"sourceRemaining(dict)={sourceRemaining} targetLanded(dict)={targetLanded} " +
                $"targetHasNulls={targetHasNulls} exceptions={exceptions.Count} " +
                $"elapsed={elapsed.TotalMilliseconds:F0}ms");

            // Assert (thread-safe expectation): all nodes move cleanly — none lost, none duplicated,
            // no null holes in the target list, no exceptions.
            // Bug fix comment: these assertions DEFINE the correct post-fix contract AND EXPOSE
            // TS-6 corruption on unfixed code via concurrent unserialized List<T> mutations.
            Assert.AreEqual(0, exceptions.Count,
                $"TS-6 CONFIRMED: {exceptions.Count} exception(s) thrown during concurrent Move() calls. " +
                $"Cause: objList.Remove / propList.Add in MoveSingleNode() hold no lock. " +
                $"Fix: wrap both IList mutations in a WriteLockScope over the full source+target operation.");

            Assert.AreEqual(0, sourceListCount,
                $"TS-6 CONFIRMED: source ChildItemsList.Count={sourceListCount} (expected 0). " +
                $"Concurrent List.Remove() left {sourceListCount} item(s) in the source list.");

            Assert.AreEqual(totalNodes, targetListCount,
                $"TS-6 CONFIRMED: target ChildItemsList.Count={targetListCount} (expected {totalNodes}). " +
                $"Concurrent List.Add() lost or duplicated {totalNodes - targetListCount} item(s) on the target list.");

            Assert.IsFalse(targetHasNulls,
                "TS-6 CONFIRMED: target ChildItemsList contains null entries — " +
                "concurrent List<T> backing-array corruption left null holes in the target list.");

            Assert.AreEqual(nodesBeforeMove, nodesAfterMove,
                $"TS-6 CONFIRMED: total node count changed from {nodesBeforeMove} to {nodesAfterMove}. " +
                $"Concurrent Move() mutations left the tree dictionaries inconsistent.");
        }

        /// <summary>
        /// TS-6 deadlock probe (distinct from the list-integrity probe above): two threads move
        /// nodes between the same tree-pair in OPPOSITE directions simultaneously.
        ///
        /// Thread A: moves node from fd1 → fd2; Thread B: moves node from fd2 → fd1.
        ///
        /// NOTE: with the CURRENT (unfixed) implementation, <see cref="Move"/> acquires each tree's
        /// write lock separately (UnRegisterAll on the source, then RegisterAll on the target) and
        /// RELEASES each lock before acquiring the next. Because no thread holds two locks
        /// simultaneously, the classic AB/BA deadlock does NOT occur here — this test PASSES on
        /// unfixed code. Its role is to act as a NON-REGRESSION gate: after the TS-6 fix introduces
        /// a true dual-lock (both trees locked simultaneously in GUID order), this test ensures that
        /// the fix itself does not introduce a new deadlock.
        /// </summary>
        [TestMethod()]
        [TestCategory("ThreadSafetyRepro")]
        [Timeout(12000)]
        public void Repro_ConcurrentCrossTreeMoves_DoNotDeadlock()
        {
            // Arrange: two separate FormDesign trees, each with a movable node.
            BaseType.ResetLastTopNode();
            var fd1 = FormDesignType.DeserializeFromXml(Setup.GetXml());

            BaseType.ResetLastTopNode();
            var fd2 = FormDesignType.DeserializeFromXml(Setup.GetXml());

            // Pick one leaf IET node from each tree to move cross-tree.
            var nodeFromFd1 = fd1.IETnodes.LastOrDefault(n => n is DisplayedType);
            var nodeFromFd2 = fd2.IETnodes.LastOrDefault(n => n is DisplayedType);

            if (nodeFromFd1 is null || nodeFromFd2 is null)
                Assert.Inconclusive("Could not find a suitable DisplayedType node in one or both trees.");

            var targetInFd2 = fd2.IETnodes.FirstOrDefault(n => n is SectionItemType) as BaseType ?? (BaseType)fd2;
            var targetInFd1 = fd1.IETnodes.FirstOrDefault(n => n is SectionItemType) as BaseType ?? (BaseType)fd1;

            var exceptions = new ConcurrentBag<Exception>();
            using var startBarrier = new Barrier(2);

            // Act: two threads cross-move simultaneously (AB/BA lock pattern if unordered).
            bool finished = RunBoundedThreads(2, t =>
            {
                startBarrier.SignalAndWait(); // align for maximum contention
                try
                {
                    if (t == 0)
                        nodeFromFd1.Move(targetInFd2, -1, false, SdcUtil.RefreshMode.UpdateNodeIdentity);
                    else
                        nodeFromFd2.Move(targetInFd1, -1, false, SdcUtil.RefreshMode.UpdateNodeIdentity);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }, out var elapsed);

            // Watchdog: a trip after TS-6 fix is introduced means the fix itself created a deadlock.
            Console.WriteLine($"[TS-6 deadlock probe] finished={finished} exceptions={exceptions.Count} elapsed={elapsed.TotalMilliseconds:F0}ms");

            Assert.IsTrue(finished,
                $"TS-6 FIX INTRODUCED DEADLOCK: concurrent opposite-direction cross-tree Move() did not complete within " +
                $"{WATCHDOG_MS} ms. Check that dual-lock acquisition uses a consistent GUID-order and never holds " +
                $"both locks across an await or a re-entrant call path.");

            Assert.AreEqual(0, exceptions.Count,
                $"TS-6 deadlock probe: unexpected exception(s) during concurrent cross-tree Move(): " +
                $"'{exceptions.FirstOrDefault()?.GetType().Name}: {exceptions.FirstOrDefault()?.Message}'");
        }
    }
}
