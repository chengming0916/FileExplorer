using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;

namespace FileExplorer.UserControls
{
    public class AutoHierarchyHelper : IHierarchyHelper
    {
        #region Constructor

        public AutoHierarchyHelper(string parentPath, string valuePath, string subEntriesPath)
        {
            ParentPath = parentPath;
            ValuePath = valuePath;
            SubentriesPath = subEntriesPath;
        }
        
        #endregion

        #region Methods

        #region Utils Func - extractPath/Name
        private string extractPath(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return "";
            else return pathName.Substring(0, pathName.LastIndexOf(Separator));
        }
        private string extractName(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return pathName;
            else return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
        }
        #endregion

        #region Overridable to improve speed.

        protected virtual object getParent(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, ParentPath);
        }

        protected virtual string getValuePath(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, ValuePath) as string;
        }

        protected virtual IEnumerable getSubEntries(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, SubentriesPath) as IEnumerable;
        }

        #endregion

        #region Implements

        public IEnumerable<object> GetHierarchy(object item, bool includeCurrent)
        {
            if (includeCurrent)
                yield return item;

            var current = getParent(item);
            while (current != null)
            {
                yield return current;
                current = getParent(current);
            }
        }

        public string GetPath(object item)
        {
            return getValuePath(item);
        }

        public object GetItem(object rootItem, string path)
        {
            Func<object, IEnumerable> list = (item) =>
            {
                return item is IEnumerable ? item as IEnumerable : getSubEntries(item);
            };

            var queue = new Queue<string>(path.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries));
            object current = rootItem;
            while (current != null && queue.Any())
            {
                var nextSegment = queue.Dequeue();
                object found = null;
                foreach (var item in list(current))
                {
                    string valuePathName = getValuePath(item);
                    string value = extractName(valuePathName); //Value may be full path, or just current value.
                    if (value.Equals(nextSegment, StringComparisonOption))
                    {
                        found = item;
                        break;
                    }
                }
                current = found;
            }
            return current;
        }

        #endregion

        #endregion

        #region Data

        static char _separator = '\\';
        static StringComparison _strComp = StringComparison.CurrentCultureIgnoreCase;

        #endregion

        #region Public Properties

        public static char Separator { get { return _separator; } set { _separator = value; } }
        public static StringComparison StringComparisonOption { get { return _strComp; } set { _strComp = value; } }
        public string ParentPath { get; set; }
        public string ValuePath { get; set; }
        public string SubentriesPath { get; set; }

        #endregion



        
    }
}
