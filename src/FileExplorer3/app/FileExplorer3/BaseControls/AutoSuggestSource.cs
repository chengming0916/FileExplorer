using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public AutoSuggestSource(object rootData, string subentriesPath, string valuePath)
            : this ()
        {
            SubentriesPath = subentriesPath;
            RootItem = rootData;
            ValuePath = valuePath;
            
        }

        public AutoSuggestSource()
        {
            StringComparisonOption = StringComparison.CurrentCultureIgnoreCase;
            Separator = '\\';
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

        private IEnumerable<object> list(object item)
        {
            var retVal = item is IEnumerable ? item as IEnumerable :
                    PropertyPathHelper.GetValue(item, SubentriesPath) as IEnumerable;
            return retVal.Cast<object>();
        }

        
        private object lookup(string path)
        {
            var queue = new Queue<string>(path.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries));
            object current = RootItem;
            while (current != null && queue.Any())
            {
                var nextSegment = queue.Dequeue();
                object found = null;
                foreach (var item in list(current))
                {
                    string valuePathName = PropertyPathHelper.GetValue(item, ValuePath) as string;
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

        public Task<IList<object>> SuggestAsync(string input)
        { 
            string valuePath = getValuePath(input);
            string valueName = getValueName(input);
            if (String.IsNullOrEmpty(valueName) && input.EndsWith(Separator + ""))
                valueName += Separator;
            var found = lookup(valuePath);
            List<object> retVal = new List<object>();

            if (found != null)
            {
                foreach (var item in list(found))
                {
                    string valuePathName = PropertyPathHelper.GetValue(item, ValuePath) as string;
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
        public object RootItem { get; set; }
        public string SubentriesPath { get; set; }

        #endregion





    }
}
