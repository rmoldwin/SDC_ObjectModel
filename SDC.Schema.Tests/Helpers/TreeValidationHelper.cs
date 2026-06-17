using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDC.Schema.Tests.Helpers
{
    /// <summary>
    /// Shared validation utilities for verifying SDC object tree integrity.
    /// Used by functional tests to detect dictionary corruption, orphaned nodes,
    /// and parent-child relationship violations.
    /// </summary>
    public static class TreeValidationHelper
    {
        /// <summary>
        /// Validates that all TopNode dictionaries are consistent and contain no corruption.
        /// Uses dictionary-based validation (not reflection traversal) to verify:
        /// - All nodes have correct GUID/TopNode/ParentNode references
        /// - Parent-child relationships are bidirectional
        /// - IETnodes consistency
        /// </summary>
        /// <param name="topNode">The TopNode to validate (typically FormDesignType or DataElementType)</param>
        /// <param name="contextMessage">Optional context message for assertion failures</param>
        public static void ValidateTreeIntegrity(BaseType topNode, string contextMessage = "")
        {
            Assert.IsNotNull(topNode, $"TopNode cannot be null. {contextMessage}");
            Assert.IsNotNull(topNode.TopNode, $"TopNode.TopNode should reference itself. {contextMessage}");
            Assert.IsTrue(ReferenceEquals(topNode, topNode.TopNode), $"TopNode should be its own TopNode. {contextMessage}");

            // Cast to ITopNode to access public Nodes/IETnodes
            if (topNode is not ITopNode iTopNode)
            {
                Assert.Fail($"TopNode must implement ITopNode interface. {contextMessage}");
                return;
            }

            // Step 1: Access public readonly dictionaries
            var nodes = iTopNode.Nodes;
            var ietNodes = iTopNode.IETnodes;

            Assert.IsNotNull(nodes, $"Nodes dictionary cannot be null. {contextMessage}");

            // Step 2: Validate each node in the Nodes dictionary
            foreach (var kvp in nodes)
            {
                var guid = kvp.Key;
                var node = kvp.Value;

                // 2a: Assert dictionary key matches node's GUID
                Assert.AreEqual(guid, node.ObjectGUID,
                    $"Node in Nodes dictionary has mismatched GUID. Key: {guid}, Node.ObjectGUID: {node.ObjectGUID}. {contextMessage}");

                // 2b: Assert TopNode reference is correct
                Assert.IsTrue(ReferenceEquals(topNode, node.TopNode),
                    $"Node {guid} has incorrect TopNode reference. Expected {topNode.ObjectGUID}, found {node.TopNode?.ObjectGUID}. {contextMessage}");

                // 2c: Assert ParentNode relationship
                if (!ReferenceEquals(node, topNode)) // TopNode has no parent
                {
                    Assert.IsNotNull(node.ParentNode,
                        $"Non-root node {guid} has null ParentNode. {contextMessage}");

                    // Verify parent is also in the same tree
                    Assert.IsTrue(nodes.ContainsKey(node.ParentNode.ObjectGUID),
                        $"Node {guid} has parent {node.ParentNode.ObjectGUID} not in Nodes dictionary. {contextMessage}");
                }
                else
                {
                    Assert.IsNull(node.ParentNode,
                        $"TopNode {guid} should have null ParentNode, but has {node.ParentNode?.ObjectGUID}. {contextMessage}");
                }
            }

            // Step 3: Validate _IETnodes collection consistency (if populated)
            if (ietNodes != null && ietNodes.Count > 0)
            {
                foreach (var ietNode in ietNodes)
                {
                    Assert.IsTrue(nodes.ContainsKey(ietNode.ObjectGUID),
                        $"IET node {ietNode.ObjectGUID} in IETnodes but missing from Nodes dictionary. {contextMessage}");
                    Assert.IsTrue(ReferenceEquals(topNode, ietNode.TopNode),
                        $"IET node {ietNode.ObjectGUID} has incorrect TopNode reference. {contextMessage}");
                }
            }

            // Step 4: Validate GUID uniqueness (all keys in dictionary should be unique by definition,
            // but verify no duplicate ObjectGUID values exist in the node objects themselves)
            var guidValues = nodes.Values.Select(n => n.ObjectGUID).ToList();
            var uniqueGuids = guidValues.Distinct().ToList();
            Assert.AreEqual(guidValues.Count, uniqueGuids.Count,
                $"Duplicate ObjectGUIDs detected among node values. Total: {guidValues.Count}, Unique: {uniqueGuids.Count}. {contextMessage}");
        }

        /// <summary>
        /// Counts all nodes in the tree by querying the TopNode's Nodes dictionary.
        /// This is the authoritative count - dictionary-based, not traversal-based.
        /// </summary>
        /// <param name="topNode">The root node (must implement ITopNode)</param>
        /// <returns>Count of nodes in the tree (including topNode)</returns>
        public static int CountReachableNodes(BaseType topNode)
        {
            Assert.IsNotNull(topNode, "TopNode cannot be null");

            if (topNode is ITopNode iTopNode)
            {
                return iTopNode.Nodes?.Count ?? 0;
            }

            Assert.Fail($"TopNode {topNode.ObjectGUID} does not implement ITopNode");
                return 0; // Never reached due to Assert.Fail
            }

            /// <summary>
            /// Validates that a specific node count expectation is met.
            /// Useful for verifying adds/deletes changed the tree by expected amount.
            /// </summary>
            public static void AssertNodeCount(BaseType topNode, int expectedCount, string message = "")
            {
                Assert.IsNotNull(topNode, $"TopNode is null. {message}");

                if (topNode is not ITopNode iTopNode)
                {
                    Assert.Fail($"TopNode must implement ITopNode. {message}");
                    return;
                }

                var nodes = iTopNode.Nodes;
                int actualCount = nodes.Count;
                Assert.AreEqual(expectedCount, actualCount,
                    $"Expected {expectedCount} nodes, found {actualCount}. {message}");
            }

            /// <summary>
            /// Validates that a node exists in the TopNode dictionaries.
        /// </summary>
        public static void AssertNodeExists(BaseType node, string message = "")
        {
            Assert.IsNotNull(node, $"Node is null. {message}");
            Assert.IsNotNull(node.TopNode, $"Node {node.ObjectGUID} has null TopNode. {message}");

            if (node.TopNode is not ITopNode iTopNode)
            {
                Assert.Fail($"TopNode must implement ITopNode. {message}");
                return;
            }

            var nodes = iTopNode.Nodes;
            Assert.IsTrue(nodes.ContainsKey(node.ObjectGUID),
                $"Node {node.ObjectGUID} not found in Nodes dictionary. {message}");
        }

        /// <summary>
        /// Validates that a node does NOT exist in any TopNode dictionaries (orphaned or removed).
        /// </summary>
        public static void AssertNodeNotExists(BaseType node, string message = "")
        {
            Assert.IsNotNull(node, $"Cannot validate null node. {message}");

            // If node has no TopNode, it's orphaned (expected)
            if (node.TopNode == null) return;

            // If node has TopNode, it should NOT be in dictionaries
            if (node.TopNode is ITopNode iTopNode)
            {
                var nodes = iTopNode.Nodes;
                Assert.IsFalse(nodes.ContainsKey(node.ObjectGUID),
                    $"Node {node.ObjectGUID} should not exist in Nodes but was found. {message}");
            }
        }
    }
}
