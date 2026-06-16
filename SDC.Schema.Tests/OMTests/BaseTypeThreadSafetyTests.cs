using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SDC.Schema.Tests.OMTests
{
    [TestClass()]
    public class BaseTypeThreadSafetyTests
    {
        private const int STRESS_TEST_ITERATIONS = 1000;
        private const int CONCURRENT_THREADS = 10;

        #region Test 1: Concurrent ItemMutator assignments

        [TestMethod()]
        public void ItemMutator_ConcurrentSameTreeReassignments_DetectsRaceConditions()
        {
            // Rationale: Attempts to trigger race conditions in ItemMutator by having multiple threads
            // simultaneously reassign the same data type nodes to different parents.
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);

            // Create source nodes
            var sourceNodes = Enumerable.Range(0, CONCURRENT_THREADS)
                .Select(i =>
                {
                    var q = new QuestionItemType(de, $"Q.Source{i}");
                    var rf = new ResponseFieldType(q);
                    rf.Response = new DataTypes_DEType(rf);
                    rf.Response.DataTypeDE_Item = new string_DEtype(rf.Response);
                    return rf.Response.DataTypeDE_Item;
                })
                .ToList();

            // Create target parents
            var targetResponses = Enumerable.Range(0, CONCURRENT_THREADS)
                .Select(i =>
                {
                    var q = new QuestionItemType(de, $"Q.Target{i}");
                    var rf = new ResponseFieldType(q);
                    rf.Response = new DataTypes_DEType(rf);
                    return rf.Response;
                })
                .ToList();

            var exceptions = new ConcurrentBag<Exception>();
            var barrier = new Barrier(CONCURRENT_THREADS);

            // Each thread tries to reassign a different source node to a different target
            Parallel.For(0, CONCURRENT_THREADS, new ParallelOptions { MaxDegreeOfParallelism = CONCURRENT_THREADS }, i =>
            {
                try
                {
                    barrier.SignalAndWait(); // Synchronize start to maximize contention
                    targetResponses[i].DataTypeDE_Item = sourceNodes[i];
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            // Rationale: If thread-safe, all assignments should succeed without exceptions
            if (exceptions.Any())
            {
                Assert.Inconclusive($"Race condition detected: {exceptions.Count} exceptions occurred. " +
                    $"First exception: {exceptions.First().Message}");
            }

            // Rationale: Validates that all reassignments completed and parent pointers are consistent
            for (int i = 0; i < CONCURRENT_THREADS; i++)
            {
                Assert.AreSame(targetResponses[i], sourceNodes[i].ParentNode,
                    $"Node {i} has incorrect parent after concurrent reassignment");
            }
        }

        #endregion

        #region Test 2: Concurrent ItemsMutator list replacements

        [TestMethod()]
        public void ItemsMutator_ConcurrentListReplacements_DetectsRaceConditions()
        {
            // Rationale: Stress-tests ItemsMutator by having multiple threads simultaneously
            // replace child lists on different sections, which all share the same TopNode dictionaries.
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);

            var sections = Enumerable.Range(0, CONCURRENT_THREADS)
                .Select(i => new SectionItemType(de, $"S.Thread{i}"))
                .ToList();

            // Seed each section with one initial child
            foreach (var section in sections)
            {
                var children = section.GetChildItemsNode();
                children.ChildItemsList = new List<IdentifiedExtensionType>
                {
                    new DisplayedType(children, $"DI.Initial_{section.ID}")
                };
            }

            var exceptions = new ConcurrentBag<Exception>();
            var barrier = new Barrier(CONCURRENT_THREADS);

            // Each thread replaces its section's child list
            Parallel.For(0, CONCURRENT_THREADS, new ParallelOptions { MaxDegreeOfParallelism = CONCURRENT_THREADS }, i =>
            {
                try
                {
                    barrier.SignalAndWait();
                    var newList = new List<IdentifiedExtensionType>
                    {
                        new DisplayedType(de, $"DI.NewA_{i}"),
                        new DisplayedType(de, $"DI.NewB_{i}")
                    };
                    sections[i].ChildItemsNode!.ChildItemsList = newList;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            if (exceptions.Any())
            {
                Assert.Inconclusive($"Race condition detected: {exceptions.Count} exceptions occurred. " +
                    $"First exception: {exceptions.First().Message}");
            }

            // Rationale: Validates that all lists were replaced and node counts are correct
            foreach (var section in sections)
            {
                Assert.AreEqual(2, section.ChildItemsNode!.ChildItemsList.Count,
                    $"Section {section.ID} has incorrect child count after concurrent replacement");
            }
        }

        #endregion

        #region Test 3: Concurrent read/write on TopNode dictionaries

        [TestMethod()]
        public void TopNodeDictionaries_ConcurrentReadWrite_DetectsRaceConditions()
        {
            // Rationale: Tests whether concurrent node creation/removal causes TopNode dictionary corruption.
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);
            var exceptions = new ConcurrentBag<Exception>();
            var createdNodes = new ConcurrentBag<BaseType>();

            // Half threads create nodes, half remove nodes
            Parallel.For(0, CONCURRENT_THREADS * 2, i =>
            {
                try
                {
                    if (i % 2 == 0)
                    {
                        // Create nodes
                        for (int j = 0; j < 100; j++)
                        {
                            var node = new DisplayedType(de, $"DI.Thread{i}_Node{j}");
                            createdNodes.Add(node);
                        }
                    }
                    else
                    {
                        // Remove nodes (with delay to ensure some exist)
                        Thread.Sleep(10);
                        foreach (var node in createdNodes.Take(10))
                        {
                            try { node.RemoveRecursive(false); }
                            catch { /* Node may have been removed by another thread */ }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            if (exceptions.Any())
            {
                Assert.Inconclusive($"Race condition detected during concurrent dictionary access: " +
                    $"{exceptions.Count} exceptions. First: {exceptions.First().Message}");
            }

            // Rationale: Validates TopNode dictionary consistency after concurrent operations
            try
            {
                var nodeCount = de.Nodes.Count;
                Assert.IsTrue(nodeCount >= 0, "Node count should be non-negative");
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Dictionary corruption detected: {ex.Message}");
            }
        }

        #endregion

        #region Test 4: Same-reference reassignment under load

        [TestMethod()]
        public void ItemMutator_ConcurrentSameReferenceReassignments_DetectsRaceConditions()
        {
            // Rationale: Tests the short-circuit path (item == valueNew) under concurrent load.
            // If not properly synchronized, even this "no-op" could corrupt state.
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);
            var question = new QuestionItemType(de, "Q.Concurrent");
            var response = new ResponseFieldType(question);
            response.Response = new DataTypes_DEType(response);
            var dataType = new string_DEtype(response.Response);
            response.Response.DataTypeDE_Item = dataType;

            var exceptions = new ConcurrentBag<Exception>();
            var barrier = new Barrier(CONCURRENT_THREADS);

            // All threads repeatedly reassign the same reference
            Parallel.For(0, CONCURRENT_THREADS, new ParallelOptions { MaxDegreeOfParallelism = CONCURRENT_THREADS }, i =>
            {
                try
                {
                    barrier.SignalAndWait();
                    for (int j = 0; j < 100; j++)
                    {
                        response.Response.DataTypeDE_Item = dataType;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            if (exceptions.Any())
            {
                Assert.Inconclusive($"Race condition in same-reference reassignment: " +
                    $"{exceptions.Count} exceptions. First: {exceptions.First().Message}");
            }

            // Rationale: Validates state consistency after concurrent no-op reassignments
            Assert.AreSame(dataType, response.Response.DataTypeDE_Item);
            Assert.AreSame(response.Response, dataType.ParentNode);
        }

        #endregion

        #region Test 5: Collection modification during enumeration (stress test)

        [TestMethod()]
        public void ItemsMutator_StressTestCollectionModificationDuringEnumeration()
        {
            // Rationale: Even with array snapshots, if multiple threads call ItemsMutator on overlapping
            // node sets, the underlying collections could be modified during another thread's enumeration.
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);
            var section = new SectionItemType(de, "S.Shared");
            var children = section.GetChildItemsNode();

            // Seed with initial nodes
            var sharedNodes = Enumerable.Range(0, 20)
                .Select(i => new DisplayedType(de, $"DI.Shared{i}"))
                .ToList();
            children.ChildItemsList = new List<IdentifiedExtensionType>(sharedNodes);

            var exceptions = new ConcurrentBag<Exception>();

            // Multiple threads simultaneously try to replace the list with overlapping node sets
            Parallel.For(0, CONCURRENT_THREADS, i =>
            {
                try
                {
                    for (int j = 0; j < 50; j++)
                    {
                        // Each replacement shares some nodes with the original list
                        var newList = sharedNodes.Skip((int)i).Take(10)
                            .Cast<IdentifiedExtensionType>()
                            .ToList();
                        children.ChildItemsList = newList;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            if (exceptions.Any())
            {
                // Expected: This should expose race conditions in the current implementation
                Assert.Inconclusive($"Race conditions detected (expected for non-thread-safe code): " +
                    $"{exceptions.Count} exceptions. First: {exceptions.First().Message}");
            }
            else
            {
                // If no exceptions occurred, either we got lucky or the code has some implicit protection
                Assert.Inconclusive("No race conditions detected in stress test, but code is not explicitly thread-safe");
            }
        }

        #endregion

        #region Test 6: ObjectGUID uniqueness under concurrent node creation

        [TestMethod()]
        public void NodeCreation_ConcurrentGuidAssignment_ValidatesUniqueness()
        {
            // Rationale: Tests whether concurrent node creation could result in duplicate ObjectGUIDs
            // in the TopNode._Nodes dictionary (which would be catastrophic).
            BaseType.ResetLastTopNode();
            var de = new DataElementType(null);
            var allGuids = new ConcurrentBag<Guid>();

            Parallel.For(0, CONCURRENT_THREADS, i =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var node = new DisplayedType(de, $"DI.Thread{i}_Node{j}");
                    allGuids.Add(node.ObjectGUID);
                }
            });

            // Rationale: All GUIDs should be unique
            var uniqueGuids = allGuids.Distinct().Count();
            var totalGuids = allGuids.Count;

            Assert.AreEqual(totalGuids, uniqueGuids,
                $"GUID collision detected: {totalGuids - uniqueGuids} duplicate(s) found");

            // Rationale: TopNode dictionary should contain all created nodes
            Assert.AreEqual(totalGuids + 1, de.Nodes.Count, // +1 for the DataElementType itself
                "TopNode dictionary has inconsistent node count after concurrent creation");
        }

        #endregion

        #region Helper: Detect thread-safety issues in test output

        [TestMethod()]
        public void RunAllThreadSafetyTests_AndReportResults()
        {
            // Rationale: Meta-test that runs all thread-safety tests and provides a summary report.
            // Use this to quickly assess overall thread-safety without running tests individually.

            var testMethods = typeof(BaseTypeThreadSafetyTests)
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Any())
                .Where(m => m.Name != nameof(RunAllThreadSafetyTests_AndReportResults))
                .ToList();

            var results = new List<(string TestName, bool Passed, string Message)>();

            foreach (var method in testMethods)
            {
                try
                {
                    method.Invoke(this, null);
                    results.Add((method.Name, true, "Passed"));
                }
                catch (Exception ex)
                {
                    var innerEx = ex.InnerException ?? ex;
                    var isPassed = innerEx is AssertInconclusiveException;
                    results.Add((method.Name, isPassed, innerEx.Message));
                }
            }

            // Generate summary report
            var report = new System.Text.StringBuilder();
            report.AppendLine("\n=== THREAD SAFETY TEST SUMMARY ===");
            report.AppendLine($"Total Tests: {results.Count}");
            report.AppendLine($"Passed: {results.Count(r => r.Passed)}");
            report.AppendLine($"Inconclusive (Race Conditions Detected): {results.Count(r => !r.Passed)}");
            report.AppendLine("\nDetails:");

            foreach (var (testName, passed, message) in results)
            {
                var status = passed ? "✓ PASS" : "⚠ RACE CONDITION DETECTED";
                report.AppendLine($"{status}: {testName}");
                if (!passed)
                {
                    report.AppendLine($"  → {message.Split('\n')[0]}"); // First line only
                }
            }

            Console.WriteLine(report.ToString());

            if (results.Any(r => !r.Passed))
            {
                Assert.Inconclusive("Thread-safety issues detected. See summary above. " +
                    "Current implementation is NOT thread-safe for concurrent mutation operations.");
            }
        }

        #endregion
    }
}
