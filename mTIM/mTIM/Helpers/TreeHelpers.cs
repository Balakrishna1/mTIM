using System;
using System.Collections.Generic;
using System.Linq;
using mTIM.Enums;
using mTIM.Models;
using mTIM.Models.D;
using mTIM.ViewModels;

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
                    AddChildren(roots[i], source, roots[i].ProjectId);
                }
            }

            return roots;
        }

        /// <summary>
        /// To update the rootid to childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IList<TimTaskModel> UpdateList(this IEnumerable<TimTaskModel> source)
        {
            source.ToList().ForEach(x => x.LoadValues());
            var roots = source.Where(x => x.Path.EndsWith("/Prj")).ToList();

            if (roots.Count() > 0)
            {
                for (int i = 0; i < roots.Count(); i++)
                {
                    roots[i].ProjectId = roots[i].Id;
                    UpdateRootIdToChildren(roots[i], source, roots[i].Id);
                }
            }

            return roots;
        }



        /// <summary>
        /// Add children to parent. 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="source"></param>
        private static void UpdateRootIdToChildren(TimTaskModel node, IEnumerable<TimTaskModel> source, int parentId)
        {
            if (source.Contains(node))
            {
                node.Childrens = source.Where(x => x.Parent.Equals(node.Id) && x.Level.Equals(node.Level + 1));
                for (int i = 0; i < node.Childrens.Count(); i++)
                {
                    node.Childrens.ElementAt(i).ProjectId = parentId;
                    UpdateRootIdToChildren(node.Childrens.ElementAt(i), source, parentId);
                }
            }
            else
            {
                node.Childrens = new List<TimTaskModel>();
            }
        }



        /// <summary>
        /// Add children to parent. 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="source"></param>
        private static void AddChildren(TimTaskModel node, IEnumerable<TimTaskModel> source, int parentId)
        {
            if (source.Contains(node))
            {
                node.Childrens = source.Where(x => x.Parent.Equals(node.Id) && x.Level.Equals(node.Level + 1));
                for (int i = 0; i < node.Childrens.Count(); i++)
                {
                    node.Childrens.ElementAt(i).ProjectId = parentId;
                    AddChildren(node.Childrens.ElementAt(i), source, parentId);
                    //AddAncestors(node.Childrens.ElementAt(i), source, rootId);
                }
            }
            else
            {
                node.Childrens = new List<TimTaskModel>();
            }
        }

        //private static void AddAncestors(TimTaskModel node, IEnumerable<TimTaskModel> source, int rootId)
        //{
        //    if (source.Contains(node))
        //    {
        //        node.Ancestors = source.Where(x => x.Level.Equals(node.Level) && x.Parent.Equals(node.Parent));
        //        for (int i = 0; i < node.Ancestors.Count(); i++)
        //            AddAncestors(node.Ancestors.ElementAt(i), source, rootId);
        //    }
        //    else
        //    {
        //        node.Ancestors = new List<TimTaskModel>();
        //    }
        //}



        /// <summary>
        /// To Get the childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TimTaskModel> GetChildrensFromParent(this TimTaskModel source)
        {
            return source.Childrens;
        }

        /// <summary>
        /// To Get the childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TimTaskModel> GetParentFromChildren(this IEnumerable<TimTaskModel> source, int id, int level)
        {
            if(source != null)
            {
                foreach(var item in source)
                {
                    if(item.Id == id && item.Level == level)
                    {
                        return item.Childrens;
                    }else
                    {
                        GetParentFromChildren(item.Childrens, id, level);
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
                foreach (var n in node.Childrens) nodes.Push(n);
            }
        }
    }
}