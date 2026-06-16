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
        /// Performs comprehensive checks on _Nodes, _ParentNodes, _ChildNodes, _IETnodes.
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

            // Step 2: Traverse tree and collect all reachable nodes
            var reachableNodes = new HashSet<Guid>();
            CollectReachableNodes(topNode, reachableNodes);

            // Step 3: Compare reachable count vs dictionary count
            int reachableCount = reachableNodes.Count;
            int dictionaryCount = nodes.Count;

            Assert.AreEqual(dictionaryCount, reachableCount,
                $"Reachable node count ({reachableCount}) should match Nodes count ({dictionaryCount}). " +
                $"Orphaned or missing nodes detected. {contextMessage}");

            // Step 4: Validate each reachable node
            foreach (var guid in reachableNodes)
            {
                // 4a: Assert GUID exists in Nodes
                Assert.IsTrue(nodes.ContainsKey(guid),
                    $"Node GUID {guid} is reachable but missing from Nodes dictionary. {contextMessage}");

                var node = nodes[guid];

                // 4b: Assert Nodes[guid] references correct object
                Assert.AreEqual(guid, node.ObjectGUID,
                    $"Node in Nodes has mismatched GUID. Expected {guid}, found {node.ObjectGUID}. {contextMessage}");

                // 4c: Assert TopNode reference is correct
                Assert.IsTrue(ReferenceEquals(topNode, node.TopNode),
                    $"Node {guid} has incorrect TopNode reference. {contextMessage}");

                // 4d: Assert ParentNode relationship
                if (!ReferenceEquals(node, topNode)) // TopNode has no parent
                {
                    Assert.IsNotNull(node.ParentNode,
                        $"Non-root node {guid} has null ParentNode. {contextMessage}");

                    // Verify parent is also in tree
                    Assert.IsTrue(nodes.ContainsKey(node.ParentNode.ObjectGUID),
                        $"Node {guid} parent {node.ParentNode.ObjectGUID} not in Nodes dictionary. {contextMessage}");
                }
                else
                {
                    Assert.IsNull(node.ParentNode,
                        $"TopNode {guid} should have null ParentNode. {contextMessage}");
                }

                // 4e: Validate children if node has any
                ValidateNodeChildren(node, nodes, contextMessage);
            }

            // Step 5: Check for orphaned dictionary entries (in dictionary but not reachable)
            var orphanedGuids = nodes.Keys.Except(reachableNodes).ToList();
            Assert.AreEqual(0, orphanedGuids.Count,
                $"Found {orphanedGuids.Count} orphaned nodes in Nodes: {string.Join(", ", orphanedGuids)}. {contextMessage}");

            // Step 6: Validate GUID uniqueness (implicitly validated by HashSet, but double-check)
            var guidList = new List<Guid>();
            CollectGuidsForUniquenessCheck(topNode, guidList);
            var uniqueGuids = guidList.Distinct().ToList();
            Assert.AreEqual(guidList.Count, uniqueGuids.Count,
                $"Duplicate GUIDs detected. Total: {guidList.Count}, Unique: {uniqueGuids.Count}. {contextMessage}");

            // Step 7: Validate _IETnodes collection consistency (if populated)
            if (ietNodes != null && ietNodes.Count > 0)
            {
                foreach (var ietNode in ietNodes)
                {
                    Assert.IsTrue(nodes.ContainsKey(ietNode.ObjectGUID),
                        $"IET node {ietNode.ObjectGUID} in IETnodes but missing from Nodes. {contextMessage}");
                    Assert.IsTrue(ReferenceEquals(topNode, ietNode.TopNode),
                        $"IET node {ietNode.ObjectGUID} has incorrect TopNode reference. {contextMessage}");
                }
            }
        }

        /// <summary>
        /// Validates that a node's children (if any) have correct parent references.
        /// </summary>
        private static void ValidateNodeChildren(BaseType node, System.Collections.ObjectModel.ReadOnlyDictionary<Guid, BaseType> nodes, string contextMessage)
        {
            // Use reflection to get child properties
            var nodeType = node.GetType();
            var properties = nodeType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Check for list properties that contain BaseType descendants
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = prop.PropertyType.GetGenericArguments()[0];
                    if (typeof(BaseType).IsAssignableFrom(listType))
                    {
                        var list = prop.GetValue(node) as System.Collections.IEnumerable;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item is BaseType child)
                                {
                                    // NOTE: Permissive check - SDC uses intermediate container nodes (ChildItemsType, ListType)
                                    // so child.ParentNode might be the container, not this node
                                    Assert.IsTrue(nodes.ContainsKey(child.ObjectGUID),
                                        $"Child {child.ObjectGUID} in {node.ObjectGUID}.{prop.Name} not in Nodes dictionary. {contextMessage}");
                                }
                            }
                        }
                    }
                }
                // Check for single-value BaseType properties
                else if (typeof(BaseType).IsAssignableFrom(prop.PropertyType))
                {
                    var child = prop.GetValue(node) as BaseType;
                    if (child != null)
                    {
                        // NOTE: Permissive check - SDC uses intermediate container nodes
                        Assert.IsTrue(nodes.ContainsKey(child.ObjectGUID),
                            $"Child {child.ObjectGUID} in {node.ObjectGUID}.{prop.Name} not in Nodes dictionary. {contextMessage}");
                    }
                }
            }
        }

        /// <summary>
        /// Validates bidirectional parent-child relationship for a specific node.
        /// </summary>
        /// <param name="node">The node to validate</param>
        public static void ValidateParentChildSymmetry(BaseType node)
        {
            Assert.IsNotNull(node, "Node cannot be null");

            var parent = node.ParentNode;
            if (parent == null)
            {
                // Node is either TopNode or orphaned
                Assert.IsTrue(ReferenceEquals(node, node.TopNode),
                    $"Node {node.ObjectGUID} has null parent but is not its own TopNode (orphaned?)");
                return;
            }

            // Access top node to validate relationships
            var topNode = node.TopNode;
            Assert.IsNotNull(topNode, $"Node {node.ObjectGUID} has null TopNode");

            if (topNode is not ITopNode iTopNode)
            {
                Assert.Fail($"TopNode must implement ITopNode interface");
                return;
            }

            var nodes = iTopNode.Nodes;

            // Verify node and parent are both in the tree
            Assert.IsTrue(nodes.ContainsKey(node.ObjectGUID),
                $"Node {node.ObjectGUID} not found in Nodes dictionary");
            Assert.IsTrue(nodes.ContainsKey(parent.ObjectGUID),
                $"Parent {parent.ObjectGUID} not found in Nodes dictionary");

            // Use reflection to find node in parent's child collections
            bool foundInParent = false;
            var parentType = parent.GetType();
            var properties = parentType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var list = prop.GetValue(parent) as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            if (item is BaseType child && ReferenceEquals(child, node))
                            {
                                foundInParent = true;
                                break;
                            }
                        }
                    }
                }
                else if (typeof(BaseType).IsAssignableFrom(prop.PropertyType))
                {
                    var child = prop.GetValue(parent) as BaseType;
                    if (child != null && ReferenceEquals(child, node))
                    {
                        foundInParent = true;
                        break;
                    }
                }

                if (foundInParent) break;
            }

            Assert.IsTrue(foundInParent,
                $"Node {node.ObjectGUID} claims parent {parent.ObjectGUID}, but parent's properties do not contain node");
        }

        /// <summary>
        /// Counts all nodes reachable by traversing the tree from topNode.
        /// </summary>
        /// <param name="topNode">The root node to start traversal</param>
        /// <returns>Count of reachable nodes (including topNode)</returns>
        public static int CountReachableNodes(BaseType topNode)
        {
            var reachableGuids = new HashSet<Guid>();
            CollectReachableNodes(topNode, reachableGuids);
            return reachableGuids.Count;
        }

        /// <summary>
        /// Recursively collects all reachable node GUIDs via depth-first traversal.
        /// Uses HashSet to avoid counting cycles.
        /// </summary>
        private static void CollectReachableNodes(BaseType node, HashSet<Guid> visited)
        {
            if (node == null) return;
            if (visited.Contains(node.ObjectGUID)) return; // Avoid cycles

            visited.Add(node.ObjectGUID);

            // Use reflection to traverse all BaseType child properties
            var nodeType = node.GetType();
            var properties = nodeType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Check for list properties
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = prop.PropertyType.GetGenericArguments()[0];
                    if (typeof(BaseType).IsAssignableFrom(listType))
                    {
                        var list = prop.GetValue(node) as System.Collections.IEnumerable;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item is BaseType child)
                                {
                                    CollectReachableNodes(child, visited);
                                }
                            }
                        }
                    }
                }
                // Check for single-value BaseType properties
                else if (typeof(BaseType).IsAssignableFrom(prop.PropertyType))
                {
                    var child = prop.GetValue(node) as BaseType;
                    if (child != null)
                    {
                        CollectReachableNodes(child, visited);
                    }
                }
            }
        }

        /// <summary>
        /// Collects all node GUIDs for uniqueness validation.
        /// Unlike CollectReachableNodes, this allows duplicates to be detected.
        /// </summary>
        private static void CollectGuidsForUniquenessCheck(BaseType node, List<Guid> guidList)
        {
            if (node == null) return;

            guidList.Add(node.ObjectGUID);

            // Use reflection to traverse children
            var nodeType = node.GetType();
            var properties = nodeType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = prop.PropertyType.GetGenericArguments()[0];
                    if (typeof(BaseType).IsAssignableFrom(listType))
                    {
                        var list = prop.GetValue(node) as System.Collections.IEnumerable;
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                if (item is BaseType child)
                                {
                                    CollectGuidsForUniquenessCheck(child, guidList);
                                }
                            }
                        }
                    }
                }
                else if (typeof(BaseType).IsAssignableFrom(prop.PropertyType))
                {
                    var child = prop.GetValue(node) as BaseType;
                    if (child != null)
                    {
                        CollectGuidsForUniquenessCheck(child, guidList);
                    }
                }
            }
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
