using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using FileExplorer.Utils;

namespace FileExplorer.BaseControls
{
    public class AutoSuggestSource : ISuggestSource
    {
        #region Constructor

        public AutoSuggestSource(string subentriesPath, string valuePath)
        {
            StringComparisonOption = StringComparison.CurrentCultureIgnoreCase;
            Separator = '\\';
            SubentriesPath = subentriesPath;
            ValuePath = valuePath;
        }

        
        #endregion

        #region Methods

        private string getValuePath(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return "";
            else return pathName.Substring(0, pathName.LastIndexOf(Separator));
        }
        private string getValueName(string pathName)
        {
            if (String.IsNullOrEmpty(pathName))
                return "";
            if (pathName.IndexOf(Separator) == -1)
                return pathName;
            else return pathName.Substring(pathName.LastIndexOf(Separator) + 1);
        }

        protected virtual IEnumerable getSubEntries(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, SubentriesPath) as IEnumerable;            
        }

        protected virtual string getValue(object item)
        {
            return PropertyPathHelper.GetValueFromPropertyInfo(item, ValuePath) as string;
        }

        private IEnumerable<object> list(object item)
        {
            var retVal = item is IEnumerable ? item as IEnumerable : getSubEntries(item);
            return retVal.Cast<object>();
        }


        private object lookup(object data, string path)
        {
            var queue = new Queue<string>(path.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries));
            object current = data;
            while (current != null && queue.Any())
            {
                var nextSegment = queue.Dequeue();
                object found = null;
                foreach (var item in list(current))
                {
                    string valuePathName = getValue(item);
                    string value = getValueName(valuePathName); //Value may be full path, or just current value.
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

        public Task<IList<object>> SuggestAsync(object data, string input)
        {
            string valuePath = getValuePath(input);
            string valueName = getValueName(input);
            if (String.IsNullOrEmpty(valueName) && input.EndsWith(Separator + ""))
                valueName += Separator;
            var found = lookup(data, valuePath);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                foreach (var item in list(found))
                {
                    string valuePathName = getValue(item) as string;
                    if (valuePathName.StartsWith(input, StringComparisonOption) &&
                        !valuePathName.Equals(input, StringComparisonOption))
                        retVal.Add(item);
                }
            }



            return Task.FromResult<IList<object>>(retVal);

        }
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties


        public char Separator { get; set; }
        public StringComparison StringComparisonOption { get; set; }
        public string ValuePath { get; set; }
        public string SubentriesPath { get; set; }

        
        #endregion
    }



    public class AutoSuggestSource<T> : AutoSuggestSource
    {
        #region Constructor
        public AutoSuggestSource(string subentriesPath, string valuePath)
            : base(subentriesPath, valuePath)
        {
            propInfoSubEntries = typeof(T).GetProperty(subentriesPath);
            propInfoValue = typeof(T).GetProperty(valuePath);
        }

        

        #endregion

        #region Methods

        protected override IEnumerable getSubEntries(object item)
        {
            return propInfoSubEntries.GetValue(item) as IEnumerable;
        }

        protected override string getValue(object item)
        {
            return propInfoValue.GetValue(item) as string;
        }

        #endregion

        #region Data

        PropertyInfo propInfoValue, propInfoSubEntries;

        #endregion

        #region Public Properties

        #endregion





    }
}
