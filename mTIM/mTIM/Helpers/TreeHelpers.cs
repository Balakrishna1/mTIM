using System;
using System.Collections.Generic;
using System.Linq;
using mTIM.Models;
using mTIM.Models.D;

namespace mTIM.Helpers
{
    internal static class TreeHelpers
    {
        /// <summary>
        /// To Build the tree from list.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IList<TimTaskModel> BuildTree(this IEnumerable<TimTaskModel> source)
        {
            var roots = source.Where(x => x.Level.Equals(0) && x.Parent.Equals(0)).ToList();

            if (roots.Count() > 0)
            {
                for (int i = 0; i < roots.Count(); i++)
                {
                    AddChildren(roots[i], source);
                }
            }

            return roots;
        }

        /// <summary>
        /// Add children to parent. 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="source"></param>
        private static void AddChildren(TimTaskModel node, IEnumerable<TimTaskModel> source)
        {
            if (source.Contains(node))
            {
                node.Children = source.Where(x => x.Parent.Equals(node.Id) && x.Level.Equals(node.Level + 1)).ToList();
                for (int i = 0; i < node.Children.Count; i++)
                {
                    AddChildren(node.Children[i], source);
                    AddAncestors(node.Children[i], source);
                }
            }
            else
            {
                node.Children = new List<TimTaskModel>();
            }
        }

        private static void AddAncestors(TimTaskModel node, IEnumerable<TimTaskModel> source)
        {
            if (source.Contains(node))
            {
                node.Ancestors = source.Where(x => x.Level.Equals(node.Level) && x.Parent.Equals(node.Parent)).ToList();
                for (int i = 0; i < node.Ancestors.Count; i++)
                    AddAncestors(node.Ancestors[i], source);
            }
            else
            {
                node.Ancestors = new List<TimTaskModel>();
            }
        }



        /// <summary>
        /// To Get the childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IList<TimTaskModel> GetChildrensFromParent(this TimTaskModel source)
        {
            return source.Children;
        }

        /// <summary>
        /// To Get the childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IList<TimTaskModel> GetParentFromChildren(this IEnumerable<TimTaskModel> source, int id, int level)
        {
            if(source != null)
            {
                foreach(var item in source)
                {
                    if(item.Id == id && item.Level == level)
                    {
                        return item.Children;
                    }else
                    {
                        GetParentFromChildren(item.Children, id, level);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// To get the list from root node.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<TimTaskModel> Descendants(this TimTaskModel root)
        {
            var nodes = new Stack<TimTaskModel>(new[] { root });
            while (nodes.Any())
            {
                TimTaskModel node = nodes.Pop();
                yield return node;
                foreach (var n in node.Children) nodes.Push(n);
            }
        }
    }
}