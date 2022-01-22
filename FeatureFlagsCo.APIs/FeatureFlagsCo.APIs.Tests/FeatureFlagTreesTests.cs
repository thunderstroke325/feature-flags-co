using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.APIs.Models;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class FeatureFlagTreesTests
    {
        private readonly FeatureFlagTagTree _tree = new FeatureFlagTagTree
        {
            Id = 1,
            Name = "root",
            Value = new List<string>(),
            Children = new List<TreeNode<IEnumerable<string>>>
            {
                new TreeNode<IEnumerable<string>>(2, "child 1", new List<string> {"ff-id-1", "ff-id-2"}),
                new TreeNode<IEnumerable<string>>(3, "child 2", new List<string> {"ff-id-1", "ff-id-3"}),
                new TreeNode<IEnumerable<string>>(4, "child 3", Enumerable.Empty<string>(), new List<TreeNode<IEnumerable<string>>>
                {
                    new TreeNode<IEnumerable<string>>(5, "child 3-1", new List<string> {"ff-id-3", "ff-id-4"})
                })
            }
        };
        
        [Fact]
        public void Should_Get_Feature_Flag_Tags()
        {
            string.Join(",", _tree.FlagTags("ff-id-1")).ShouldBe("child 1,child 2");
            string.Join(",", _tree.FlagTags("ff-id-4")).ShouldBe("child 3-1");
            string.Join(",", _tree.FlagTags("ff-id-3")).ShouldBe("child 2,child 3-1");
        }

        [Fact]
        public void Should_Check_Trees_Is_Invalid_Or_Not()
        {
            var anotherTree = new FeatureFlagTagTree
            {
                Id = 1,
                Name = "node id 1 has been used in _tree",
                Value = new List<string>(),
                Children = new List<TreeNode<IEnumerable<string>>>()
            };

            var invalid = new FeatureFlagTagTrees(1, new[] { _tree, anotherTree });
            
            var valid1 = new FeatureFlagTagTrees(1, new[] { _tree });
            var valid2 = new FeatureFlagTagTrees(1, new[] { anotherTree });

            Should.NotThrow(() => valid1.Check());
            Should.NotThrow(() => valid2.Check());
            
            Should.Throw<ArgumentException>(() => invalid.Check());
        }
    }
}