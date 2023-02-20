using System;
using System.Collections.Generic;
using System.Linq;
using mTIM.Models;
using mTIM.Models.D;
using Newtonsoft.Json;

namespace mTIM.Helpers
{
    public static class TimTaskListHelper
    {
        private static List<TimTaskModel> Source { get; set; }
        public static IList<TimTaskModel> ProjectList { get; set; }
        private static StatusConfiguration Configuration { get; set; }
        private static CurrentElementsState CurrentElementsState { get; set; }

        /// <summary>
        /// To Build the tree from list.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IList<TimTaskModel> BuildTree(this IEnumerable<TimTaskModel> source)
        {
            Source = source.ToList();
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
            Source = source.ToList();
            var roots = source.Where(x => x.Path.EndsWith("/Prj")).ToList();
            ProjectList = roots;
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
                var childrens = source.Where(x => x.Parent.Equals(node.Id) && x.Level.Equals(node.Level + 1));
                if (childrens?.Count() > 0)
                {
                    if (childrens.First().ShowInList)
                        node.ShowInLineList = true;
                    node.Childrens = childrens;
                    for (int i = 0; i < node.Childrens.Count(); i++)
                    {
                        node.Childrens.ElementAt(i).ProjectId = parentId;
                        UpdateRootIdToChildren(node.Childrens.ElementAt(i), source, parentId);
                    }
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

        public static bool IsParent(int id)
        {
            bool isParent = true;
            if (id > 1)
            {
                isParent = ProjectList?.Where(x => x.Id == id).ToList()?.Count > 0;
            }
            return isParent;
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
        /// Get Item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TimTaskModel GetItem(int id)
        {
            return Source?.Where(x => x.Id.Equals(id)).FirstOrDefault();
        }



        /// <summary>
        /// To Get the childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TimTaskModel> GetChildrens(this TimTaskModel source)
        {
            return source.Childrens;
        }

        /// <summary>
        /// To Get the childrens.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<TimTaskModel> GetParentsFromChildren(int id, int level)
        {
            return Source.Where(x => x.Level.Equals(level) && x.Parent.Equals(id));
        }

        public static IList<TimTaskModel> GetPrecastElements()
        {
            return Source?.Where(x => x.ObjectId != "00000000-0000-0000-0000-000000000000").ToList();
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

        public static IList<TimTaskModel> GetTotalList()
        {
            return Source;
        }

        public static void UpdateStateInfo(string stateJson)
        {
            if (!string.IsNullOrEmpty(stateJson))
            {
                var stateInformation = JsonConvert.DeserializeObject<StateInformation>(stateJson);
                if (stateInformation != null)
                {
                    Configuration = stateInformation.Configuration;
                    CurrentElementsState = stateInformation.State;
                    UpdateElementStates();
                    RecalcStatus();
                }
            }
        }

        private static void UpdateElementStates()
        {
            Source?.ForEach(x => x.Conditions.Clear());
            if (CurrentElementsState != null && CurrentElementsState.Elements?.Count() > 0)
            {
                foreach (var element in CurrentElementsState.Elements)
                {
                    var taskData = Source.Where(x => x.ObjectId.Equals(element.ObjectID)).FirstOrDefault();
                    if (taskData != null)
                    {
                        taskData.Conditions = taskData.Conditions;
                    }
                }
            }
        }

        private static void RecalcStatus()
        {
            for (int i = 0; i < Source.Count(); i++)
                Source[i].RecalcStatus(Configuration);
        }
    }
}