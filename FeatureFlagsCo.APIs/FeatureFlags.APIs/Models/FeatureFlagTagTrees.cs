using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.APIs.Services.MongoDb;

namespace FeatureFlags.APIs.Models
{
    public class FeatureFlagTagTrees : MongoDbObjectIdEntity
    {
        public int EnvId { get; protected set; }

        public ICollection<FeatureFlagTagTree> Trees { get; protected set; }

        public FeatureFlagTagTrees(int envId, ICollection<FeatureFlagTagTree> trees)
        {
            if (envId == 0)
            {
                throw new ArgumentException("envId cannot be 0");
            }
            EnvId = envId;

            Trees = trees ?? new List<FeatureFlagTagTree>();
            
            // check if is valid
            Check();
        }

        public void Update(List<FeatureFlagTagTree> trees)
        {
            Trees = trees ?? new List<FeatureFlagTagTree>();
            
            // check if is valid
            Check();
        }

        public TreeNode<IEnumerable<string>> Node(int tagId)
        {
            foreach (var tagTree in Trees)
            {
                var node = tagTree.Node(tagId);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        public List<string> FlagTags(string flagId)
        {
            var tags = new List<string>();
            
            foreach (var tagTree in Trees)
            {
                tags.AddRange(tagTree.FlagTags(flagId));
            }
            
            return tags;
        }

        public void Check()
        {
            var visitedIds = new List<int>();
            foreach (var tree in Trees)
            {
                tree.Check(tree, visitedIds);
            }
        }
    }

    public class FeatureFlagTagTree : TreeNode<IEnumerable<string>>
    {
        public IEnumerable<string> FlagTags(string flagId)
        {
            var tags = new List<string>();

            FlagTags(this, flagId, tags);

            return tags;
        }

        private void FlagTags(
            TreeNode<IEnumerable<string>> tree,
            string flagId,
            ICollection<string> tags)
        {
            if (tree.Value != null && tree.Value.Contains(flagId))
            {
                tags.Add(tree.Name);
            }

            foreach (var child in tree.Children)
            {
                FlagTags(child, flagId, tags);
            }
        }
    }
}