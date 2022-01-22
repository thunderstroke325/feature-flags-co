using System;
using System.Collections.Generic;
using System.Linq;
using FeatureFlags.APIs.Models;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class TreeTests
    {
        private readonly TreeNode<string> _tree = new TreeNode<string>(1, "1", "", new List<TreeNode<string>>
        {
            new TreeNode<string>(2, "1-1", "ff-id-1"),
            new TreeNode<string>(3, "1-2", "ff-id-2"),
            new TreeNode<string>(4, "1-3", "", new List<TreeNode<string>>
            {
                new TreeNode<string>(5, "1-3-1", "ff-id-3")
            })
        });

        [Fact]
        public void Should_Get_Sub_Tree()
        {
            // 1-3 sub tree
            var subTree = _tree.Node(4);
            subTree.Name.ShouldBe("1-3");
            subTree.Children.Count.ShouldBe(1);
            subTree.Children.ElementAt(0).Name.ShouldBe("1-3-1");
        }

        [Fact]
        public void Should_Check_Tree_Is_Valid_Or_Not()
        {
            // tree cannot have duplicate node id
            var invalidTree = new TreeNode<string>(1, "1", "", new List<TreeNode<string>>
            {
                new TreeNode<string>(2, "1-1", "ff-id-1", new List<TreeNode<string>>
                {
                    new TreeNode<string>(1, "1-1-1-duplicate", "ff-id-x")
                })
            });

            Should.NotThrow(() => _tree.Check());
            Should.Throw<ArgumentException>(() => invalidTree.Check());
        }

        [Fact]
        public void Should_Get_Tree_Values()
        {
            _tree.Values().ShouldBe(new[] { "", "ff-id-1", "ff-id-2", "", "ff-id-3" });
            
            // subtree values
            _tree.Node(4).Values().ShouldBe(new[] { "", "ff-id-3" });
        }
    }
}