using System;
using System.Collections.Generic;

namespace FeatureFlags.APIs.Models
{
    public class TreeNode<TValue> where TValue : class
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public TValue Value { get; set; }

        public ICollection<TreeNode<TValue>> Children { get; set; }

        public TreeNode()
        {
        }

        public TreeNode(int id, string name, TValue value, ICollection<TreeNode<TValue>> children = null)
        {
            if (id == 0)
            {
                throw new ArgumentException("tree id cannot be 0");
            }
            Id = id;

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("tree name cannot be null or whitespace");
            }
            Name = name;

            Value = value;
            Children = children ?? new List<TreeNode<TValue>>();
        }

        public TreeNode<TValue> Node(int nodeId)
        {
            return Node(this, nodeId);
        }

        private TreeNode<TValue> Node(TreeNode<TValue> treeNode, int nodeId)
        {
            if (nodeId == treeNode.Id)
            {
                return treeNode;
            }

            foreach (var child in treeNode.Children)
            {
                var node = Node(child, nodeId);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        public IEnumerable<TValue> Values()
        {
            var values = new List<TValue>();
            
            Values(this, values);

            return values;
        }

        private void Values(TreeNode<TValue> treeNode, ICollection<TValue> values)
        {
            values.Add(treeNode.Value);
            foreach (var child in treeNode.Children)
            {
                Values(child, values);
            }
        }

        public void Check()
        {
            var visitedNodeIds = new List<int>();

            Check(this, visitedNodeIds);
        }

        public void Check(TreeNode<TValue> treeNode, ICollection<int> visitedNodeIds)
        {
            var hasDuplicates = visitedNodeIds.Contains(treeNode.Id);
            if (hasDuplicates)
            {
                throw new ArgumentException($"tree has duplicate nodeId {treeNode.Id}");
            }

            visitedNodeIds.Add(treeNode.Id);
            foreach (var child in treeNode.Children)
            {
                Check(child, visitedNodeIds);
            }
        }
    }
}